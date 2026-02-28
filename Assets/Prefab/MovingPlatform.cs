using UnityEngine;

public class GlitchableObject : Glitchable
{
    [Header("Object Type")]
    public bool isAnimal = false;  // Check if this is an animal (chases player), uncheck for platform

    [Header("Animal Settings (only if isAnimal=true)")]
    [Tooltip("Speed when chasing player")]
    public float chaseSpeed = 3f;
    [Tooltip("Start chasing when player is within this distance")]
    public float chaseRange = 8f;

    [Header("Platform Settings (only if isAnimal=false)")]
    public Transform pointA;
    public Transform pointB;
    public float normalSpeed = 2f;
    public float glitchedSpeed = 6f;
    public bool glitchedReverseDirection = false;

    [Header("Visual Glitch Effect")]
    [Tooltip("Shake amount when glitched")]
    public float glitchShakeIntensity = 0.08f;
    [Tooltip("Flicker speed when glitched")]
    public float glitchFlickerSpeed = 12f;
    [Tooltip("Glitch tint color")]
    public Color glitchColor = new Color(1f, 0.2f, 0.6f, 1f);

    // Components
    private Transform player;
    private SpriteRenderer mainRenderer;
    private Rigidbody2D rb;
    
    // Original states
    private Vector3 originalPos;
    private Vector3 originalScale;
    private Color originalColor;
    
    // Platform specific - store positions to prevent drift
    private Vector3 pointAPos;
    private Vector3 pointBPos;
    private Vector3 targetPosition;
    private bool movingToPointB = true;

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

        // Find player if this is an animal
        if (isAnimal)
        {
            FindPlayer();
        }
        else
        {
            // PLATFORM: Store exact point positions
            if (pointA != null && pointB != null)
            {
                pointAPos = pointA.position;
                pointBPos = pointB.position;
                targetPosition = pointBPos;
                movingToPointB = true;
            }
        }
        
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // Keep searching for player if animal and not found
        if (isAnimal && player == null)
        {
            FindPlayer();
        }
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
        if (isAnimal)
        {
            // ANIMAL BEHAVIOR (chases player)
            AnimalGlitchedBehavior();
        }
        else
        {
            // PLATFORM BEHAVIOR (moves between points)
            PlatformGlitchedBehavior();
        }
        
        // Apply glitch visuals ONLY when glitched
        ApplyGlitchVisuals();
    }

    protected override void NormalBehavior()
    {
        if (isAnimal)
        {
            // ANIMAL NORMAL (idle)
            AnimalNormalBehavior();
        }
        else
        {
            // PLATFORM NORMAL (normal speed)
            PlatformNormalBehavior();
        }
        
        // IMPORTANT FIX: Reset to original visuals when unglitched
        transform.position = new Vector3(transform.position.x, transform.position.y, 0); // Reset any Z offset
        mainRenderer.color = originalColor;
        transform.localScale = originalScale;
    }

    // ANIMAL BEHAVIORS
    private void AnimalGlitchedBehavior()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Chase if in range
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

            // Flip sprite
            if (direction.x != 0)
                mainRenderer.flipX = direction.x < 0;
        }
        else
        {
            // Stop when out of range
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    private void AnimalNormalBehavior()
    {
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // PLATFORM BEHAVIORS
    private void PlatformGlitchedBehavior()
    {
        if (pointA == null || pointB == null) return;
        
        float currentSpeed = glitchedSpeed;
        
        // Determine target based on reverse setting
        Vector3 target;
        if (glitchedReverseDirection)
        {
            // When reversed, go to opposite point
            target = movingToPointB ? pointAPos : pointBPos;
        }
        else
        {
            // Normal direction
            target = movingToPointB ? pointBPos : pointAPos;
        }
        
        // Move towards target
        MovePlatform(target, currentSpeed);
    }

    private void PlatformNormalBehavior()
    {
        if (pointA == null || pointB == null) return;
        
        float currentSpeed = normalSpeed;
        
        // Normal direction only
        Vector3 target = movingToPointB ? pointBPos : pointAPos;
        
        // Move towards target
        MovePlatform(target, currentSpeed);
    }

    private void MovePlatform(Vector3 target, float speed)
    {
        // Move towards target position
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        
        // Check if reached target
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            // Snap exactly to target
            transform.position = target;
            
            // Switch direction
            movingToPointB = !movingToPointB;
        }
    }

    private void ApplyGlitchVisuals()
    {
        // Store current position before visual shake
        Vector3 currentPos = transform.position;
        
        // Shake effect (visual only)
        float shakeX = Mathf.Sin(Time.time * glitchFlickerSpeed * 1.5f) * glitchShakeIntensity;
        float shakeY = Mathf.Cos(Time.time * glitchFlickerSpeed * 1.8f) * glitchShakeIntensity;
        
        // Apply shake
        transform.position = new Vector3(
            currentPos.x + shakeX,
            currentPos.y + shakeY,
            currentPos.z
        );

        // Color flicker
        float flicker = (Mathf.Sin(Time.time * glitchFlickerSpeed) + 1f) * 0.5f;
        mainRenderer.color = Color.Lerp(originalColor, glitchColor, flicker);

        // Scale pulse
        float pulse = 1f + Mathf.Sin(Time.time * glitchFlickerSpeed * 1.2f) * 0.03f;
        transform.localScale = originalScale * pulse;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // ANIMAL: kill player if glitched
            if (isAnimal && isGlitched)
            {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.Die();
                }
            }
            
            // PLATFORM: parent player for moving platform
            if (!isAnimal)
            {
                collision.gameObject.transform.parent = transform;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // PLATFORM: unparent player when leaving
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
            // Reset visuals immediately when unglitched
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            mainRenderer.color = originalColor;
            transform.localScale = originalScale;
            
            // Stop movement
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    // Visual aids in editor
    private void OnDrawGizmosSelected()
    {
        if (isAnimal)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
        }
        else
        {
            if (pointA != null && pointB != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(pointA.position, pointB.position);
                Gizmos.DrawWireSphere(pointA.position, 0.3f);
                Gizmos.DrawWireSphere(pointB.position, 0.3f);
            }
        }
    }
}