using UnityEngine;

public class Projectile : MonoBehaviour
{ 

    Rigidbody rb;

    Vector3 previousVelocity;

    [Header("Attributes")]
    [SerializeField] float velocity;
    [SerializeField] int maxBounce = 2;
    [SerializeField] float maxLife = 5f;
    [SerializeField] float damage = 2;

    [SerializeField] LayerMask damageableLayers;

    float timeAlive = 0f;
    int bounceCounter = 0;

    LevelManager levelManager;
    EventManagerSO eventManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        levelManager = FindAnyObjectByType<LevelManager>();
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }


    private void Start()
    {
        rb.AddForce(transform.forward * velocity, ForceMode.Impulse);
    }

    private void OnEnable()
    {
        eventManager.onGamePaused += Pause;
        eventManager.onGameResumed += Resume;
    }
    private void OnDisable()
    {
        eventManager.onGamePaused -= Pause;
        eventManager.onGameResumed -= Resume;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Apply damage first so it still lands on the impact frame
        if ((damageableLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            if (other.TryGetComponent<Enemy>(out Enemy detectedEnemy))
            {
                detectedEnemy.TakeDamage(damage);
            }
        }

        bounceCounter++;
        if (bounceCounter >= maxBounce)
        {
            Destroy(this.gameObject);
        }
    }
    private void Update()
    {
        if(levelManager.CurrentGameState == GameState.paused)
        {
            return;
        }

        timeAlive += Time.deltaTime;

        if (timeAlive > maxLife)
        {
            Destroy(this.gameObject);
        }
        previousVelocity = rb.linearVelocity;
    }

    private void Pause()
    {
        rb.isKinematic = true;
    }
    private void Resume()
    {
        rb.isKinematic = false;
        rb.linearVelocity = previousVelocity;
    }
}
