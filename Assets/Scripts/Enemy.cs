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

    [Header("Damage related")]
    [SerializeField] float damageAmount = 5f;
    [SerializeField] float damageDelay = 1f;
    [SerializeField] float meleeRange = 2f;
    float damageTimer;
    Damageable playerDamageableComponent;

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

        // set current health to max
        currentHealth = maxHealth;

        // disable health bar
        healthBar.gameObject.SetActive(false);

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
        // TODO: Add knock back effect

        // Reduce damage from our current health
        currentHealth -= incomingDamage;

        // If health drops below zero...
        if (currentHealth <= 0)
        {
            if (eventManager != null) eventManager.EnemyDefeated(scoreValue);
            Destroy(this.gameObject); // enemy perishes
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

}
