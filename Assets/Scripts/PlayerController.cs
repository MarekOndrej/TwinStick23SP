using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    CharacterController characterController;

    [SerializeField] float movementSpeed = 6f;
    [SerializeField] float gravityForce = -9.81f;
    [SerializeField] float jumpHeight = 5f;

    [SerializeField] Vector3 velocity;

    [SerializeField] Gun gun;

    [SerializeField] LayerMask aimLayers;

    [SerializeField] Transform aimPoint;
    [SerializeField] Transform gunSocket;

    Vector3 recoilVelocity;
    [SerializeField] float recoverySpeed = 8f;

    [Header("Fall death")]
    [Tooltip("If the player's Y drops below this for fallDeathDelay seconds, they die.")]
    [SerializeField] float fallDeathYThreshold = -5f;
    [SerializeField] float fallDeathDelay = 3f;
    float fallTimer;

    //managers
    LevelManager levelManager;
    EventManagerSO eventManager;
    Damageable damageable;

    // Input System action handles
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        damageable = GetComponent<Damageable>();
        levelManager = FindFirstObjectByType<LevelManager>();
        eventManager = Resources.Load<EventManagerSO>("EventManager");

        var actions = InputSystem.actions;
        if (actions == null)
        {
            Debug.LogError(
                "PlayerController: InputSystem.actions is null. " +
                "Set the project-wide actions asset in Project Settings → Input System Package.");
            return;
        }

        moveAction = actions.FindAction("Player/Move");
        jumpAction = actions.FindAction("Player/Jump");
        attackAction = actions.FindAction("Player/Attack");
    }

    private void OnEnable()
    {
        // Ensure the Player action map is active even if some other script disabled it
        InputSystem.actions?.FindActionMap("Player")?.Enable();

        // Recoil now fires when the gun actually shoots, not every frame Fire is held
        if (eventManager != null)
            eventManager.onGunFired += HandleShotFired;
    }

    private void OnDisable()
    {
        if (eventManager != null)
            eventManager.onGunFired -= HandleShotFired;
    }

    private void Update()
    {
        bool pausePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        if (levelManager.CurrentGameState == GameState.paused)
        {
            if (pausePressed)
            {
                eventManager.GameResumed();
            }
            return;
        }

        if (levelManager.CurrentGameState == GameState.gameOver)
        {
            if (gun != null) gun.WantsToFire = false;
            return;
        }

        if (pausePressed)
        {
            eventManager.GamePaused();
            if (gun != null) gun.WantsToFire = false;
            return;
        }

        // Fall-death: if the player falls below the arena for fallDeathDelay
        // seconds, deal lethal damage so the normal game-over flow fires.
        if (transform.position.y < fallDeathYThreshold)
        {
            fallTimer += Time.deltaTime;
            if (fallTimer >= fallDeathDelay)
            {
                fallTimer = 0f;
                if (damageable != null) damageable.ReceiveDamage(float.MaxValue);
                return;
            }
        }
        else
        {
            fallTimer = 0f;
        }

        // Movement input
        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        if (move.sqrMagnitude > 1f) move.Normalize();
        move *= movementSpeed;

        // Ground check & jump
        if (characterController.isGrounded)
        {
            velocity.y = -2f; // keep us pinned to the ground when grounded

            if (jumpAction != null && jumpAction.WasPressedThisFrame())
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityForce);
            }
        }

        // Recoil decays toward zero
        recoilVelocity = Vector3.MoveTowards(
            recoilVelocity,
            Vector3.zero,
            recoverySpeed * Time.deltaTime);

        // Apply gravity (additive — do not mutate gravityForce)
        velocity.y += gravityForce * Time.deltaTime;

        // Single Move per frame, including recoil
        characterController.Move((move + recoilVelocity + velocity) * Time.deltaTime);

        // Fire control — Gun handles its own cadence; we just set intent.
        if (gun != null)
        {
            gun.WantsToFire = attackAction != null && attackAction.IsPressed();
        }

        MoveToMouse();
    }



    private void MoveToMouse()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (Mouse.current == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);


        //did our ray hit?
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, aimLayers))
        {
            Debug.DrawLine(cam.transform.position, hit.point, Color.red);

            // Aiming
            Vector3 direction = hit.point - transform.position;
            if (gunSocket != null) gunSocket.rotation = Quaternion.LookRotation(direction);


            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction);


            // Move aim point
            if (aimPoint != null)
                aimPoint.position = Vector3.MoveTowards(transform.position, hit.point, 10f);
        }
    }

    private void HandleShotFired()
    {
        if (gunSocket == null || gun == null) return;
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
