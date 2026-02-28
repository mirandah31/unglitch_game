using UnityEngine;

public abstract class Glitchable : MonoBehaviour
{
    public bool isGlitched = true;
    public bool canBeUnglitched = true;

    [SerializeField]
    protected SpriteRenderer glowRenderer;   // the child glow object

    public virtual void SetGlitched(bool glitched)
    {
        if (!canBeUnglitched && !glitched) return;
        isGlitched = glitched;
    }

    public virtual void SetSelected(bool selected)
    {
        if (glowRenderer != null)
        {
            glowRenderer.enabled = selected;
            // Optional: add a little pulse when selected
            if (selected)
            {
                glowRenderer.color = Color.cyan; // Bright cyan for visibility
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No glowRenderer assigned!");
        }
    }

    protected abstract void GlitchedBehavior();
    protected abstract void NormalBehavior();

    protected virtual void Update()
    {
        if (isGlitched) GlitchedBehavior();
        else NormalBehavior();
    }

    protected virtual void Start()
    {
        // Auto-assign glow if not set and child "Glow" exists
        if (glowRenderer == null)
        {
            Transform glowChild = transform.Find("Glow");
            if (glowChild != null)
            {
                glowRenderer = glowChild.GetComponent<SpriteRenderer>();
                if (glowRenderer != null)
                {
                    // Ensure glow starts hidden
                    glowRenderer.enabled = false;
                    Debug.Log($"{gameObject.name}: Glow found and initialized");
                }
                else
                {
                    Debug.LogError($"{gameObject.name}: Found 'Glow' child but no SpriteRenderer attached!");
                }
            }
            else
            {
                Debug.LogError($"{gameObject.name}: No 'Glow' child found! Please create a child GameObject named 'Glow' with a SpriteRenderer.");
            }
        }
        else
        {
            // If manually assigned, ensure it starts hidden
            glowRenderer.enabled = false;
        }
    }
}