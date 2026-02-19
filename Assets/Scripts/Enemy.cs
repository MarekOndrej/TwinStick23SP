using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    //[SerializeField] Vector3 destination;

    //[SerializeField] float speed = 0f;
    [Header("General")]
    [SerializeField] Transform chaseTarget;

    NavMeshAgent agent;
    [Header("Health Related")]
    [SerializeField] HealthBar healthBar;
    [SerializeField] private float maxHealth = 10;
    [SerializeField] private float currentHealth;
    

    

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        //chaseTarget = GameObject.Find("ChaseTarget");

        currentHealth = maxHealth;
        
    }
    private void Update()
    {
        // Update agent's destination

        agent.destination = chaseTarget.transform.position;
    }


    public void SetChaseTarget(GameObject newChaseTarget)
    {
        chaseTarget = newChaseTarget.transform;
    }

    public void TakeDamage(float incomingDamage)
    {
        currentHealth -= incomingDamage;
        if (currentHealth == 0)
        {
            Destroy(this.gameObject);
        }

        healthBar.HealthPercent(currentHealth, maxHealth);
    }
}