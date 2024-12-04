using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls enemy movement, interactions, and behavior
public class EnemyController : MonoBehaviour
{
    // Enemy properties
    public float speed;               // Movement speed
    public bool vertical;             // True for vertical movement, false for horizontal
    public float changeTime;          // Time interval to switch direction
    public ParticleSystem smokeEffect; // Effect when enemy is "fixed"
    public AudioClip fixedSound;
    public GameObject enemyEyePrefab;

    // Private variables
    private Rigidbody2D rigidbody2d;
    private Animator animator;
    private AudioSource footstepAudioSource;
    private float timer;
    private int direction = 1;        // Current direction (1 or -1)
    private bool broken = true;       // Active state
    private bool isFixed = false;     // Fixed state
    public GameObject stunParticlePrefab; // Stun effect prefab

    // Initialization
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        footstepAudioSource = GetComponent<AudioSource>();
        timer = changeTime;
    }

    // Handle updates (skip if inactive or fixed)
    void Update()
    {
        if (!broken || isFixed) return;
    }

    // Handle physics and movement
    void FixedUpdate()
    {
        if (!broken || isFixed) return;

        // Timer for direction change
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            direction = -direction;
            timer = changeTime;
        }

        // Update position and animation based on movement type
        Vector2 position = rigidbody2d.position;

        if (vertical)
        {
            position.y += speed * direction * Time.deltaTime;
            animator.SetFloat("Move X", 0);
            animator.SetFloat("Move Y", direction);
        }
        else
        {
            position.x += speed * direction * Time.deltaTime;
            animator.SetFloat("Move X", direction);
            animator.SetFloat("Move Y", 0);
        }

        rigidbody2d.MovePosition(position);
    }

    // Handle collisions
    void OnCollisionEnter2D(Collision2D other)
    {
        // Player interaction
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ChangeHealth(-1);
            return;
        }

        // Projectile interaction
        Projectile projectile = other.gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            TakeHit(); // Trigger hit logic
        }
    }

    // Manage enemy hit behavior
    public void TakeHit()
    {
        if (isFixed) return;

        EnterFixState(); // Fix enemy immediately
        StartCoroutine(FlashRed()); // Flash red for visual feedback
    }

    // Temporarily flash enemy red
    private IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(1.0f);
            spriteRenderer.color = Color.white;
        }
    }

    // Transition to fixed state
    private void EnterFixState()
    {
        isFixed = true;
        broken = false;
        animator.SetTrigger("Fixed");
        smokeEffect.Stop();
        GetComponent<Rigidbody2D>().simulated = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyFixed();
        }
        else
        {
            Debug.LogError("GameManager instance is null!");
        }

        if (footstepAudioSource != null && footstepAudioSource.isPlaying)
        {
            footstepAudioSource.Stop();
        }
        if (footstepAudioSource != null && fixedSound != null)
        {
            footstepAudioSource.PlayOneShot(fixedSound);
        }

        // Spawn the enemy eye
        if (enemyEyePrefab != null)
        {
            Instantiate(enemyEyePrefab, transform.position, Quaternion.identity);
        }
    }

    // Permanently fix the enemy
    public void Fix()
    {
        isFixed = true;
        broken = false;
        animator.SetTrigger("Fixed");
        smokeEffect.Stop();
        GetComponent<Rigidbody2D>().simulated = false;
        Debug.Log("Enemy has been permanently fixed!");
    }

    private void Death()
    {
        // Add death logic here
    }
}
