using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Managers/EventManager", fileName = "EventManager")]
public class DemoEventManagerSO : ScriptableObject
{

    // the message
    public event Action onToggleLight;
    public event Action onResize;
    public event Action<int> onDoorToggle;

    public void DoorToggle(int incomingID)
    {
        onDoorToggle?.Invoke(incomingID);
    }

    public void Resize()
    {
        onResize?.Invoke();
    }


    //the sending of the message
    public void ToggleLight()
    {
        onToggleLight?.Invoke();
    }
}
