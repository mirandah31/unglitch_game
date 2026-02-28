using UnityEngine;

public abstract class Glitchable : MonoBehaviour
{
    public bool isGlitched = true; // Default: glitched (random/attack behavior)
    public bool canBeUnglitched = true; // Some objects might not allow it

    protected virtual void Update()
    {
        if (isGlitched)
        {
            GlitchedBehavior();
        }
        else
        {
            NormalBehavior();
        }
    }

    public void ToggleGlitch()
    {
        if (canBeUnglitched)
        {
            isGlitched = !isGlitched;
            // Add visual glitch effect toggle here (e.g., enable/disable a glitch shader or particle system)
            // Example: GetComponent<SpriteRenderer>().material = isGlitched ? glitchMaterial : normalMaterial;
        }
    }

    protected abstract void GlitchedBehavior(); // Random/attack
    protected abstract void NormalBehavior(); // Predictable/passive
}