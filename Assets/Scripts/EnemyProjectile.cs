using UnityEngine;

// Projectile fired by ranged enemies. Targets the player (anything with a
// Damageable component) and despawns on first impact or after maxLife.
//
// Deliberately simpler than Projectile.cs:
//   - No bouncing (one-and-done).
//   - No layer mask: it'll damage anything with a Damageable; for a player-only
//     setup that's just the player. (Enemies don't have Damageable; they have
//     Enemy with its own TakeDamage signature.)
//   - Pause-aware, matching Projectile.cs.
//   - Pool-aware via IPoolable.
public class EnemyProjectile : MonoBehaviour, IPoolable
{
    Rigidbody rb;
    Vector3 previousVelocity;

    [Header("Attributes")]
    [SerializeField] float speed = 8f;
    [SerializeField] float maxLife = 3f;
    [SerializeField] float damage = 8f;

    float timeAlive;

    LevelManager levelManager;
    EventManagerSO eventManager;
    PrefabPool _pool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        levelManager = FindAnyObjectByType<LevelManager>();
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }

    public void OnTakenFromPool(PrefabPool pool)
    {
        _pool = pool;
        ResetForFlight();
        LaunchForward();
    }

    private void Start()
    {
        // Non-pooled path. If pooled, OnTakenFromPool fires LaunchForward instead.
        if (_pool == null) LaunchForward();
    }

    private void OnEnable()
    {
        if (eventManager == null) return;
        eventManager.onGamePaused += Pause;
        eventManager.onGameResumed += Resume;
    }

    private void OnDisable()
    {
        if (eventManager == null) return;
        eventManager.onGamePaused -= Pause;
        eventManager.onGameResumed -= Resume;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore other enemies — the projectile spawns 0.6u in front of the
        // shooter's chest, which may overlap the shooter's own collider for a
        // frame; without this we'd despawn instantly and never hit the player.
        // Also nice side effect: ranged enemies can't friendly-fire each other.
        if (other.GetComponentInParent<Enemy>() != null) return;

        if (other.TryGetComponent<Damageable>(out Damageable target))
        {
            target.ReceiveDamage(damage);
        }
        // Despawn on any other collision (walls, player, etc.) so the shot
        // doesn't sail through geometry.
        Despawn();
    }

    private void Update()
    {
        if (levelManager != null && levelManager.CurrentGameState == GameState.paused) return;

        timeAlive += Time.deltaTime;
        if (timeAlive > maxLife)
        {
            Despawn();
            return;
        }

        if (rb != null) previousVelocity = rb.linearVelocity;
    }

    private void Pause()
    {
        if (rb != null) rb.isKinematic = true;
    }

    private void Resume()
    {
        if (rb == null) return;
        rb.isKinematic = false;
        rb.linearVelocity = previousVelocity;
    }

    private void ResetForFlight()
    {
        timeAlive = 0f;
        previousVelocity = Vector3.zero;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void LaunchForward()
    {
        if (rb == null) return;
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
    }

    private void Despawn()
    {
        if (_pool != null) _pool.Release(gameObject);
        else Destroy(gameObject);
    }
}
