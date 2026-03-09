using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    [SerializeField] int doorID;

    [SerializeField] Light doorLight;
    [SerializeField] TMP_Text doorText;

    [SerializeField] Color openColor = Color.green;
    [SerializeField] Color closeColor = Color.red;

    [SerializeField] Transform door;
    [SerializeField] Transform shutPos;
    [SerializeField] Transform openPos;

    [SerializeField] DemoEventManagerSO eventManager;


    bool _doorToggleRequested;
    bool _isOpen;

    private void Start()
    {
        _isOpen = false;
        doorText.text = doorID.ToString();
        doorLight.color = closeColor;
    }

    private void Update()
    {
        if (_doorToggleRequested)
        {
            if (!_isOpen)
            {
                MoveDoor(openPos);
            }
            else
            {
                MoveDoor(shutPos);
            }
        }
    }


    private void OnEnable()
    {
        eventManager.onDoorToggle += RequestToggle;
    }

    private void OnDisable()
    {
        eventManager.onDoorToggle -= RequestToggle;
    }

    private void MoveDoor(Transform target)
    {
        door.position = Vector3.MoveTowards(door.position, target.position, 5f * Time.deltaTime);

        if(Vector3.Distance(door.position, target.position) < 0.01f)
        {
            
            _isOpen = !_isOpen;
            _doorToggleRequested = false;
            doorLight.color = (_isOpen) ? openColor : closeColor;
        }
    }

    private void RequestToggle(int incomingDoorID)
    {
        if (incomingDoorID == doorID)
        {
            _doorToggleRequested = true;
        }
    }
}
