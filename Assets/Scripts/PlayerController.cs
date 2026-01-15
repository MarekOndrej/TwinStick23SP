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


    

    }


}
