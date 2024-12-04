using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Controls player movement, health, combat, and interactions
public class PlayerController : MonoBehaviour
{
    // Movement variables
    public InputAction MoveAction;
    public float speed = 3.0f;
    private Rigidbody2D rigidbody2d;
    private Vector2 move;
    private Vector2 moveDirection = new Vector2(1, 0);
    public AudioClip[] footstepClips;
    public float footstepInterval = 0.5f;
    private float footstepTimer;
    public int currentXP = 0; // Current experience points
    public int xpToNextLevel = 100; // XP required to level up
    public int level = 1; // Player's level
    public int statPoints = 0; // Available stat points
    public System.Action onLevelUp; // Event for level up
    public System.Action<int, int> onXPChanged; // Event for XP changes (current, max)
    public System.Action<int> onStatPointsChanged; // Event for stat points changes

    // Health system
    public int maxHealth = 5;
    private int currentHealth;
    public int health { get { return currentHealth; } }
    public float timeInvincible = 2.0f;
    private bool isInvincible;
    private float damageCooldown;
    public AudioClip hurtSound;

    // Animation and combat
    private Animator animator;
    public GameObject projectilePrefab;
    public InputAction launchAction;
    public InputAction talkAction;
    public BloodOverlayController bloodOverlay;

    // Audio
    private AudioSource audioSource;

    // NPC interaction
    private bool isNearNPC = false;
    private NonPlayerCharacter currentNPC = null;

    void Awake()
    {
        // Initialize input actions if they haven't been created
        if (MoveAction == null)
        {
            MoveAction = new InputAction("Move", InputActionType.Value);
            MoveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
        }

        if (launchAction == null)
        {
            launchAction = new InputAction("Launch", InputActionType.Button);
            launchAction.AddBinding("<Mouse>/leftButton");
        }

        if (talkAction == null)
        {
            talkAction = new InputAction("Talk", InputActionType.Button);
            talkAction.AddBinding("<Keyboard>/f1");
        }

        // Enable all input actions
        MoveAction.Enable();
        launchAction.Enable();
        talkAction.Enable();

        // Get component references
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;
    }

    void Start()
    {
        try
        {
            footstepTimer = footstepInterval;

            // Initialize XP and stats at zero and update UI
            currentXP = 0;
            level = 1;
            xpToNextLevel = 100;
            statPoints = 0;
            
            // Wait a frame to ensure UI is initialized
            StartCoroutine(InitializeUIValues());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in PlayerController Start: {e.Message}\n{e.StackTrace}");
        }
    }

    void OnEnable()
    {
        // Subscribe to input events
        if (launchAction != null) launchAction.performed += OnLaunchActionPerformed;
        if (talkAction != null) talkAction.performed += OnTalkActionPerformed;

        // Enable all input actions
        MoveAction?.Enable();
        launchAction?.Enable();
        talkAction?.Enable();
    }

    void OnDisable()
    {
        // Unsubscribe from input events
        if (launchAction != null) launchAction.performed -= OnLaunchActionPerformed;
        if (talkAction != null) talkAction.performed -= OnTalkActionPerformed;

        // Disable all input actions
        MoveAction?.Disable();
        launchAction?.Disable();
        talkAction?.Disable();
    }

