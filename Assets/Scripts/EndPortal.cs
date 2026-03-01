using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPortal : MonoBehaviour
{
    public string Level2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Mark game as completed before loading next level
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameCompleted(true);
            }
            SceneManager.LoadScene(Level2);
        }
    }
}