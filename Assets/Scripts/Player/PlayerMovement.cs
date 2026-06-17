using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintSpeed = 14f;
    public float groundDrag = 6f;
    public float airDrag = 1f;
    public float airMultiplier = 0.4f;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float coyoteTime = 0.15f;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask groundMask;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;

    // States
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public MovementState state;

    public enum MovementState { Walking, Sprinting, Air }

   
    private Rigidbody rb;
    private Transform orientation;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDir;
    private bool readyToJump = true;
    private float coyoteTimer;
    private RaycastHit slopeHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        
        GameObject ori = new GameObject("Orientation");
        ori.transform.SetParent(transform);
        ori.transform.localPosition = Vector3.zero;
        orientation = ori.transform;
    }

    private void Update()
    {
        GroundCheck();
        GetInput();
        StateHandler();
        SpeedControl();
        HandleDrag();

      
        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0f;
            if (camForward != Vector3.zero)
                orientation.forward = camForward;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();
    }

    private void StateHandler()
    {
        if (isGrounded && isSprinting)
            state = MovementState.Sprinting;
        else if (isGrounded)
            state = MovementState.Walking;
        else
            state = MovementState.Air;
    }

    private void MovePlayer()
    {
        moveDir = orientation.forward * verticalInput
                + orientation.right * horizontalInput;

        float currentSpeed = (state == MovementState.Sprinting) ? sprintSpeed : moveSpeed;

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDir() * currentSpeed * 20f, ForceMode.Force);
            // Keep player on slope 
            if (rb.linearVelocity.y > 0f)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (isGrounded)
            rb.AddForce(moveDir.normalized * currentSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDir.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);

        //  player doesn't slide
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        float currentSpeed = (state == MovementState.Sprinting) ? sprintSpeed : moveSpeed;

        if (OnSlope())
        {
            if (rb.linearVelocity.magnitude > currentSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            return;
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 capped = flatVel.normalized * currentSpeed;
            rb.linearVelocity = new Vector3(capped.x, rb.linearVelocity.y, capped.z);
        }
    }

    private void HandleDrag()
    {
        rb.linearDamping = isGrounded ? groundDrag : airDrag;
    }

    private void TryJump()
    {
        bool canJump = (isGrounded || coyoteTimer > 0f) && readyToJump;
        if (!canJump) return;

        readyToJump = false;
        coyoteTimer = 0f;

        
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump() => readyToJump = true;

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + 0.2f,
            groundMask
        );

        // Coyote time — preserve jump window briefly after walking off edge
        if (wasGrounded && !isGrounded)
            coyoteTimer = coyoteTime;
        else if (isGrounded)
            coyoteTimer = 0f;
        else
            coyoteTimer -= Time.deltaTime;
    }

    // Slope check
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down,
            out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDir()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

  
    public void SetVelocity(Vector3 vel) => rb.linearVelocity = vel;
    public Vector3 GetVelocity() => rb.linearVelocity;
    public Rigidbody GetRigidbody() => rb;
}