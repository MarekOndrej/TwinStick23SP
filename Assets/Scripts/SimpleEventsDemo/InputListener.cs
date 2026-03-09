using System;
using UnityEngine;

public class InputListener : MonoBehaviour
{

    [SerializeField] int doorID = 0;
    public event Action onSpacePressed;
    public event Action<int> onEnterPressed;

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
            onSpacePressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            onEnterPressed?.Invoke(doorID);
        }
    }
}
