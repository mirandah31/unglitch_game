using UnityEngine;

public class Animal : Glitchable
{
    [Header("Object Type")]
    public bool isAnimal = false;

    [Header("Animal Settings")]
    public float chaseSpeed = 3f;
    public float chaseRange = 8f;

    [Header("Platform Settings")]
    public Transform pointA;
    public Transform pointB;
    public float normalSpeed = 2f;
    public float glitchedSpeed = 6f;
    public bool glitchedReverseDirection = false;

    [Header("Visual Glitch Effect")]
    [Tooltip("How much the object shakes when glitched (reduced for stability)")]
    public float glitchShakeIntensity = 0.03f;  // Reduced from 0.08f
    [Tooltip("Speed of glitch effects")]
    public float glitchFlickerSpeed = 8f;  // Reduced from 12f
    [Tooltip("Glitch tint color")]
    public Color glitchColor = new Color(1f, 0.2f, 0.6f, 0.7f);

    // Components
    private Transform player;
    private SpriteRenderer mainRenderer;
    private Rigidbody2D rb;
    
    // Original states
    private Vector3 originalPos;
    private Vector3 originalScale;
    private Color originalColor;
    
    // Platform specific
    private Vector3 pointAPos;
    private Vector3 pointBPos;
    private bool movingToPointB = true;
    private Vector3 visualOffset; // Track visual shake offset

    protected override void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (mainRenderer == null)
        {
            Debug.LogError("GlitchableObject needs SpriteRenderer!");
            return;
        }

        originalPos = transform.position;
        originalScale = transform.localScale;
        originalColor = mainRenderer.color;

        if (isAnimal)
        {
            FindPlayer();
        }
        else
        {
            if (pointA != null && pointB != null)
            {
                pointAPos = pointA.position;
                pointBPos = pointB.position;
                movingToPointB = true;
            }
        }
        
        base.Start(); // This sets up the glow
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    protected override void GlitchedBehavior()
    {
        // Store the base position before visual effects
        Vector3 basePosition = transform.position;
        
        if (isAnimal)
        {
            AnimalGlitchedBehavior();
        }
        else
        {
            PlatformGlitchedBehavior();
        }
        
        // Apply visual glitch (but don't affect actual movement too much)
        ApplyGlitchVisuals();
    }

    protected override void NormalBehavior()
    {
        // Reset visual offset
        visualOffset = Vector3.zero;
        
        if (isAnimal)
        {
            AnimalNormalBehavior();
        }
        else
        {
            PlatformNormalBehavior();
        }
        
        // Reset visuals completely
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        mainRenderer.color = originalColor;
        transform.localScale = originalScale;
    }

    private void AnimalGlitchedBehavior()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distToPlayer < chaseRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
            }
            else
            {
                transform.position += direction * chaseSpeed * Time.deltaTime;
            }

            if (direction.x != 0)
                mainRenderer.flipX = direction.x < 0;
        }
        else
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    private void AnimalNormalBehavior()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void PlatformGlitchedBehavior()
    {
        if (pointA == null || pointB == null) return;
        
        float currentSpeed = glitchedSpeed;
        
        Vector3 target = glitchedReverseDirection 
            ? (movingToPointB ? pointAPos : pointBPos)
            : (movingToPointB ? pointBPos : pointAPos);
        
        MovePlatform(target, currentSpeed);
    }

    private void PlatformNormalBehavior()
    {
        if (pointA == null || pointB == null) return;
        
        Vector3 target = movingToPointB ? pointBPos : pointAPos;
        MovePlatform(target, normalSpeed);
    }

    private void MovePlatform(Vector3 target, float speed)
    {
        // Move without visual shake affecting position
        Vector3 newPos = Vector3.MoveTowards(transform.position - visualOffset, target, speed * Time.deltaTime);
        transform.position = newPos + visualOffset;
        
        if (Vector3.Distance(newPos, target) < 0.01f)
        {
            transform.position = target + visualOffset;
            movingToPointB = !movingToPointB;
        }
    }

    private void ApplyGlitchVisuals()
    {
        // Calculate visual shake (stays around point A/platform position)
        float shakeX = Mathf.Sin(Time.time * glitchFlickerSpeed * 1.5f) * glitchShakeIntensity;
        float shakeY = Mathf.Cos(Time.time * glitchFlickerSpeed * 1.8f) * glitchShakeIntensity * 0.5f;
        
        // Store visual offset
        visualOffset = new Vector3(shakeX, shakeY, 0);
        
        // Apply visual offset to current position
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z
        );

        // Color flicker
        float flicker = (Mathf.Sin(Time.time * glitchFlickerSpeed) + 1f) * 0.5f;
        mainRenderer.color = Color.Lerp(originalColor, glitchColor, flicker * 0.7f); // Less intense color change

        // Scale pulse (subtle)
        float pulse = 1f + Mathf.Sin(Time.time * glitchFlickerSpeed * 1.2f) * 0.02f;
        transform.localScale = originalScale * pulse;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (isAnimal && isGlitched)
            {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.Die();
                }
            }
            
            if (!isAnimal)
            {
                collision.gameObject.transform.parent = transform;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!isAnimal && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = null;
        }
    }

    public override void SetGlitched(bool glitched)
    {
        base.SetGlitched(glitched);
        
        if (!glitched)
        {
            // Reset everything when unglitched
            visualOffset = Vector3.zero;
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            mainRenderer.color = originalColor;
            transform.localScale = originalScale;
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}