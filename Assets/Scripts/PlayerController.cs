using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public float fallMultiplier = 2.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isGrounded;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool wantsToJump = false;

    [Header("Unglitch Ability")]
    public float selectionRange = 8f;
    public LayerMask glitchableLayer;

    private List<Glitchable> allGlitchablesInScene = new List<Glitchable>();
    private Glitchable selectedGlitchable;
    private Glitchable currentlyUnglitched;
    private int selectionIndex = 0;

    [Header("Death Effect")]
    public float deathGlitchDuration = 1.5f;
    private bool isDead = false;
    private Coroutine deathCoroutine;

    [Header("Player Glitch Effect")]
    private bool isPlayerGlitched = false;
    private Color originalPlayerColor;
    private Vector3 originalPlayerScale;
    private Coroutine playerGlitchCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputActions = new PlayerInputActions();
        Physics2D.queriesStartInColliders = false;
        
        originalPlayerColor = spriteRenderer.color;
        originalPlayerScale = transform.localScale;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += _ => wantsToJump = true;
        inputActions.Player.SelectLeft.performed += _ => CycleSelection(-1);
        inputActions.Player.SelectRight.performed += _ => CycleSelection(1);
        inputActions.Player.ToggleUnglitch.performed += _ => TryToggleUnglitch();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Jump.performed -= _ => wantsToJump = true;
        inputActions.Player.SelectLeft.performed -= _ => CycleSelection(-1);
        inputActions.Player.SelectRight.performed -= _ => CycleSelection(1);
        inputActions.Player.ToggleUnglitch.performed -= _ => TryToggleUnglitch();
    }

    private void Start()
    {
        // Find all glitchable objects in the scene at start
        FindAllGlitchables();
        
        // Check if player should be glitched from previous game completion
        if (GameManager.Instance != null && GameManager.Instance.playerGlitched)
        {
            EnablePlayerGlitch(true);
        }
    }

    private void FindAllGlitchables()
    {
        allGlitchablesInScene.Clear();
        Glitchable[] glitchables = FindObjectsByType<Glitchable>(FindObjectsSortMode.None);
        allGlitchablesInScene.AddRange(glitchables);
        
        Debug.Log($"Found {allGlitchablesInScene.Count} glitchable objects in scene");
        
        // Select the first one by default
        if (allGlitchablesInScene.Count > 0)
        {
            selectedGlitchable = allGlitchablesInScene[0];
            selectionIndex = 0;
            UpdateSelectionVisuals();
        }
    }

    private void Update()
    {
        // Don't process movement if dead
        if (isDead) return;
        
        // Movement
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        if (wantsToJump)
        {
            wantsToJump = false;
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f;
        animator.SetBool("running", isMoving);
        animator.SetBool("jumping", !isGrounded);

        if (moveInput.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (moveInput.x < -0.01f)
            spriteRenderer.flipX = true;
    }

    private void CycleSelection(int direction)
    {
        if (allGlitchablesInScene.Count == 0) return;

        // Turn off glow on current selection
        if (selectedGlitchable != null)
        {
            selectedGlitchable.SetSelected(false);
        }

        // Update index
        selectionIndex = (selectionIndex + direction + allGlitchablesInScene.Count) % allGlitchablesInScene.Count;
        selectedGlitchable = allGlitchablesInScene[selectionIndex];
        
        // Turn on glow on new selection
        if (selectedGlitchable != null)
        {
            selectedGlitchable.SetSelected(true);
            Debug.Log($"Selected: {selectedGlitchable.gameObject.name}");
        }
    }

    private void TryToggleUnglitch()
    {
        if (selectedGlitchable == null) return;

        if (selectedGlitchable.isGlitched)
        {
            // Unglitch the selected item
            if (currentlyUnglitched != null && currentlyUnglitched != selectedGlitchable)
            {
                currentlyUnglitched.SetGlitched(true);
            }
            
            selectedGlitchable.SetGlitched(false);
            currentlyUnglitched = selectedGlitchable;
            Debug.Log($"Unglitched: {selectedGlitchable.gameObject.name}");
        }
        else
        {
            // Glitch it back
            selectedGlitchable.SetGlitched(true);
            if (currentlyUnglitched == selectedGlitchable)
            {
                currentlyUnglitched = null;
            }
            Debug.Log($"Glitched: {selectedGlitchable.gameObject.name}");
        }
    }

    private void UpdateSelectionVisuals()
    {
        if (selectedGlitchable != null)
        {
            selectedGlitchable.SetSelected(true);
        }
    }

    // ========== PLAYER GLITCH EFFECT METHODS ==========
    
    public void EnablePlayerGlitch(bool glitched)
    {
        isPlayerGlitched = glitched;
        
        if (glitched)
        {
            if (playerGlitchCoroutine != null) StopCoroutine(playerGlitchCoroutine);
            playerGlitchCoroutine = StartCoroutine(PlayerGlitchEffect());
        }
        else
        {
            if (playerGlitchCoroutine != null)
            {
                StopCoroutine(playerGlitchCoroutine);
                playerGlitchCoroutine = null;
            }
            // Reset player visuals
            spriteRenderer.color = originalPlayerColor;
            transform.localScale = originalPlayerScale;
        }
    }
    
    private IEnumerator PlayerGlitchEffect()
    {
        while (isPlayerGlitched)
        {
            if (isDead) yield break;
            
            // Shake effect
            float shakeX = Mathf.Sin(Time.time * 15f) * 0.04f;
            float shakeY = Mathf.Cos(Time.time * 18f) * 0.04f;
            
            // Color flicker
            float flicker = (Mathf.Sin(Time.time * 12f) + 1f) * 0.5f;
            spriteRenderer.color = Color.Lerp(originalPlayerColor, new Color(1f, 0.2f, 0.6f, 1f), flicker * 0.4f);
            
            // Scale pulse
            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.02f;
            transform.localScale = originalPlayerScale * pulse;
            
            yield return null;
        }
    }

    // ========== DEATH METHODS ==========
    
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        inputActions.Player.Disable();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Stop player glitch effect if active
        if (playerGlitchCoroutine != null)
        {
            StopCoroutine(playerGlitchCoroutine);
            playerGlitchCoroutine = null;
        }

        // Check if animator has "die" parameter
        if (animator != null)
        {
            try
            {
                animator.SetTrigger("die");
            }
            catch
            {
                Debug.LogWarning("No 'die' trigger in animator");
            }
        }

        if (deathCoroutine != null) StopCoroutine(deathCoroutine);
        deathCoroutine = StartCoroutine(GlitchDeathSequence());
    }

    private IEnumerator GlitchDeathSequence()
    {
        SpriteRenderer sr = spriteRenderer;
        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        Color originalColor = sr.color;

        while (timer < deathGlitchDuration)
        {
            timer += Time.deltaTime;
            float shake = Mathf.Sin(timer * 25f) * 0.08f;
            transform.localPosition += new Vector3(shake, shake * 0.5f, 0);
            float intensity = Mathf.Sin(timer * 18f) * 0.5f + 0.5f;
            sr.color = Color.Lerp(originalColor, new Color(1f, 0.3f, 0.6f, 1f), intensity);
            yield return null;
        }

        float fadeTime = 0f;
        while (fadeTime < 0.6f)
        {
            fadeTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTime / 0.6f);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            transform.localScale = originalScale * (1f + Mathf.Sin(fadeTime * 40f) * 0.15f);
            yield return null;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // ========== UTILITY METHODS ==========
    
    // Call this to reset player glitch state when restarting
    public void ResetPlayerState()
    {
        isDead = false;
        rb.simulated = true;
        inputActions.Player.Enable();
        
        // Reset visuals
        spriteRenderer.color = originalPlayerColor;
        transform.localScale = originalPlayerScale;
        
        // Check if should be glitched
        if (GameManager.Instance != null && GameManager.Instance.playerGlitched)
        {
            EnablePlayerGlitch(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        Gizmos.color = new Color(1f, 0.7f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, selectionRange);
    }
}