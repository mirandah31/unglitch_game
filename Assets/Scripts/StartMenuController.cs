using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void onStartClick()
    {
        SceneManager.LoadScene("TestScene");
    }
        
}
