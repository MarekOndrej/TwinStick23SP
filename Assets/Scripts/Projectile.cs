using UnityEngine;

public class Projectile : MonoBehaviour
{ 

    Rigidbody rb;

    [Header("Attributes")]
    [SerializeField] float velocity;
    [SerializeField] int maxBounce = 2;
    [SerializeField] float maxLife = 5f;
    [SerializeField] float damage = 2;

    [SerializeField] LayerMask damageableLayers;

    float timeAlive = 0f;
    int bounceCounter = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
    }


    private void Start()
    {
        rb.AddForce(transform.forward * velocity, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        bounceCounter++;
        // destruction

        if (bounceCounter == maxBounce)
        {
            Destroy(this.gameObject);
        }
        // see if the object is on a damageable layer
        if((damageableLayers.value & (1<<other.gameObject.layer))> 0)
        {
            //get enemy component and tell it to take damage
            Enemy detectedEnemy = other.GetComponent<Enemy>();
            detectedEnemy.TakeDamage(damage);
        }
    }
    private void Update()
    {
        timeAlive += Time.deltaTime;

        if (timeAlive > maxLife)
        {
            Destroy(this.gameObject);
        }
    }
}
