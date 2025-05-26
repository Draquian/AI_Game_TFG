using UnityEngine;

public class PlayerController_Copilot : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerStats_Copilot playerStats;
    private Camera mainCamera;

    // Movement variables.
    private float initialY; // Store the starting y position.
    private Vector3 moveDirection = Vector3.zero;

    // Camera control variables.
    private float cameraPitch = 0f;
    private float cameraSensitivity = 2f;

    // This flag disables camera rotation if set to true.
    public bool lockCameraRotation = false;

    // For handling magic charging.
    private bool isChargingMagic = false;
    private float magicChargeTimer = 0f;
    private float magicChargeThreshold = 1f; // seconds required for a charged attack

    // Boost variables.
    private bool isBoosting = false;
    private float boostSpeedMultiplier = 2f;
    private float boostDuration = 1f;
    private float boostTimer = 0f;

    // Interaction variables.
    public float interactRange = 3f;  // Maximum distance (in units) to interact with an object.

    void Start()
    {
        // Add and get required components.
        characterController = gameObject.GetComponent<CharacterController>();

        // Ensure PlayerStats is attached.
        playerStats = gameObject.GetComponent<PlayerStats_Copilot>();
        if (playerStats == null)
        {
            playerStats = gameObject.AddComponent<PlayerStats_Copilot>();
        }

        // Get (or create) the main camera.
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCamera = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }

        // Store the starting y position.
        initialY = transform.position.y;
    }

    void Update()
    {
        // Block any player actions if the game is paused.
        if (Time.timeScale == 0f)
            return;

        HandleMovement();
        HandleAiming();
        HandleAttacks();
        HandleBoost();
        HandleInteraction();
    }

    void HandleMovement()
    {
        // Get horizontal and vertical input (WASD/left analog stick).
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Determine current speed (with boost if active).
        float currentSpeed = playerStats.speed;
        if (isBoosting)
        {
            currentSpeed *= boostSpeedMultiplier;
        }

        // Only apply horizontal movement.
        Vector3 horizontalMove = move * currentSpeed;
        horizontalMove.y = 0f;  // Ensure no y movement is added.

        // Move the character.
        characterController.Move(horizontalMove * Time.deltaTime);

        // Enforce constant y position.
        Vector3 pos = transform.position;
        pos.y = 0.25f;//initialY
        transform.position = pos;
    }

    void HandleAiming()
    {
        // Only process mouse input if camera rotation is not locked.
        if (!lockCameraRotation)
        {
            float mouseX = Input.GetAxis("Mouse X") * cameraSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * cameraSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -45f, 45f);
            mainCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

            // Keep the camera at a fixed offset from the player's position.
            mainCamera.transform.position = transform.position + new Vector3(0, 2f, 0);
        }
        // When lockCameraRotation is true, the camera remains fixed.
    }

    void HandleAttacks()
    {
        // Physical attack: Left Mouse Button or A (mapped as "Fire1").
        if (Input.GetButtonDown("Fire1"))
        {
            playerStats.Attack();
        }

        // Magical attack: Right Mouse Button or X (mapped as "Fire2").
        if (Input.GetButtonDown("Fire2"))
        {
            isChargingMagic = true;
            magicChargeTimer = 0f;
        }
        if (Input.GetButton("Fire2"))
        {
            if (isChargingMagic)
            {
                magicChargeTimer += Time.deltaTime;
            }
        }
        if (Input.GetButtonUp("Fire2"))
        {
            if (isChargingMagic)
            {
                // If held long enough, execute charged magic attack.
                if (magicChargeTimer >= magicChargeThreshold)
                {
                    if (playerStats.magicAbility != null)
                    {
                        playerStats.magicAbility.MagicChargedAttack();
                    }
                }
                else
                {
                    // Otherwise, perform a normal magic attack.
                    if (playerStats.magicAbility != null)
                    {
                        playerStats.magicAbility.MagicAttack();
                    }
                }
                isChargingMagic = false;
            }
        }
    }

    void HandleBoost()
    {
        // Boost: Space (or Y on Xbox controller, mapped as "Jump").
        if (Input.GetButtonDown("Jump"))
        {
            isBoosting = true;
            boostTimer = 0f;
        }
        if (isBoosting)
        {
            boostTimer += Time.deltaTime;
            if (boostTimer >= boostDuration)
            {
                isBoosting = false;
            }
        }
    }
    /// <summary>
    /// Handles interaction input. When the E key is pressed, a ray is cast from the center
    /// of the screen to see if it hits an object with an IInteractable component.
    /// </summary>
    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Create a ray from the center of the screen.
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactRange))
            {
                IInteractable_Copilot interactable = hit.collider.GetComponent<IInteractable_Copilot>();
                if (interactable != null)
                {
                    // Call the interact method on the object.
                    interactable.Interact(gameObject);
                }
            }
        }
    }
}
