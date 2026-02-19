using Unity.VisualScripting;
using UnityEngine;

public class SpotLightController : MonoBehaviour
{
    [SerializeField] InputListener inputListener;
    private Light lightbulb;

    private void OnEnable()
    {
        //find signal
        inputListener.onSpacePressed += ToggleLight;

    }

    private void OnDisable()
    {
        //lose signal
        inputListener.onSpacePressed -= ToggleLight;
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
