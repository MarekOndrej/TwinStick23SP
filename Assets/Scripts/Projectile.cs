using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] float velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
    }


    private void Start()
    {
        rb.AddForce(transform.forward * velocity, ForceMode.Impulse);
    }
}
