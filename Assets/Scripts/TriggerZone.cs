using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    private EventManagerSO eventManager;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // player detected!

            eventManager.ZoneTriggered();
        }
    }
}
