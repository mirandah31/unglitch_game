using UnityEngine;

public class Animal : Glitchable
{
    [Header("Glitched (Chase/Attack) Settings")]
    [Tooltip("Speed when chasing player")]
    public float chaseSpeed = 3f;
    [Tooltip("Start chasing when player is within this distance")]
    public float chaseRange = 8f;

    [Header("Visual Glitch Effect (Only when Glitched)")]
    [Tooltip("Shake amount")]
    public float glitchShakeIntensity = 0.08f;
    [Tooltip("Flicker speed")]
    public float glitchFlickerSpeed = 12f;
    [Tooltip("Glitch tint color")]
    public Color glitchColor = new Color(1f, 0.2f, 0.6f, 1f);  // Pink/purple

    private Transform player;
    private Vector2 originalPos;
    private SpriteRenderer mainRenderer;
    private Vector3 originalScale;
    private Color originalColor;
    private bool playerSearchTried = false;

    protected override void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        if (mainRenderer == null)
        {
            Debug.LogError("Animal needs SpriteRenderer!");
            return;
        }

        originalPos = transform.position;
        originalScale = transform.localScale;
        originalColor = mainRenderer.color;

        FindPlayer();
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (player == null && !playerSearchTried)
        {
            FindPlayer();
            playerSearchTried = true;
        }
    }

    private void FindPlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    protected override void GlitchedBehavior()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // Chase if in range
        if (distToPlayer < chaseRange && distToPlayer > 0.5f)
        {
            Vector3 direction3D = (player.position - transform.position).normalized;
            transform.position += direction3D * chaseSpeed * Time.deltaTime;
            mainRenderer.flipX = direction3D.x < 0;
        }

        // Apply glitch visuals
        ApplyGlitchVisuals();
    }

    protected override void NormalBehavior()
    {
        // CRITICAL FIX: Reset position and visuals to original
        transform.position = originalPos;
        transform.localScale = originalScale;
        mainRenderer.color = originalColor;
        mainRenderer.flipX = false;
    }

    private void ApplyGlitchVisuals()
    {
        // Shake
        float shakeX = Mathf.Sin(Time.time * glitchFlickerSpeed * 1.8f) * glitchShakeIntensity;
        float shakeY = Mathf.Cos(Time.time * glitchFlickerSpeed * 2.1f) * glitchShakeIntensity * 0.6f;
        Vector3 glitchOffset = new Vector3(shakeX, shakeY, 0);
        transform.position = (Vector3)originalPos + glitchOffset;  // FIX: Use position, not localPosition

        // Color flicker
        float flicker = (Mathf.Sin(Time.time * glitchFlickerSpeed) + 1f) * 0.5f;
        mainRenderer.color = Color.Lerp(originalColor, glitchColor, flicker);

        // Scale pulse
        float pulse = 1f + Mathf.Sin(Time.time * glitchFlickerSpeed * 1.2f) * 0.06f;
        transform.localScale = originalScale * pulse;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && isGlitched)
        {
            var playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.Die();
            }
        }
    }

    public override void SetGlitched(bool glitched)
    {
        base.SetGlitched(glitched);
        // FIX: Ensure position resets immediately when unglitched
        if (!glitched)
        {
            transform.position = originalPos;
            transform.localScale = originalScale;
            mainRenderer.color = originalColor;
            mainRenderer.flipX = false;
        }
    }
}