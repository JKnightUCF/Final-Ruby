using UnityEngine;

public class EnemyEyeItem : MonoBehaviour
{
    public float speed = 5f;        // Speed at which the eye moves toward the player
    public float pullRadius = 5f;   // Distance at which the eye starts moving toward the player
    public int xpValue = 40;        // Amount of XP given when collected

    private Transform target;
    private Rigidbody2D rb;
    private bool isBeingPulled = false;
    private bool isCollected = false;  // New flag to prevent multiple collections

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Add initial force to make it "pop out"
        if (rb != null)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 randomDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
            rb.AddForce(randomDirection * 5f, ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        if (isCollected) return;  // Skip update if already collected

        if (!isBeingPulled)
        {
            // Find the player if we don't have a target
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            // Check if player is within pull radius
            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                if (distance < pullRadius)
                {
                    isBeingPulled = true;
                    if (rb != null)
                    {
                        rb.velocity = Vector2.zero; // Stop any existing movement
                    }
                }
            }
        }
        else
        {
            // Move toward the player
            if (target != null)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;  // Skip if already collected

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            isCollected = true;  // Mark as collected immediately
            Debug.Log($"EnemyEye collected by player");
            player.GainXP(xpValue);
            // Use Destroy at end of frame to ensure we don't destroy while still processing collisions
            Destroy(gameObject, 0.01f);
        }
    }
}
