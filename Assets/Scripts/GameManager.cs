using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ending State")]
    public bool gameCompleted = false; // Track if player has finished the game
    public bool playerGlitched = false; // Track if player should be glitched

    private void Awake()
    {
        // Singleton pattern - keep this object between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameCompleted(bool completed)
    {
        gameCompleted = completed;
        playerGlitched = completed; // When game completed, player becomes glitched
    }
}