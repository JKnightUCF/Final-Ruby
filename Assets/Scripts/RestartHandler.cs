using UnityEngine;
using UnityEngine.SceneManagement;

// Handles game restart functionality after game over
public class RestartHandler : MonoBehaviour
{
    private bool GameOver = false;

    // Enable restart functionality
    public void EnableRestart()
    {
        GameOver = true;
        Debug.Log("Restart enabled. Press R to restart the game.");
    }

    // Check for restart input
    private void Update()
    {
        if (GameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    // Restart the game by reloading the main scene
    private void RestartGame()
    {
        Debug.Log("Restarting Game...");
        SceneManager.LoadScene("MainScene"); // Replace "MainScene" with your scene name
    }
}
