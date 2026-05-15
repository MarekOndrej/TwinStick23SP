using UnityEngine;

public class Damageable : MonoBehaviour
{

    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth;
    bool isDead;

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
        if (isDead) return;

        currentHealth -= damageAmount;

        eventManager.PlayerHealthChanged(currentHealth, maxHealth);

        // if health drops to zero or below
        if (currentHealth <= 0)
        {
            isDead = true;

            // Inform event manager
            eventManager.GameOver();

            // Disable gameplay scripts instead of destroying the player so the
            // game-over UI/restart flow has something to talk to.
            var controller = GetComponent<PlayerController>();
            if (controller) controller.enabled = false;

            var charController = GetComponent<CharacterController>();
            if (charController) charController.enabled = false;

            var gun = GetComponentInChildren<Gun>();
            if (gun) gun.WantsToFire = false;
        }
    }



}
