using UnityEngine;
using UnityEngine.SceneManagement;

// Handles main menu actions like starting and quitting the game
public class MainMenuController : MonoBehaviour
{
    // Start the game by loading the main scene
    public void StartGame()
    {
        SceneManager.LoadScene("MainScene"); // Replace "MainScene" with your scene name
    }

    // Quit the game application
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
