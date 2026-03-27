using System;
using UnityEditor.MPE;
using UnityEngine;

[CreateAssetMenu(menuName = "Managers/EventManager", fileName = "EventManager")]
public class EventManagerSO : ScriptableObject
{
    
    // === EVENT MANAGER ===
    // relays messages between scripts

    // == event actions (the messages) ==
    public event Action onZoneTriggered;

    // == methods (sending the messages) ==
    public void ZoneTriggered()
    {
        Debug.Log("Zone was triggered");
        onZoneTriggered?.Invoke();
    }
}
