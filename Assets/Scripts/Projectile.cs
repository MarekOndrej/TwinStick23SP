using UnityEngine;

public class Projectile : MonoBehaviour
{ 

    Rigidbody rb;

    [SerializeField] float velocity;
    [SerializeField] int maxBounce = 2;
    [SerializeField] float maxLife = 5f;

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

        if (bounceCounter == maxBounce)
        {
            Destroy(this.gameObject);
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
