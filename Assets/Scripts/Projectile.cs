using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{

    Rigidbody rb;

    Vector3 previousVelocity;

    [Header("Attributes")]
    [SerializeField] float velocity;
    [SerializeField] int maxBounce = 2;
    [SerializeField] float maxLife = 5f;
    [SerializeField] float damage = 2;
    [Tooltip("How far back the enemy is pushed when hit (world units).")]
    [SerializeField] float knockbackDistance = 0.8f;

    [SerializeField] LayerMask damageableLayers;

    float timeAlive = 0f;
    int bounceCounter = 0;

    LevelManager levelManager;
    EventManagerSO eventManager;
    PrefabPool _pool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        levelManager = FindAnyObjectByType<LevelManager>();
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }

    // Called by PrefabPool each time the projectile is fetched from the pool.
    // (Not called when instantiated directly — for that path, OnEnable handles
    // first-fire init via the same LaunchForward() helper below.)
    public void OnTakenFromPool(PrefabPool pool)
    {
        _pool = pool;
        ResetForFlight();
        LaunchForward();
    }

    private void OnEnable()
    {
        if (eventManager != null)
        {
            eventManager.onGamePaused += Pause;
            eventManager.onGameResumed += Resume;
        }
    }

    private void OnDisable()
    {
        if (eventManager != null)
        {
            eventManager.onGamePaused -= Pause;
            eventManager.onGameResumed -= Resume;
        }
    }

    private void Start()
    {
        // First-time instantiation (non-pooled path): apply initial force here.
        // For pooled spawns, OnTakenFromPool runs after Awake/OnEnable on the
        // first Get and calls LaunchForward instead. Either way the projectile
        // moves on its first frame.
        if (_pool == null) LaunchForward();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Apply damage first so it still lands on the impact frame
        if ((damageableLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            if (other.TryGetComponent<Enemy>(out Enemy detectedEnemy))
            {
                detectedEnemy.TakeDamage(damage);

                // Knock the enemy back along the projectile's travel direction.
                // Use previousVelocity because by this frame rb.linearVelocity may
                // already reflect the trigger collision response.
                Vector3 dir = previousVelocity.sqrMagnitude > 0.01f
                    ? previousVelocity
                    : transform.forward;
                detectedEnemy.ApplyKnockback(dir, knockbackDistance);
            }
        }

        bounceCounter++;
        if (bounceCounter >= maxBounce)
        {
            Despawn();
        }
    }

    private void Update()
    {
        if (levelManager != null && levelManager.CurrentGameState == GameState.paused)
        {
            return;
        }

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
        bounceCounter = 0;
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
        rb.AddForce(transform.forward * velocity, ForceMode.Impulse);
    }

    // Return to pool if we came from one, otherwise destroy.
    private void Despawn()
    {
        if (_pool != null)
        {
            _pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
