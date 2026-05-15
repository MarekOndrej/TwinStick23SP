using UnityEngine;


public class PlayerController : MonoBehaviour
{
    CharacterController characterController;

    [SerializeField] float movementSpeed = 6f;
    [SerializeField] float gravityForce = -9.81f;

    [SerializeField] Vector3 velocity;

    [SerializeField] Gun gun;

    [SerializeField] LayerMask aimLayers;

    [SerializeField] Transform aimPoint;
    [SerializeField] Transform gunSocket;

    Vector3 recoilVelocity;
    [SerializeField] float recoverySpeed;


    //managers 
    LevelManager levelManager;
    EventManagerSO eventManager;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        levelManager = FindFirstObjectByType<LevelManager>();
        eventManager = Resources.Load<EventManagerSO>("EventManager");
    }

    private void Update()
    {

        if (levelManager.CurrentGameState == GameState.paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                eventManager.GameResumed();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            eventManager.GamePaused();
            gun.WantsToFire = false;
            return;
        }



        //read horizontal and vertical input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(horizontalInput, 0f, verticalInput).normalized * movementSpeed;

        //ground check and reset velocity pull
        if (characterController.isGrounded)
        {
            velocity.y = -2f; //to keep us ground and never floating
        }

        recoilVelocity = Vector3.MoveTowards(
            recoilVelocity,
            Vector3.zero,
            recoverySpeed * Time.deltaTime
            );


        velocity.y += gravityForce *= Time.deltaTime;

        characterController.Move((move + recoilVelocity + velocity) * Time.deltaTime);

        // jump

        if (characterController.isGrounded && Input.GetButton("Jump"))
        {
            velocity.y = Mathf.Sqrt(5f * -2 * gravityForce);
        }

        if (Input.GetButton("Fire1") && gun != null)
        {
            gun.WantsToFire = true;
            HandleShotFired();
        }
        if (Input.GetButtonUp("Fire1") && gun != null)
        {
            gun.WantsToFire = false;
        }

        // apply gravity

        velocity.y += gravityForce * Time.deltaTime;

        characterController.Move((move + velocity) * Time.deltaTime);

        MoveToMouse();


    }

    

    private void MoveToMouse()
    {
        Camera cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);


        //did our ray hit?
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, aimLayers))
        {
            //Debug.Log(hit.collider.gameObject.name);

            Debug.DrawLine(cam.transform.position, hit.point, Color.red);


            // Aiming
            Vector3 direction = hit.point - transform.position;
            Vector3 aimdirection = direction + new Vector3(0, 0f, 0f);
            gunSocket.rotation = Quaternion.LookRotation(direction);


            direction.y = 0f;

            transform.rotation = Quaternion.LookRotation(direction);


            // Move aim point

            aimPoint.position = Vector3.MoveTowards(transform.position, hit.point, 10f);
        }
    }

    private void HandleShotFired()
    {
        AddRecoil(-gunSocket.forward, gun.RecoilAmount);
    }
    private void AddRecoil(Vector3 direction, float strength)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            recoilVelocity += direction.normalized * strength;
        }
    }

}
