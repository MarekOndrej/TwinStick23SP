using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    [Header("General settings")]
    [SerializeField] Transform chaseTarget;
    [SerializeField] float aggroDistance = 8f;
    NavMeshAgent agent;

    [Header("Health related")]
    [SerializeField] HealthBar healthBar;
    [SerializeField] float maxHealth = 50f;
    [SerializeField] int scoreValue = 10;
    float currentHealth;
    EventManagerSO eventManager;

    [Header("Hit feedback")]
    [Tooltip("Color the enemy flashes to when hit.")]
    [SerializeField] Color hitFlashColor = new Color(1f, 0.92f, 0.3f, 1f);
    [Tooltip("How long the flash lasts before reverting.")]
    [SerializeField] float hitFlashDuration = 0.08f;
    [Tooltip("Optional VFX prefab spawned at the enemy's position when it dies. " +
             "Leave empty to load 'Assets/Resources/DeathPuff.prefab' automatically.")]
    [SerializeField] GameObject deathVfxPrefab;
    [Tooltip("How long the knockback lerp takes (seconds).")]
    [SerializeField] float knockbackDuration = 0.12f;

    [Header("Out-of-bounds self-destruct")]
    [Tooltip("If the enemy is knocked off the navmesh OR falls below this Y, " +
             "it self-destructs after the timeout (and counts as a kill so " +
             "the wave can clear).")]
    [SerializeField] float outOfBoundsYThreshold = -3f;
    [SerializeField] float outOfBoundsTimeout = 1.5f;
    [Tooltip("How long after spawn the OOB check is suppressed. Gives the " +
             "NavMeshAgent time to attach to the navmesh before we'd otherwise " +
             "interpret 'not attached yet' as 'fell off the arena'.")]
    [SerializeField] float outOfBoundsGracePeriod = 1.5f;
    [Tooltip("How close the enemy has to be to a navmesh surface to be " +
             "considered 'on the arena'. Anything further is OOB.")]
    [SerializeField] float outOfBoundsSampleRadius = 1.5f;
    float _oobTimer;
    float _spawnedAt;

    Coroutine _knockbackRoutine;

    Renderer[] _renderers;
    MaterialPropertyBlock _propertyBlock;
    Color[] _originalColors;
    Coroutine _flashRoutine;
    static readonly int _BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int _ColorID = Shader.PropertyToID("_Color");

    [Header("Damage related")]
    [SerializeField] float damageAmount = 5f;
    [SerializeField] float damageDelay = 1f;
    [SerializeField] float meleeRange = 2f;
    float damageTimer;
    Damageable playerDamageableComponent;

    [Header("Ranged attack (only for shooter enemies)")]
    [Tooltip("If true, this enemy fires projectiles at the player at close-ish " +
             "range instead of melee-attacking.")]
    [SerializeField] bool useRangedAttack = false;
    [Tooltip("Projectile prefab to instantiate when shooting. If null, falls " +
             "back to Resources/EnemyProjectile.prefab.")]
    [SerializeField] GameObject rangedProjectilePrefab;
    [Tooltip("Local-space offset where the projectile spawns (relative to enemy).")]
    [SerializeField] Vector3 rangedFiringOffset = new Vector3(0f, 1f, 0.5f);
    [Tooltip("Maximum distance at which this enemy will choose to shoot " +
             "(it'll close in if the player is farther).")]
    [SerializeField] float rangedAttackRange = 5f;
    [Tooltip("Stop and shoot when within this many units (avoids walking into " +
             "the player to melee them). Should be < rangedAttackRange.")]
    [SerializeField] float rangedStopDistance = 3.5f;
    [Tooltip("Seconds between successive shots.")]
    [SerializeField] float rangedAttackInterval = 1.4f;
    float _rangedAttackTimer;

    [Header("Roaming")]
    [SerializeField] float roamRadius = 8f;
    [SerializeField] float roamMinAcceptableDistance = 1.2f;
    [SerializeField] float roamMinMoveDistance = 3f;
    [SerializeField] float roamWaitTime = 1.5f;

    float roamWaitTimer = 0f;
    bool hasRoamTarget = false;


    private LevelManager levelManager;

    private enum EnemyMode
    {
        stopped,
        roaming,
        chasing
    }

    private EnemyMode currentEnemyMode;

    private void Awake()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        eventManager = Resources.Load<EventManagerSO>("EventManager");

        agent = GetComponent<NavMeshAgent>();

        _spawnedAt = Time.time;

        // set current health to max
        currentHealth = maxHealth;

        // disable health bar
        healthBar.gameObject.SetActive(false);

        // Cache renderers for hit-flash, and remember original colors so we
        // can restore them after the flash. Uses MaterialPropertyBlock to
        // avoid creating per-instance material clones (preserves batching).
        InitHitFlash();

        // Fallback: if no death VFX was assigned in the prefab, try to load one
        // from Resources so a single asset can be shared by all enemies.
        if (deathVfxPrefab == null)
        {
            deathVfxPrefab = Resources.Load<GameObject>("DeathPuff");
        }

        // Same fallback for the ranged projectile so ranged enemies don't need
        // an inspector reference if they're happy with the default visual.
        if (useRangedAttack && rangedProjectilePrefab == null)
        {
            rangedProjectilePrefab = Resources.Load<GameObject>("EnemyProjectile");
        }

        // Enemies are ready to attack as soon as they are born
        damageTimer = damageDelay;

        // Start enemy paused
        currentEnemyMode = EnemyMode.stopped;
    }

    private void Update()
    {

        // If game is paused....
        if (levelManager.CurrentGameState == GameState.paused)
        {
            agent.isStopped = true;
            return; // nothing else to do here
        }

        // Out-of-bounds: if the enemy got knocked off the arena (or fell into
        // a pit), the NavMeshAgent won't apply gravity so they'd hang in mid-air
        // forever. Worse, _aliveThisWave on the spawner never decrements and
        // the next wave never starts. Self-destruct + raise EnemyDefeated so
        // the wave clears cleanly. Knockback routine sets a brief grace period
        // by not running this check (it's paused via early return below).
        if (IsOutOfBounds())
        {
            _oobTimer += Time.deltaTime;
            if (_oobTimer >= outOfBoundsTimeout)
            {
                KillSelf();
                return;
            }
        }
        else
        {
            _oobTimer = 0f;
        }

        // Respond to game state
        switch (levelManager.CurrentGameState)
        {
            case GameState.running:
                if (chaseTarget != null && Vector3.Distance(transform.position, chaseTarget.position) < aggroDistance)
                {
                    // change enemy mode to chasing
                    agent.isStopped = false;
                    SetEnemyMode(EnemyMode.chasing);
                    ChaseAndAttack();
                    break;
                }

                // change enemy mode to stopped
                SetEnemyMode(EnemyMode.roaming);
                HandleRandomRoaming();
                break;

            case GameState.gameOver:
                // change enemy mode to stopped
                SetEnemyMode(EnemyMode.roaming);
                HandleRandomRoaming();
                break;

            default:
                // change enemy mode to stopped
                SetEnemyMode(EnemyMode.stopped);
                agent.isStopped = true;
                break;
        }

    }

    private void SetEnemyMode(EnemyMode changedEnemyMode)
    {
        // if we are already in this mode, then do nothing
        if (currentEnemyMode == changedEnemyMode)
            return;

        currentEnemyMode = changedEnemyMode;

        switch (currentEnemyMode)
        {
            case EnemyMode.chasing:
                Debug.Log($"{name}: I'm going to chase!");
                hasRoamTarget = false;
                roamWaitTimer = 0f;
                break;

            case EnemyMode.roaming:
                Debug.Log($"{name}: I'm going to roam!");
                hasRoamTarget = false;
                roamWaitTimer = 0f;
                agent.ResetPath();
                break;

            case EnemyMode.stopped:
                Debug.Log($"{name}: I'm going to stay put!");
                hasRoamTarget = false;
                agent.ResetPath();
                agent.isStopped = true;
                break;
        }
    }

    private void HandleRandomRoaming()
    {
        agent.isStopped = false;

        // Do we already have a destination picked?
        // If not...
        if (!hasRoamTarget)
        {
            roamWaitTimer -= Time.deltaTime;

            // Is the wait over?
            if (roamWaitTimer <= 0f)
            {
                // Try to find a random point nearby on navmesh
                if (TryGetRandomNavmeshPoint(transform.position, roamRadius, out Vector3 randomPoint))
                {
                    agent.SetDestination(randomPoint);
                    hasRoamTarget = true;
                }
                else
                {
                    // Try again shortly if no good roam point was found
                    roamWaitTimer = 0.5f;
                }
            }
            return;
        }

        // The path is not ready yet
        if (agent.pathPending)
            return;

        // Invalid path? Give up and try a new one soon
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            hasRoamTarget = false;
            roamWaitTimer = 0.5f;
            return;
        }

        // Reached destination
        if (agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, roamMinAcceptableDistance))
        {
            // Optional extra check so arrival feels more reliable
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
            {
                hasRoamTarget = false;
                roamWaitTimer = roamWaitTime;
            }
        }
    }


    private bool TryGetRandomNavmeshPoint(Vector3 center, float radius, out Vector3 result)
    {
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < 15; i++)
        {
            Vector2 random2D = Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(random2D.x, 0f, random2D.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                Vector3 flatOffset = hit.position - transform.position;
                flatOffset.y = 0f;

                // Reject points that are too close to current position
                if (flatOffset.sqrMagnitude < roamMinMoveDistance * roamMinMoveDistance)
                    continue;

                // Reject points that do not have a complete path
                if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        result = hit.position;
                        return true;
                    }
                }
            }
        }

        result = center;
        return false;
    }

    private void ChaseAndAttack()
    {
        if (useRangedAttack && rangedProjectilePrefab != null)
        {
            ChaseAndShoot();
        }
        else
        {
            ChaseAndMelee();
        }
    }

    private void ChaseAndMelee()
    {
        // Make sure agent is mobile
        agent.isStopped = false;

        // Update agent's destination
        agent.destination = chaseTarget.position;


        // Add time to the timer
        damageTimer += Time.deltaTime;

        // Can we attack yet??
        if (damageTimer >= damageDelay)
        {
            // Are we close to the player
            if (Vector3.Distance(transform.position, chaseTarget.position) < meleeRange)
            {
                if (playerDamageableComponent)
                {
                    Debug.Log("I am dealing damage!");
                    playerDamageableComponent.ReceiveDamage(damageAmount);
                }
            }
            // Reset timer
            damageTimer = 0;
        }
    }

    private void ChaseAndShoot()
    {
        float distance = Vector3.Distance(transform.position, chaseTarget.position);

        if (distance > rangedAttackRange)
        {
            // Out of shooting range — close in.
            agent.isStopped = false;
            agent.destination = chaseTarget.position;
            return;
        }

        // In shooting range. Stop and face the target, then fire on cadence.
        agent.isStopped = true;

        // Stop short of the stop distance so we don't walk into the player.
        if (distance > rangedStopDistance)
        {
            agent.isStopped = false;
            agent.destination = chaseTarget.position;
        }

        // Face the player.
        Vector3 toTarget = chaseTarget.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(toTarget);
        }

        _rangedAttackTimer += Time.deltaTime;
        if (_rangedAttackTimer >= rangedAttackInterval)
        {
            _rangedAttackTimer = 0f;
            FireRangedProjectile();
        }
    }

    private void FireRangedProjectile()
    {
        if (rangedProjectilePrefab == null) return;

        // Spawn the projectile at the firing offset, oriented at the player.
        Vector3 spawnPos = transform.position
            + transform.right * rangedFiringOffset.x
            + Vector3.up * rangedFiringOffset.y
            + transform.forward * rangedFiringOffset.z;

        Quaternion rot = transform.rotation;
        if (chaseTarget != null)
        {
            Vector3 aim = chaseTarget.position - spawnPos;
            aim.y = 0f;
            if (aim.sqrMagnitude > 0.0001f) rot = Quaternion.LookRotation(aim);
        }

        // Pool the projectile so a wave of ranged enemies firing every 1.4s
        // doesn't churn GC. Falls back to Instantiate if pool unavailable.
        var pool = PrefabPool.For(rangedProjectilePrefab);
        if (pool != null) pool.Get(spawnPos, rot);
        else Instantiate(rangedProjectilePrefab, spawnPos, rot);
    }

    public void SetChaseTarget(Transform newChaseTarget)
    {
        // Update chase target
        chaseTarget = newChaseTarget;

        // Try to get the damageable component
        if (chaseTarget == null) return;

        if (chaseTarget.gameObject.TryGetComponent<Damageable>(out Damageable foundDamageable))
        {
            Debug.Log("I was able to find damageable component!");
            playerDamageableComponent = foundDamageable;
        }
    }


    public void TakeDamage(float incomingDamage)
    {
        // Reduce damage from our current health
        currentHealth -= incomingDamage;

        // Brief color flash to register the hit visually
        Flash();

        // If health drops below zero...
        if (currentHealth <= 0)
        {
            KillSelf();
            return;
        }

        // If health bar is disabled, enable it
        if (healthBar != null && !healthBar.gameObject.activeSelf)
        {
            healthBar.gameObject.SetActive(true);
        }

        // Update health bar
        if (healthBar != null)
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    // === Hit feedback ===

    private void InitHitFlash()
    {
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: false);
        _propertyBlock = new MaterialPropertyBlock();
        _originalColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            var mat = _renderers[i].sharedMaterial;
            if (mat == null) { _originalColors[i] = Color.white; continue; }

            if (mat.HasProperty(_BaseColorID))
                _originalColors[i] = mat.GetColor(_BaseColorID);
            else if (mat.HasProperty(_ColorID))
                _originalColors[i] = mat.GetColor(_ColorID);
            else
                _originalColors[i] = Color.white;
        }
    }

    private void Flash()
    {
        if (_renderers == null || _renderers.Length == 0) return;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        ApplyColor(hitFlashColor, useOverride: true);
        yield return new WaitForSeconds(hitFlashDuration);
        ApplyColor(default, useOverride: false); // restore originals
        _flashRoutine = null;
    }

    private void ApplyColor(Color overrideColor, bool useOverride)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(_propertyBlock);
            Color c = useOverride ? overrideColor : _originalColors[i];

            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(_BaseColorID))
                _propertyBlock.SetColor(_BaseColorID, c);
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(_ColorID))
                _propertyBlock.SetColor(_ColorID, c);

            r.SetPropertyBlock(_propertyBlock);
        }
    }

    private void SpawnDeathVfx()
    {
        if (deathVfxPrefab == null) return;
        // Spawn slightly above ground so the puff isn't half buried.
        var pos = transform.position + Vector3.up * 0.5f;
        Instantiate(deathVfxPrefab, pos, Quaternion.identity);
    }

    // Common death path. Used by both the damage->zero branch and the
    // out-of-bounds self-destruct branch so the wave-tracking event always
    // fires (and the death VFX always plays).
    private void KillSelf()
    {
        if (eventManager != null) eventManager.EnemyDefeated(scoreValue);
        SpawnDeathVfx();
        Destroy(this.gameObject);
    }

    // True if the enemy fell below the world floor OR ended up off the navmesh
    // (e.g. after a knockback pushed them off a platform). Either way they're
    // not coming back, so we should remove them.
    //
    // Notes on the navmesh check:
    //   - A spawn grace period suppresses the check entirely so the agent has
    //     time to attach to the navmesh after Awake.
    //   - We use NavMesh.SamplePosition (with a tolerant radius) rather than
    //     agent.isOnNavMesh because the latter can return false transiently
    //     during knockback Warp() or right after instantiation, even when the
    //     enemy is geographically on the arena.
    private bool IsOutOfBounds()
    {
        if (Time.time - _spawnedAt < outOfBoundsGracePeriod) return false;

        if (transform.position.y < outOfBoundsYThreshold) return true;

        if (agent != null && agent.enabled)
        {
            if (!NavMesh.SamplePosition(transform.position, out _,
                outOfBoundsSampleRadius, NavMesh.AllAreas))
            {
                return true;
            }
        }
        return false;
    }

    // === Knockback ===

    public void ApplyKnockback(Vector3 worldDirection, float distance)
    {
        if (this == null || !gameObject.activeInHierarchy) return;
        if (distance <= 0f) return;

        // Ignore vertical component — we're a NavMesh agent on flat ground.
        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.0001f) return;

        if (_knockbackRoutine != null) StopCoroutine(_knockbackRoutine);
        _knockbackRoutine = StartCoroutine(KnockbackRoutine(worldDirection.normalized, distance));
    }

    private IEnumerator KnockbackRoutine(Vector3 dir, float distance)
    {
        // Pause NavMeshAgent driving the transform; we'll move it ourselves.
        bool hadAgent = agent != null && agent.enabled;
        if (hadAgent)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dir * distance;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / knockbackDuration);
            // Ease-out cubic: fast at the start, decelerates to a stop.
            float eased = 1f - Mathf.Pow(1f - k, 3f);
            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        // Re-anchor the NavMeshAgent at the new position so its internal state
        // doesn't drag us back along the old path.
        if (hadAgent)
        {
            if (agent.isOnNavMesh) agent.Warp(transform.position);
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.isStopped = false;
        }

        _knockbackRoutine = null;
    }

}
