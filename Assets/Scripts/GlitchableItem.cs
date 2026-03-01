using UnityEngine;

public class GlitchableItem : Glitchable
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
    public float glitchShakeIntensity = 0.03f;
    public float glitchFlickerSpeed = 8f;
    public Color glitchColor = new Color(1f, 0.2f, 0.6f, 0.7f);

    private Transform player;
    private SpriteRenderer mainRenderer;
    private Rigidbody2D rb;
    private Vector3 originalPos;
    private Vector3 originalScale;
    private Color originalColor;
    private Vector3 pointAPos;
    private Vector3 pointBPos;
    private bool movingToPointB = true;
    private Vector3 visualOffset;

    protected override void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (mainRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: Needs a SpriteRenderer!");
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
            }
        }
        
        base.Start();
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
        if (mainRenderer == null) return;
        
        if (isAnimal)
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

                if (direction.x != 0 && mainRenderer != null)
                    mainRenderer.flipX = direction.x < 0;
            }
            else if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else
        {
            if (pointA == null || pointB == null) return;
            
            Vector3 target = glitchedReverseDirection 
                ? (movingToPointB ? pointAPos : pointBPos)
                : (movingToPointB ? pointBPos : pointAPos);
            
            Vector3 newPos = Vector3.MoveTowards(transform.position - visualOffset, target, glitchedSpeed * Time.deltaTime);
            transform.position = newPos + visualOffset;
            
            if (Vector3.Distance(newPos, target) < 0.01f)
            {
                transform.position = target + visualOffset;
                movingToPointB = !movingToPointB;
            }
        }
        
        float shakeX = Mathf.Sin(Time.time * glitchFlickerSpeed * 1.5f) * glitchShakeIntensity;
        float shakeY = Mathf.Cos(Time.time * glitchFlickerSpeed * 1.8f) * glitchShakeIntensity * 0.5f;
        visualOffset = new Vector3(shakeX, shakeY, 0);
        
        float flicker = (Mathf.Sin(Time.time * glitchFlickerSpeed) + 1f) * 0.5f;
        mainRenderer.color = Color.Lerp(originalColor, glitchColor, flicker * 0.7f);

        float pulse = 1f + Mathf.Sin(Time.time * glitchFlickerSpeed * 1.2f) * 0.02f;
        transform.localScale = originalScale * pulse;
    }

    protected override void NormalBehavior()
    {
        if (mainRenderer == null) return;
        
        visualOffset = Vector3.zero;
        
        if (isAnimal)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else
        {
            if (pointA == null || pointB == null) return;
            
            Vector3 target = movingToPointB ? pointBPos : pointAPos;
            Vector3 newPos = Vector3.MoveTowards(transform.position, target, normalSpeed * Time.deltaTime);
            transform.position = newPos;
            
            if (Vector3.Distance(newPos, target) < 0.01f)
            {
                transform.position = target;
                movingToPointB = !movingToPointB;
            }
        }
        
        mainRenderer.color = originalColor;
        transform.localScale = originalScale;
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
}