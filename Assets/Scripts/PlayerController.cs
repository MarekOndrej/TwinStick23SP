using UnityEngine;


public class PlayerController : MonoBehaviour
{
    CharacterController characterController;

    [SerializeField]    float movementSpeed = 6f;
    [SerializeField]    float gravityForce = -9.81f;

    [SerializeField]    Vector3 velocity;

    [SerializeField] Gun gun;

    [SerializeField] LayerMask aimLayers;

    [SerializeField] Transform aimPoint;
    [SerializeField] Transform gunSocket;

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

        //ground check and reset velocity pull
        if (characterController.isGrounded)
        {
            velocity.y = -2f; //to keep us ground and never floating
        }


        // jump

        if (characterController.isGrounded && Input.GetButton("Jump"))
        {
            velocity.y = Mathf.Sqrt(5f * -2 * gravityForce);
        }

        if (Input.GetButton("Fire1") && gun != null)
        {
            gun.WantsToFire = true;
        }
        if (Input.GetButtonUp("Fire1") && gun != null)
        {
            gun.WantsToFire = false;
        }

        // apply gravity

        velocity.y += gravityForce * Time.deltaTime;

        characterController.Move((move+velocity) * Time.deltaTime);

        MoveToMouse();
    

    }

    private void MoveToMouse()
    {
        Camera cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);


        //did our ray hit?
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, aimLayers))
        {
            Debug.Log(hit.collider.gameObject.name);

            Debug.DrawLine(cam.transform.position, hit.point, Color.red);


            // Aiming
            Vector3 direction = hit.point - transform.position;
            Vector3 aimdirection = direction + new Vector3(0, 0.5f, 0f);
            gunSocket.rotation = Quaternion.LookRotation(aimdirection);


            direction.y = 0f;

            transform.rotation = Quaternion.LookRotation(direction);


            // Move aim point

            aimPoint.position = Vector3.MoveTowards(transform.position, hit.point, 10f);
        }
    }


}
