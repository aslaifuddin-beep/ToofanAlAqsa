using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;
    public float gravity = -20f;

    [Header("Camera")]
    public Transform cameraHolder;
    public float mouseSensitivity = 2f;
    public float lookSmoothTime = 0.05f;
    public float maxLookAngle = 80f;

    [Header("Crouch")]
    public float crouchHeight = 0.5f;
    public float standingHeight = 1.8f;
    public float crouchTransitionSpeed = 10f;

    [Header("Audio")]
    public AudioSource footstepSource;
    public float footstepInterval = 0.5f;
    public float sprintFootstepMultiplier = 0.7f;

    private CharacterController characterController;
    private Vector3 velocity;
    private float currentSpeed;
    private bool isCrouching;
    private bool isSprinting;
    private float verticalRotation;
    private Vector2 currentLookVelocity;
    private float currentFootstepTimer;
    private Vector3 currentCameraPos;

    public bool IsMoving { get; private set; }
    public bool IsGrounded => characterController.isGrounded;
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraHolder == null)
            cameraHolder = Camera.main?.transform;

        currentCameraPos = cameraHolder.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
        HandleSprint();
        HandleJump();
        HandleFootsteps();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentLookVelocity = Vector2.Lerp(currentLookVelocity,
            new Vector2(mouseX, mouseY), 1f / lookSmoothTime * Time.deltaTime);

        verticalRotation -= currentLookVelocity.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * currentLookVelocity.x);
    }

    public void ApplyLookDelta(Vector2 delta)
    {
        verticalRotation -= delta.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * delta.x * mouseSensitivity);
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        IsMoving = (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f);

        currentSpeed = isCrouching ? crouchSpeed :
                       isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * h + transform.forward * v;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    public void MoveWithInput(Vector2 inputDirection)
    {
        IsMoving = inputDirection.magnitude > 0.1f;

        currentSpeed = isCrouching ? crouchSpeed :
                       isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = !isCrouching;

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        Vector3 camTargetPos = isCrouching ?
            new Vector3(0, crouchHeight * 0.8f, 0) :
            new Vector3(0, standingHeight * 0.9f, 0);

        currentCameraPos = Vector3.Lerp(currentCameraPos, camTargetPos, crouchTransitionSpeed * Time.deltaTime);
        cameraHolder.localPosition = currentCameraPos;
    }

    public void SetCrouch(bool crouch)
    {
        isCrouching = crouch;
    }

    void HandleSprint()
    {
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching && IsMoving;
    }

    public void SetSprint(bool sprint)
    {
        isSprinting = sprint && !isCrouching && IsMoving;
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    public void Jump()
    {
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void HandleFootsteps()
    {
        if (!IsMoving || !characterController.isGrounded)
        {
            currentFootstepTimer = 0f;
            return;
        }

        float interval = isSprinting ? footstepInterval * sprintFootstepMultiplier : footstepInterval;
        currentFootstepTimer -= Time.deltaTime;

        if (currentFootstepTimer <= 0f)
        {
            if (footstepSource != null)
                footstepSource.Play();

            currentFootstepTimer = interval;
        }
    }
}
