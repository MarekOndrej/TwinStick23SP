using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image healthBarSprite;
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;

        //hide health bar sprite
        
    }

    private void Update()
    {
        //  copy camera rotation
        transform.rotation = camera.transform.rotation;
    }

    public void HealthPercent(float currentHealth, float maxHealth)
    {
        healthBarSprite.fillAmount = currentHealth / maxHealth;
    }
}
