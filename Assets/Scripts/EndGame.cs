using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class EndGame : MonoBehaviour
{
    public GameObject winUI;
    public string mainMenuSceneName = "MainMenu";
    
    // Find all glitchable objects in the scene
    private List<Glitchable> allGlitchables = new List<Glitchable>();

    private void Start()
    {
        // Find all glitchable objects at start
        FindAllGlitchables();
    }

    private void FindAllGlitchables()
    {
        allGlitchables.Clear();
        Glitchable[] glitchables = FindObjectsByType<Glitchable>(FindObjectsSortMode.None);
        allGlitchables.AddRange(glitchables);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Find all glitchables again in case new ones were added
            FindAllGlitchables();
            
            // Un-glitch all glitchable objects in the scene
            foreach (Glitchable glitchable in allGlitchables)
            {
                if (glitchable != null)
                {
                    glitchable.SetGlitched(false);
                }
            }
            
            // Mark game as fully completed
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameCompleted(true);
            }
            
            // Show win UI
            Time.timeScale = 0;
            winUI.SetActive(true);
        }
    }

    // Called by HomeButton
    public void OnHomeButtonClick()
    {
        Time.timeScale = 1; // Resume time
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Called by PlayAgain button if you have one
    public void OnPlayAgainClick()
    {
        Time.timeScale = 1; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }
}