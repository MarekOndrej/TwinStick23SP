using System;
using UnityEngine;

public class InputListener : MonoBehaviour
{
    public event Action onSpacePressed;
    public event Action onEnterPressed;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("You pressed space");

            //send msg
            //can receive empty = "?"
            onSpacePressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            onEnterPressed?.Invoke();
        }
    }
}
