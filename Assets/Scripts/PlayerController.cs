using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ─── Existing fields you already have ───
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Fall Speed")]
    public float fallMultiplier = 2.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    // Animator & flip
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool wantsToJump = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();   // for flipping

        inputActions = new PlayerInputActions();
        Physics2D.queriesStartInColliders = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += _ => wantsToJump = true;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Jump.performed -= _ => wantsToJump = true;
    }

    private void Update()
    {
        // ─── Movement ────────────────────────────────────────────────
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // ─── Ground check ────────────────────────────────────────────
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // ─── Faster fall ─────────────────────────────────────────────
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        // ─── Jump ────────────────────────────────────────────────────
        if (wantsToJump)
        {
            wantsToJump = false;
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        // ─── ANIMATION PARAMETERS ────────────────────────────────────
        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f;           // small deadzone

        animator.SetBool("running", isMoving);
        animator.SetBool("jumping", !isGrounded);                 // true only when in air

        // ─── FLIP CHARACTER TO FACE MOVEMENT DIRECTION ──────────────
        if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = false;     // face right
        }
        else if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = true;      // face left
        }
        // when not moving → keep last direction (no change)
    }

    // Debug ground check
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}