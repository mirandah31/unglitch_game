using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndGame : MonoBehaviour
{
    public GameObject winUI;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == 'Player')
        {
            Time.timeScale = 0;
            winUI.SetActive(true);
        }
    }
}