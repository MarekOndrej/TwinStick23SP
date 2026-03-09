using System;
using UnityEngine;

public class InputListener : MonoBehaviour
{

    [SerializeField] int doorID = 0;

    [SerializeField] DemoEventManagerSO eventManger;
    //public event Action onSpacePressed;
    //public event Action<int> onEnterPressed;

    private void Start()
    {
        Debug.Log("Hello");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("You pressed space");

            //send msg
            //can receive empty = "?"
            //onSpacePressed?.Invoke();

            eventManger.Resize();
            eventManger.ToggleLight();
            eventManger.DoorToggle(doorID);
        }

        //eventManager.HealthGained(30);
    }
}
