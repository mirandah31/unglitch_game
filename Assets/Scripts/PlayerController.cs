using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Fall Speed")]
    public float fallMultiplier = 2.5f;

    [Header("Ground Check")]
    public Transform groundCheck;               // Empty child positioned at/below feet
    public float groundCheckRadius = 0.25f;     // 0.2–0.35 range usually works best
    public LayerMask groundLayer;               // MUST be your "Ground" layer only

    private Rigidbody2D rb;
    private bool isGrounded;

    // Input
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool wantsToJump = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();

        // ─── THIS LINE FIXES SELF-DETECTION ───
        // Prevents OverlapCircle from hitting the player's own collider
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
        // Movement
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Faster fall when going down
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        // Jump only if grounded + pressed this frame
        if (wantsToJump)
        {
            wantsToJump = false;  // Consume input

            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    // Debug: Green circle = grounded (can jump), Red = air (no jump)
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}