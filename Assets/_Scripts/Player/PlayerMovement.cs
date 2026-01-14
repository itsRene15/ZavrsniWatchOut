using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    [Header("Grounding")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundProbeDistance = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    public bool reversed = false;
    private float reverseTimer = 0f;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private float inputX;
    private bool lastFlipX;

    private bool isGrounded;
    private bool lastIsGrounded;

    [Header("Animation")]
    [SerializeField] private string animParamIsRunning = "isRunning";

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private float footstepsMinSpeed = 0.1f;
    [SerializeField] private float jumpVolume = 0.9f;
    [SerializeField] private float landVolume = 0.8f;

    private bool lastRunning;

    // Cache components and input actions
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
        }
    }

    // Update timers
    void Update()
    {
        HandleReverseTimer();
        jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);
    }

    // Physics and movement
    void FixedUpdate()
    {
        CheckGround();
        TryJump();

        var v = GetVelocity2D();
        v.x = inputX * moveSpeed;
        SetVelocity2D(v);

        if (animator != null)
        {
            float speedX = Mathf.Abs(v.x);
            bool running = speedX > footstepsMinSpeed;
            if (!string.IsNullOrEmpty(animParamIsRunning) && running != lastRunning)
            {
                animator.SetBool(animParamIsRunning, running);
                lastRunning = running;
            }
        }
    }

    // Check if grounded and handle coyote/land sounds
    void CheckGround()
    {
        if (groundCheck != null)
        {
            lastIsGrounded = isGrounded;
            RaycastHit2D hit = Physics2D.CircleCast(
                groundCheck.position,
                groundCheckRadius,
                Vector2.down,
                Mathf.Max(0.01f, groundProbeDistance),
                groundLayer
            );

            bool groundedNow = false;
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                if (hit.normal.y > 0.5f)
                    groundedNow = true;
            }
            isGrounded = groundedNow;
            if (isGrounded)
                coyoteTimeCounter = coyoteTime;
            else
                coyoteTimeCounter = Mathf.Max(0f, coyoteTimeCounter - Time.fixedDeltaTime);

            if (!lastIsGrounded && isGrounded)
            {
                if (landClip != null && sfxSource != null)
                    sfxSource.PlayOneShot(landClip, landVolume);
            }
        }
        else
        {
            coyoteTimeCounter = Mathf.Max(0f, coyoteTimeCounter - Time.fixedDeltaTime);
        }
        
    }

    // Try to perform jump
    void TryJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            var v = GetVelocity2D();
            v.y = jumpForce;
            SetVelocity2D(v);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            if (jumpClip != null && sfxSource != null)
                sfxSource.PlayOneShot(jumpClip, jumpVolume);
        }
    }

    // Count down reverse timer
    void HandleReverseTimer()
    {
        if (reverseTimer > 0)
        {
            reverseTimer -= Time.deltaTime;
            if (reverseTimer <= 0)
                reversed = false;
        }
    }
    
    
    /*public void SetGravity(float g)
    {
        rb.gravityScale = g;
    }*/

    // Handle horizontal input and sprite flip
    private void HandleInputX(float rawX)
    {
        float newInputX = reversed ? -rawX : rawX;
        if (Mathf.Approximately(newInputX, inputX)) return;
        inputX = newInputX;

        if (spriteRenderer != null)
        {
            if (Mathf.Abs(inputX) > 0.001f)
            {
                bool flip = inputX < 0f;
                if (flip != lastFlipX)
                {
                    spriteRenderer.flipX = flip;
                    lastFlipX = flip;
                }
            }
        }
    }

 
    private void OnEnable()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var actions = playerInput.actions;
            if (actions != null)
            {
                moveAction = actions.FindAction("Move", throwIfNotFound: false);
                jumpAction = actions.FindAction("Jump", throwIfNotFound: false);

                if (moveAction != null)
                {
                    moveAction.performed += OnMovePerformed;
                    moveAction.canceled += OnMoveCanceled;
                    if (!moveAction.enabled) moveAction.Enable();
                }

                if (jumpAction != null)
                {
                    jumpAction.performed += OnJumpStarted;
                    if (!jumpAction.enabled) jumpAction.Enable();
                }

                if (actions.enabled == false) actions.Enable();
            }
        }
    }


    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpStarted;
        }
    }

    // Read movement input
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        float rawX = 0f;
        var vt = ctx.valueType;
        if (vt == typeof(Vector2))
        {
            rawX = ctx.ReadValue<Vector2>().x;
        }
        else if (vt == typeof(float))
        {
            rawX = ctx.ReadValue<float>();
        }
        else
        {
            Vector2 v;
            if (ctx.action != null && ctx.action.type == InputActionType.Value)
            {
                v = ctx.ReadValue<Vector2>();
                rawX = v.x;
            }
        }

        HandleInputX(rawX);
    }

    // Stop movement when canceled
    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        HandleInputX(0f);
    }

    // Buffer jump input
    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        jumpBufferCounter = jumpBufferTime;
    }
    


    private Vector2 GetVelocity2D()
    {
        return rb.linearVelocity;
    }
    
    private void SetVelocity2D(Vector2 v)
    {
        rb.linearVelocity = v;
    }
}