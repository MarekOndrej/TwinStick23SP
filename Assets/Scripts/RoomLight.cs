using UnityEngine;

public class RoomLight : MonoBehaviour
{
    //reference to light component
    private Light lightB;
    //event manager
    private EventManagerSO eventManager;

    private void Awake()
    {
        //fetch light component
        lightB = GetComponent<Light>();

        //fetch event manager
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }

    //turn off light on game start
    private void Start()
    {
        lightB.enabled = false;
    }
    //subscribe/unsubscribe to zone triggerend
    private void OnEnable()
    {
        eventManager.onZoneTriggered += TurnOnLight;
    }
    private void OnDisable()
    {
        eventManager.onZoneTriggered -= TurnOnLight;
    }

    //turn on light on trigger
    private void TurnOnLight()
    {
        lightB.enabled = true;
    }
}
