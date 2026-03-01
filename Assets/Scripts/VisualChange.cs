using UnityEngine;

public class VisualChange : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"VisualChange triggered by: {collision.gameObject.name} with tag: {collision.gameObject.tag}");
        
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player detected! Trying to enable glitch...");
            
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log("PlayerController found! Enabling player glitch...");
                player.EnablePlayerGlitch(true);
            }
            else
            {
                Debug.LogError("PlayerController component not found on player!");
            }
        }
    }
}