    private void OnTalkActionPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Talk action performed. Near NPC: " + isNearNPC + ", Current NPC: " + (currentNPC != null ? currentNPC.name : "null"));
        if (isNearNPC && currentNPC != null)
        {
            Debug.Log("Attempting to talk to NPC");
            currentNPC.DisplayDialog();
        }
    }

    private void OnLaunchActionPerformed(InputAction.CallbackContext context)
    {
        Launch();
    }

    // Handles player movement and invincibility cooldown
    void Update()
    {
        if (isInvincible)
        {
            damageCooldown -= Time.deltaTime;
            if (damageCooldown < 0)
                isInvincible = false;
        }

        // Get input and update movement
        if (MoveAction != null)
        {
            move = MoveAction.ReadValue<Vector2>();

            if (move != Vector2.zero)
            {
                // Update movement direction
                moveDirection = move.normalized;
                
                // Update animator parameters
                if (animator != null)
                {
                    animator.SetFloat("Move X", move.x);
                    animator.SetFloat("Move Y", move.y);
                    animator.SetFloat("Speed", move.magnitude);
                }

                // Handle footstep sounds
                footstepTimer -= Time.deltaTime;
                if (footstepTimer < 0)
                {
                    PlayFootstep();
                    footstepTimer = footstepInterval;
                }
            }
            else
            {
                // When not moving, set Speed to 0 but maintain last direction
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0);
                    animator.SetFloat("Move X", moveDirection.x);
                    animator.SetFloat("Move Y", moveDirection.y);
                }
            }
        }
    }

    // Updates physics and movement
    void FixedUpdate()
    {
        if (move != Vector2.zero)
        {
            Vector2 position = rigidbody2d.position;
            position += move * speed * Time.fixedDeltaTime;
            rigidbody2d.MovePosition(position);
        }
    }

    // Change player's health and handle damage
    public void ChangeHealth(int amount)
    {
        // Prevent damage if player is invincible
        if (amount < 0 && isInvincible)
            return;
        if (amount < 0)
        {
            // Enable invincibility and start cooldown
            isInvincible = true;
            damageCooldown = timeInvincible;
            animator.SetTrigger("Hit");

            // Play hurt sound
            if (hurtSound != null)
            {
                PlaySound(hurtSound);
            }

            // Show blood effect
            if (bloodOverlay != null)
            {
                bloodOverlay.ShowBloodEffect();
            }
        }

        // Update player's health and clamp between 0 and max
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // Update UI to reflect current health
        UIHandler.instance.SetHealthValue((float)currentHealth / maxHealth);
    }

    // Launches a projectile
    private void Launch()
    {
        if (projectilePrefab != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePosition.z = 0;
            
            Vector3 direction = (mousePosition - transform.position).normalized;
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
            
            Projectile projectile = projectileObject.GetComponent<Projectile>();
            projectile.Launch(direction, 300);

            animator.SetTrigger("Launch");
        }
    }

    // Play sound effects
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    void PlayFootstep()
    {
        if (footstepClips.Length > 0 && audioSource != null)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    // Handle XP gain and leveling
    public void GainXP(int amount)
    {
        if (amount <= 0) return;  // Ignore invalid XP amounts
        
        currentXP += amount;
        
        // Check for level up
        while (currentXP >= xpToNextLevel)  // Use while loop to handle multiple level ups
        {
            level++;
            currentXP = currentXP - xpToNextLevel;  // Subtract XP needed for this level
            xpToNextLevel = (int)(xpToNextLevel * 1.5f);  // Increase XP needed for next level
            
            // Ensure we don't have negative XP
            if (currentXP < 0) currentXP = 0;
            
            // Add stat point
            statPoints++;
            
            if (onStatPointsChanged != null)
            {
                onStatPointsChanged.Invoke(statPoints);
            }
            
            // Notify level up
            if (onLevelUp != null)
            {
                onLevelUp.Invoke();
            }
            
            // Show level up animation
            if (UIHandler.instance != null)
            {
                UIHandler.instance.ShowLevelUpAnimation();
                UIHandler.instance.UpdateStatPoints(statPoints);
            }
        }

        // Notify UI of XP change
        if (onXPChanged != null)
        {
            onXPChanged.Invoke(currentXP, xpToNextLevel);
        }
        
        // Update UI
        if (UIHandler.instance != null)
        {
            UIHandler.instance.UpdateXPBar(currentXP, xpToNextLevel);
        }
    }

    // Collision detection for NPCs and other objects
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            isNearNPC = true;
            currentNPC = other.GetComponent<NonPlayerCharacter>();
            UIHandler.instance?.ShowInteractionIndicator();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            isNearNPC = false;
            currentNPC = null;
            UIHandler.instance?.HideInteractionIndicator();
            UIHandler.instance?.HideDialogue();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("NPC"))
        {
            isNearNPC = true;
            currentNPC = collision.gameObject.GetComponent<NonPlayerCharacter>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("NPC"))
        {
            isNearNPC = false;
            currentNPC = null;
            UIHandler.instance?.HideInteractionIndicator();
            UIHandler.instance?.HideDialogue();
        }
    }

    private IEnumerator InitializeUIValues()
    {
        yield return null; // Wait one frame

        try
        {
            // Update XP UI
            if (UIHandler.instance != null)
            {
                UIHandler.instance.UpdateXPBar(currentXP, xpToNextLevel);
                UIHandler.instance.UpdateStatPoints(statPoints);
                Debug.Log("Initial UI values set");
            }
            else
            {
                Debug.LogError("UIHandler.instance is null during initialization!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing UI values: {e.Message}\n{e.StackTrace}");
        }
    }
}
