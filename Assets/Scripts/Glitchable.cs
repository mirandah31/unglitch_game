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
            // FIX: Default to hidden, only show when selected
            glowRenderer.enabled = selected;
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
            glowRenderer = transform.Find("Glow")?.GetComponent<SpriteRenderer>();
        }
        
        // FIX: Ensure glow starts hidden
        if (glowRenderer != null)
        {
            glowRenderer.enabled = false;
        }
    }
}