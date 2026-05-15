using UnityEngine;

public class Damageable : MonoBehaviour
{

    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth;

    EventManagerSO eventManager;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }

    private void Start()
    {
        // start with full health
        currentHealth = maxHealth;
        eventManager.PlayerHealthChanged(currentHealth, maxHealth);
    }

    public void ReceiveDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        eventManager.PlayerHealthChanged(currentHealth, maxHealth);

        // if health drops to zero or below
        if(currentHealth <= 0)
        {
            // Inform event manager
            eventManager.GameOver();

            // Kick the bucket
            Destroy(this.gameObject);
        }
    }



}
