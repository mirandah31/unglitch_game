using UnityEngine;

public abstract class Glitchable : MonoBehaviour
{
    public bool isGlitched = true;
    public bool canBeUnglitched = true;

    [SerializeField]
    protected SpriteRenderer glowRenderer;

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
            Debug.Log($"{gameObject.name} glow set to: {selected}"); // Watch this in console
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
        if (glowRenderer == null)
        {
            Transform glowChild = transform.Find("Glow");
            if (glowChild != null)
            {
                glowRenderer = glowChild.GetComponent<SpriteRenderer>();
                if (glowRenderer != null)
                {
                    // CRITICAL: Start disabled
                    glowRenderer.enabled = false;
                    Debug.Log($"{gameObject.name}: Glow found and disabled by default");
                }
                else
                {
                    Debug.LogError($"{gameObject.name}: Glow child has no SpriteRenderer!");
                }
            }
            else
            {
                Debug.LogError($"{gameObject.name}: No child named 'Glow' found!");
            }
        }
        else
        {
            // If manually assigned, ensure it starts disabled
            glowRenderer.enabled = false;
            Debug.Log($"{gameObject.name}: Glow manually assigned and disabled");
        }
    }
}