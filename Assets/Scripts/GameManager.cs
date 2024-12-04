using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Manages game state, including tracking enemies and handling the game over screen
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Enemy tracking
    private int enemiesFixed = 0;
    public int totalEnemies = 4;

    // Game over handling
    private VisualElement gameOverScreen;
    public RestartHandler restartHandler;
    public bool IsGameOver { get; private set; } = false;

    // Ensure only one instance of GameManager exists
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Initialize UI and game over elements
    private void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        restartHandler = GetComponent<RestartHandler>();

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            gameOverScreen = root.Q<VisualElement>("GameOverScreen");

            if (gameOverScreen != null)
                gameOverScreen.style.display = DisplayStyle.None; // Hide initially
            else
                Debug.LogError("GameOverScreen element not found in UI!");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    // Increment fixed enemies and check for game over
    public void EnemyFixed()
    {
        enemiesFixed++;
        Debug.Log($"Enemies fixed: {enemiesFixed}/{totalEnemies}");

        if (enemiesFixed >= totalEnemies)
        {
            ShowVictoryScreen();
        }
    }

    // Display victory screen
    private void ShowVictoryScreen()
    {
        if (!IsGameOver)
        {
            IsGameOver = true;
            Time.timeScale = 0f; // Freeze everything
            
            if (UIHandler.instance != null)
            {
                UIHandler.instance.ShowVictoryScreen();
            }
        }
    }

    // Restart the game
    public void RestartGame()
    {
        if (UIHandler.instance != null)
        {
            UIHandler.instance.HideVictoryScreen();
        }
        
        Time.timeScale = 1f; // Unfreeze everything
        IsGameOver = false;
        enemiesFixed = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
