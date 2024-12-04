using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.UIElements.Experimental;  // Add this for Easing

// Manages UI elements such as health bar and NPC dialogue
public class UIHandler : MonoBehaviour
{
    // Public variables
    public float displayTime = 4.0f; // Duration to display NPC dialogue

    // Private variables
    private VisualElement m_Healthbar;        // Health bar UI element
    private VisualElement m_DialogueContainer; // Dialogue container
    private VisualElement m_DialoguePanel;
    private VisualElement m_NPCPortrait;
    private Label m_NPCName;
    private Label m_DialogueText;
    private VisualElement m_ContinueIndicator;
    private float m_TimerDisplay;
    private ProgressBar m_XPBar;
    private Label m_XPText;
    private Label m_XPProgress;
    private VisualElement m_XPBarFill;
    private VisualElement m_LevelUpContainer;
    private VisualElement m_LevelUpPanel;
    private VisualElement m_LevelUpOuterGlow;
    private Label m_LevelUpTitle;
    private Label m_LevelNumber;
    private Label m_LevelUpStats;
    private float m_LevelUpRotation = 0f;
    private Label m_AttackSpeedValue;
    private Label m_MovementSpeedValue;
    private Label m_StatPointsValue;
    private Label m_LevelUpText;
    private VisualElement m_InteractionIndicator;

