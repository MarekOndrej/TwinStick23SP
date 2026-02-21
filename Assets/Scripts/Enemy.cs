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
    [SerializeField] private bool isRanged = false;
    

    

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        //chaseTarget = GameObject.Find("ChaseTarget");

        currentHealth = maxHealth;

        //disable health bar
        healthBar.gameObject.SetActive(false);
        
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

        //is the healthbar on?
        if (!healthBar.gameObject.activeSelf)
            healthBar.gameObject.SetActive(true);
            
            
        // changing the visual bar
        healthBar.HealthPercent(currentHealth, maxHealth);
    }
}