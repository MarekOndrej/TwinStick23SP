using Unity.VisualScripting;
using UnityEngine;

public class SpotLightController : MonoBehaviour
{
    [SerializeField] DemoEventManagerSO eventManager;
    private Light lightbulb;

    private void OnEnable()
    {
        //find signal
        eventManager.onToggleLight += ToggleLight;

    }

    private void OnDisable()
    {
        //lose signal
        eventManager.onToggleLight -= ToggleLight;
    }

    private void Awake()
    {
        lightbulb = GetComponent<Light>();
    }

    
    private void ToggleLight()
    {
        //if its on, it turns off, if not, it turns on
        lightbulb.enabled = !lightbulb.enabled;
    }
}