    [Header("Level Up Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float displayDuration = 2.0f;
    public float fadeOutDuration = 0.5f;
    public float rotationSpeed = 50f;

    // Singleton instance for global access
    public static UIHandler instance;

    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement gameUI;

    private float m_LevelUpDisplayTime = 2f;  // Time in seconds to show the level up UI
    private float m_LevelUpTimer = 0f;
    private bool m_IsLevelUpVisible = false;

    void Awake()
    {
        try
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Get the UIDocument component
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogError("UIDocument component not found! Please add a UIDocument component and assign the UXML asset.");
                    return;
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            Debug.Log("UIHandler Awake completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UIHandler Awake: {e.Message}\n{e.StackTrace}");
        }
    }

    void Start()
    {
        try
        {
            StartCoroutine(InitializeUIWithRetry());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UIHandler Start: {e.Message}\n{e.StackTrace}");
        }
    }

    private IEnumerator InitializeUIWithRetry()
    {
        int attempts = 0;
        const int maxAttempts = 3;

        while (attempts < maxAttempts)
        {
            Debug.Log($"Attempt {attempts + 1} to initialize UI");
            if (TryInitializeUI())
            {
                Debug.Log("UI successfully initialized!");
                yield break;
            }
            attempts++;
            yield return new WaitForSeconds(0.5f);
        }
        Debug.LogError("Failed to initialize UI after multiple attempts");
    }

    private bool TryInitializeUI()
    {
        try
        {
            Debug.Log("Attempting to initialize UI...");
            
            // Get UIDocument if not already assigned
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                Debug.Log($"UIDocument component {(uiDocument != null ? "found" : "not found")}");
            }

            if (uiDocument == null)
            {
                Debug.LogError("UIDocument is null! Please ensure UIDocument component is attached and configured.");
                return false;
            }

            root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("Root element is null! Please ensure the UXML asset is assigned in the UIDocument component.");
                return false;
            }

            // Find GameUI
            gameUI = root.Q<VisualElement>("GameUI");
            if (gameUI == null)
            {
                Debug.LogError("GameUI element not found in UXML! Please ensure your UXML has a root element named 'GameUI'");
                return false;
            }

            Debug.Log("GameUI found. Looking for dialogue elements...");

            // Find NPCDialoguePanel first
            var dialoguePanel = gameUI.Q<VisualElement>("NPCDialoguePanel");
            if (dialoguePanel == null)
            {
                Debug.LogError("NPCDialoguePanel not found under GameUI!");
                return false;
            }

            // Find DialogueContainer under NPCDialoguePanel
            m_DialogueContainer = dialoguePanel.Q<VisualElement>("DialogueContainer");
            if (m_DialogueContainer == null)
            {
                Debug.LogError("DialogueContainer not found under NPCDialoguePanel!");
                return false;
            }

            // Find DialoguePanel under DialogueContainer
            m_DialoguePanel = m_DialogueContainer.Q<VisualElement>("DialoguePanel");
            
            // Find other elements under DialoguePanel
            if (m_DialoguePanel != null)
            {
                m_NPCPortrait = m_DialoguePanel.Q<VisualElement>("NPCPortrait");
                var dialogueContent = m_DialoguePanel.Q<VisualElement>("DialogueContent");
                if (dialogueContent != null)
                {
                    m_NPCName = dialogueContent.Q<Label>("NPCName");
                    m_DialogueText = dialogueContent.Q<Label>("DialogueText");
                }
                m_ContinueIndicator = m_DialoguePanel.Q<VisualElement>("ContinueIndicator");
            }

            // Log what we found
            Debug.Log($"Found UI elements: " +
                $"\nDialogueContainer: {m_DialogueContainer != null}" +
                $"\nDialoguePanel: {m_DialoguePanel != null}" +
                $"\nNPCPortrait: {m_NPCPortrait != null}" +
                $"\nNPCName: {m_NPCName != null}" +
                $"\nDialogueText: {m_DialogueText != null}" +
                $"\nContinueIndicator: {m_ContinueIndicator != null}");

            // Verify critical elements
            if (m_DialogueContainer == null || m_NPCName == null || m_DialogueText == null)
            {
                Debug.LogError("Critical dialogue UI elements are missing!");
                return false;
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in TryInitializeUI: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    private void InitializeDialogueElements()
    {
        if (m_DialogueContainer == null) return;

        // Create a new dialogue panel if it doesn't exist
        if (m_DialoguePanel == null)
        {
            Debug.Log("Creating new dialogue panel");
            m_DialoguePanel = new VisualElement
            {
                name = "DialoguePanel",
                style =
                {
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f),
                    paddingTop = 15,
                    paddingBottom = 15,
                    paddingLeft = 15,
                    paddingRight = 15,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                }
            };
            m_DialogueContainer.Add(m_DialoguePanel);
        }

        // Create or update NPC name label
        if (m_NPCName == null)
        {
            m_NPCName = new Label
            {
                name = "NPCName",
                style =
                {
                    color = Color.white,
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 5
                }
            };
            m_DialoguePanel.Add(m_NPCName);
        }

        // Create or update dialogue text label
        if (m_DialogueText == null)
        {
            m_DialogueText = new Label
            {
                name = "DialogueText",
                style =
                {
                    color = Color.white,
                    fontSize = 16,
                    whiteSpace = WhiteSpace.Normal,
                    marginTop = 5
                }
            };
            m_DialoguePanel.Add(m_DialogueText);
        }

        // Initially hide the dialogue
        m_DialogueContainer.style.display = DisplayStyle.None;
    }

    private void InitializeHealthBar(VisualElement gameUI)
    {
        var healthBarContainer = gameUI.Q("HealthBarContainer");
        if (healthBarContainer != null)
        {
            m_Healthbar = healthBarContainer.Q("HealthBar");
            if (m_Healthbar == null)
            {
                Debug.LogError("HealthBar element not found in HealthBarContainer");
            }
            else
            {
                Debug.Log("Health bar initialized successfully");
            }
        }
        else
        {
            Debug.LogError("HealthBarContainer not found in GameUI");
        }
    }

    private void InitializeXPBar(VisualElement gameUI)
    {
        var xpBarInstance = gameUI.Q("XPBar");
        if (xpBarInstance == null)
        {
            xpBarInstance = new VisualElement { name = "XPBar" };
            gameUI.Add(xpBarInstance);
        }

        m_XPBar = xpBarInstance.Q<ProgressBar>("XPBar");
        if (m_XPBar == null)
        {
            m_XPBar = new ProgressBar { name = "XPBar" };
            xpBarInstance.Add(m_XPBar);
        }

        m_XPProgress = xpBarInstance.Q<Label>("XPProgress");
        if (m_XPProgress == null)
        {
            m_XPProgress = new Label { name = "XPProgress", text = "" };
            xpBarInstance.Add(m_XPProgress);
        }

        m_XPBarFill = m_XPBar?.Q<VisualElement>("XPBarFill");
        if (m_XPBarFill == null)
        {
            m_XPBarFill = new VisualElement { name = "XPBarFill" };
            m_XPBar.Add(m_XPBarFill);
        }

        m_XPText = xpBarInstance.Q<Label>("XPText");
        if (m_XPText == null)
        {
            m_XPText = new Label { name = "XPText", text = "" };
            xpBarInstance.Add(m_XPText);
        }
    }

    private void InitializeStatsPanel(VisualElement gameUI)
    {
        var statsPanel = gameUI.Q<VisualElement>("StatsPanel");
        if (statsPanel == null)
        {
            statsPanel = new VisualElement { name = "StatsPanel" };
            gameUI.Add(statsPanel);
        }

        m_AttackSpeedValue = statsPanel.Q<Label>("AttackSpeedValue");
        if (m_AttackSpeedValue == null)
        {
            m_AttackSpeedValue = new Label { name = "AttackSpeedValue", text = "" };
            statsPanel.Add(m_AttackSpeedValue);
        }

        m_MovementSpeedValue = statsPanel.Q<Label>("MovementSpeedValue");
        if (m_MovementSpeedValue == null)
        {
            m_MovementSpeedValue = new Label { name = "MovementSpeedValue", text = "" };
            statsPanel.Add(m_MovementSpeedValue);
        }

        m_StatPointsValue = statsPanel.Q<Label>("StatPointsValue");
        if (m_StatPointsValue == null)
        {
            m_StatPointsValue = new Label { name = "StatPointsValue", text = "" };
            statsPanel.Add(m_StatPointsValue);
        }
    }

    private void InitializeLevelUpUI()
    {
        if (m_LevelUpContainer == null)
        {
            var root = GetComponent<UIDocument>()?.rootVisualElement;
            if (root != null)
            {
                var levelUpInstance = root.Q("LevelUpInstance");
                if (levelUpInstance != null)
                {
                    m_LevelUpContainer = levelUpInstance.Q<VisualElement>("LevelUpContainer");
                    if (m_LevelUpContainer != null)
                    {
                        m_LevelUpPanel = m_LevelUpContainer.Q<VisualElement>("LevelUpPanel");
                        m_LevelUpOuterGlow = m_LevelUpContainer.Q<VisualElement>("LevelUpOuterGlow");
                        m_LevelUpTitle = m_LevelUpContainer.Q<Label>("LevelUpTitle");
                        m_LevelNumber = m_LevelUpContainer.Q<Label>("LevelNumber");
                        m_LevelUpStats = m_LevelUpContainer.Q<Label>("LevelUpStats");
                        Debug.Log("Level Up UI initialized successfully");
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        try
        {
            // Subscribe to player events
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.onXPChanged += UpdateXPBar;
                player.onLevelUp += ShowLevelUpAnimation;
                player.onStatPointsChanged += UpdateStatPoints;
                Debug.Log("Successfully subscribed to player events");
            }
            else
            {
                Debug.LogError("Could not find PlayerController!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnEnable: {e.Message}\n{e.StackTrace}");
        }
    }

    void OnDisable()
    {
        try
        {
            // Unsubscribe from player events
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.onXPChanged -= UpdateXPBar;
                player.onLevelUp -= ShowLevelUpAnimation;
                player.onStatPointsChanged -= UpdateStatPoints;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnDisable: {e.Message}");
        }
    }

    public void ShowDialogue(string npcName, string text, Sprite portrait)
    {
        try
        {
            Debug.Log("ShowDialogue called - checking UI elements...");
            
            // Try to initialize if elements are null
            if (m_DialogueContainer == null || m_NPCName == null || m_DialogueText == null)
            {
                Debug.Log("UI elements not initialized, attempting to initialize...");
                if (!TryInitializeUI())
                {
                    Debug.LogError("Failed to initialize UI elements!");
                    return;
                }
            }

            // Double check after initialization attempt
            if (m_DialogueContainer == null || m_NPCName == null || m_DialogueText == null)
            {
                Debug.LogError($"Dialogue UI elements still not initialized! Container: {m_DialogueContainer}, Name: {m_NPCName}, Text: {m_DialogueText}");
                return;
            }

            // Set the content
            m_NPCName.text = npcName;
            m_DialogueText.text = text;
            
            // Update portrait if available
            if (m_NPCPortrait != null && portrait != null)
            {
                m_NPCPortrait.style.backgroundImage = new StyleBackground(portrait);
            }

            // Show the dialogue container
            m_DialogueContainer.style.display = DisplayStyle.Flex;
            
            Debug.Log("Dialogue UI elements should now be visible");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ShowDialogue: {e.Message}\n{e.StackTrace}");
        }
    }

    // Simple version for backward compatibility
    public void ShowDialogue(string text)
    {
        ShowDialogue("NPC", text, null);
    }

    public void HideDialogue()
    {
        try
        {
            if (m_DialogueContainer != null)
            {
                m_DialogueContainer.style.display = DisplayStyle.None;
                
                // Clear the text
                if (m_DialogueText != null) m_DialogueText.text = "";
                if (m_NPCName != null) m_NPCName.text = "";
                
                Debug.Log("Dialogue container hidden");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error hiding dialogue: {e.Message}\n{e.StackTrace}");
        }
    }

    private void HideLevelUpUI()
    {
        try
        {
            if (m_LevelUpContainer == null)
            {
                InitializeLevelUpUI();
            }

            if (m_LevelUpContainer != null)
            {
                m_LevelUpContainer.style.display = DisplayStyle.None;
                m_IsLevelUpVisible = false;
                Debug.Log("Level Up UI hidden");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error hiding level up UI: {e.Message}");
        }
    }

    public void ShowLevelUpAnimation()
    {
        try
        {
            Debug.Log("Showing Level Up Animation");
            
            if (m_LevelUpContainer == null)
            {
                InitializeLevelUpUI();
            }

            if (m_LevelUpContainer == null)
            {
                Debug.LogWarning("LevelUpContainer not found - skipping animation");
                return;
            }

            // Get player level
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                m_LevelNumber.text = $"LEVEL {player.level}";
                
                // Set stats text based on level
                switch (player.level)
                {
                    case 2:
                        m_LevelUpStats.text = "Growing stronger!";
                        break;
                    case 3:
                        m_LevelUpStats.text = "Getting powerful!";
                        break;
                    case 4:
                        m_LevelUpStats.text = "Unstoppable!";
                        break;
                    default:
                        m_LevelUpStats.text = "Legendary!";
                        break;
                }
            }

            // Show the container and start the timer
            m_LevelUpContainer.style.display = DisplayStyle.Flex;
            m_LevelUpTimer = m_LevelUpDisplayTime;
            m_IsLevelUpVisible = true;
            Debug.Log("Level Up UI shown, timer started");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ShowLevelUpAnimation: {e.Message}\n{e.StackTrace}");
        }
    }

    private void Update()
    {
        // Handle dialogue timer
        if (m_DialogueContainer != null && m_DialogueContainer.style.display == DisplayStyle.Flex)
        {
            if (m_TimerDisplay > 0)
            {
                m_TimerDisplay -= Time.deltaTime;
                if (m_TimerDisplay <= 0)
                {
                    m_DialogueContainer.style.display = DisplayStyle.None;
                }
            }
        }

        // Handle level up UI timer
        if (m_IsLevelUpVisible)
        {
            if (m_LevelUpTimer > 0)
            {
                m_LevelUpTimer -= Time.deltaTime;
                if (m_LevelUpTimer <= 0)
                {
                    HideLevelUpUI();
                    Debug.Log("Level Up timer expired, hiding UI");
                }
            }
        }
    }

    public void SetHealthValue(float percentage)
    {
        try
        {
            if (m_Healthbar == null)
            {
                var root = GetComponent<UIDocument>()?.rootVisualElement;
                if (root != null)
                {
                    var healthBarContainer = root.Q("HealthBarContainer");
                    if (healthBarContainer != null)
                    {
                        m_Healthbar = healthBarContainer.Q("HealthBar");
                    }
                }
            }

            if (m_Healthbar != null)
            {
                percentage = Mathf.Clamp01(percentage);
                m_Healthbar.style.width = Length.Percent(percentage * 100);
                Debug.Log($"Health updated to {percentage * 100}%");
            }
            else
            {
                Debug.LogError("Health bar not found when trying to update health");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating health bar: {e.Message}");
        }
    }

    public void UpdateXPBar(int currentXP, int maxXP)
    {
        try
        {
            if (m_XPBar == null)
            {
                var root = GetComponent<UIDocument>()?.rootVisualElement;
                if (root != null)
                {
                    var xpBarInstance = root.Q("XPBarContainer");
                    if (xpBarInstance != null)
                    {
                        m_XPBar = xpBarInstance.Q<ProgressBar>("XPBar");
                        m_XPProgress = xpBarInstance.Q<Label>("XPProgress");
                        m_XPBarFill = m_XPBar?.Q<VisualElement>("XPBarFill");
                        m_XPText = xpBarInstance.Q<Label>("XPText");
                    }
                }
            }

            if (m_XPBar != null)
            {
                float percentage = Mathf.Clamp01((float)currentXP / maxXP) * 100f;
                m_XPBar.value = percentage;

                if (m_XPProgress != null)
                {
                    m_XPProgress.text = $"{currentXP}/{maxXP} XP";
                }

                if (m_XPBarFill != null)
                {
                    m_XPBarFill.style.width = Length.Percent(percentage);
                }

                // Update level text
                if (m_XPText != null)
                {
                    PlayerController player = FindObjectOfType<PlayerController>();
                    if (player != null)
                    {
                        m_XPText.text = $"Level {player.level}";
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating XP bar: {e.Message}");
        }
    }

    public void UpdateAttackSpeed(float speedMultiplier)
    {
        if (m_AttackSpeedValue != null)
        {
            int percentage = Mathf.RoundToInt((speedMultiplier - 1) * 100);
            m_AttackSpeedValue.text = $"{percentage}%";
        }
    }

    public void UpdateMovementSpeed(float speedMultiplier)
    {
        if (m_MovementSpeedValue != null)
        {
            int percentage = Mathf.RoundToInt((speedMultiplier - 1) * 100);
            m_MovementSpeedValue.text = $"{percentage}%";
        }
    }

    public void UpdateStatPoints(int points)
    {
        try
        {
            if (m_StatPointsValue == null)
            {
                var uiDocument = GetComponent<UIDocument>();
                if (uiDocument != null && uiDocument.rootVisualElement != null)
                {
                    var gameUI = uiDocument.rootVisualElement.Q<VisualElement>("GameUI");
                    if (gameUI != null)
                    {
                        var statsPanel = gameUI.Q<VisualElement>("StatsPanel");
                        if (statsPanel != null)
                        {
                            m_StatPointsValue = statsPanel.Q<Label>("StatPointsValue");
                        }
                    }
                }
            }

            if (m_StatPointsValue != null)
            {
                m_StatPointsValue.text = points.ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateStatPoints: {e.Message}");
        }
    }

    public void ShowLevelUpEffect()
    {
        try
        {
            var levelUpElement = GetComponent<UIDocument>()?.rootVisualElement.Q<VisualElement>("LevelUpEffect");
            if (levelUpElement != null)
            {
                levelUpElement.style.display = DisplayStyle.Flex;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error showing level up effect: {e.Message}");
        }
    }

    public void ShowVictoryScreen()
    {
        Debug.Log("Showing Victory Screen");
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) return;

        var victoryScreen = root.Q("VictoryScreen");
        if (victoryScreen == null)
        {
            // Create victory screen if it doesn't exist
            victoryScreen = new VisualElement();
            victoryScreen.name = "VictoryScreen";
            victoryScreen.style.position = Position.Absolute;
            victoryScreen.style.width = Length.Percent(100);
            victoryScreen.style.height = Length.Percent(100);
            victoryScreen.style.backgroundColor = new Color(0, 0, 0, 0.8f);
            victoryScreen.style.alignItems = Align.Center;
            victoryScreen.style.justifyContent = Justify.Center;
            root.Add(victoryScreen);

            // Create container for victory content
            var container = new VisualElement();
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            container.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            container.style.borderTopLeftRadius = 20;
            container.style.borderTopRightRadius = 20;
            container.style.borderBottomLeftRadius = 20;
            container.style.borderBottomRightRadius = 20;
            container.style.paddingTop = 30;
            container.style.paddingBottom = 30;
            container.style.paddingLeft = 30;
            container.style.paddingRight = 30;
            victoryScreen.Add(container);

            // Victory title
            var titleLabel = new Label();
            titleLabel.text = "VICTORY!";
            titleLabel.style.fontSize = 48;
            titleLabel.style.color = new Color(1f, 0.84f, 0f); // Gold color
            titleLabel.style.marginBottom = 20;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            container.Add(titleLabel);

            // Congratulations text
            var congratsLabel = new Label();
            congratsLabel.text = "All enemies have been fixed!";
            congratsLabel.style.fontSize = 24;
            congratsLabel.style.color = Color.white;
            congratsLabel.style.marginBottom = 30;
            container.Add(congratsLabel);

            // Restart instruction
            var restartLabel = new Label();
            restartLabel.text = "Press R to Restart";
            restartLabel.style.fontSize = 20;
            restartLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            restartLabel.style.marginTop = 20;
            container.Add(restartLabel);
        }

        victoryScreen.style.display = DisplayStyle.Flex;
    }

    public void HideVictoryScreen()
    {
        Debug.Log("Hiding Victory Screen");
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) return;

        var victoryScreen = root.Q("VictoryScreen");
        if (victoryScreen != null)
        {
            victoryScreen.style.display = DisplayStyle.None;
            root.Remove(victoryScreen);
        }
    }

    public void ShowGameOverScreen()
    {
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) return;

        // Create the game over screen container
        var gameOverScreen = new VisualElement
        {
            name = "GameOverScreen",
            style =
            {
                position = Position.Absolute,
                left = 0,
                right = 0,
                top = 0,
                bottom = 0,
                backgroundColor = new Color(0, 0, 0, 0.8f),
                alignItems = Align.Center,
                justifyContent = Justify.Center
            }
        };

        // Create the content container with larger size
        var container = new VisualElement
        {
            style =
            {
                backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f),
                borderTopLeftRadius = 20,
                borderTopRightRadius = 20,
                borderBottomLeftRadius = 20,
                borderBottomRightRadius = 20,
                paddingTop = 40,
                paddingBottom = 40,
                paddingLeft = 60,
                paddingRight = 60,
                alignItems = Align.Center,
                justifyContent = Justify.Center,
                width = 600,  // Increased width
                height = 400  // Increased height
            }
        };

        // Create and style the game over text with animation
        var gameOverText = new Label("GAME OVER")
        {
            style =
            {
                fontSize = 72,  // Larger font
                color = new Color(0.8f, 0.2f, 0.2f, 1),  // Red color
                marginBottom = 20,
                unityFontStyleAndWeight = FontStyle.Bold,
                scale = new Scale(new Vector3(1, 1, 1))
            }
        };

        // Create the restart instruction text
        var restartText = new Label("Press R to Try Again")
        {
            style =
            {
                fontSize = 24,
                color = Color.white,
                marginTop = 30
            }
        };

        // Add elements to the container
        container.Add(gameOverText);
        container.Add(restartText);
        gameOverScreen.Add(container);

        // Add to root with initial state
        root.Add(gameOverScreen);
        gameOverScreen.style.display = DisplayStyle.Flex;
    }

    public void HideGameOverScreen()
    {
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) return;

        var gameOverScreen = root.Q("GameOverScreen");
        if (gameOverScreen != null)
        {
            gameOverScreen.style.display = DisplayStyle.None;
            root.Remove(gameOverScreen);
        }
    }

    public void ShowInteractionIndicator()
    {
        if (m_InteractionIndicator != null)
        {
            m_InteractionIndicator.style.display = DisplayStyle.Flex;
        }
    }

    public void HideInteractionIndicator()
    {
        if (m_InteractionIndicator != null)
        {
            m_InteractionIndicator.style.display = DisplayStyle.None;
        }
    }
}
