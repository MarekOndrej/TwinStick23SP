using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController characterController;

    [SerializeField] float movementSpeed = 6f;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        //read horizontal and vertical input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3 (horizontalInput, 0f, verticalInput).normalized * movementSpeed;

        characterController.Move(move * Time.deltaTime);

        MoveToMouse();
    

    }

    private void MoveToMouse()
    {
        Camera cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);


        //did our ray hit?
        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            Debug.Log(hit.collider.gameObject.name);

            Debug.DrawLine(cam.transform.position, hit.point, Color.red);

            Vector3 direction = hit.point - transform.position;
            direction.y = 0f;

            transform.rotation = Quaternion.LookRotation(direction);
        }
    }


}
