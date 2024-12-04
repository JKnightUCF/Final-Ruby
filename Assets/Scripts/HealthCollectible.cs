using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles collectible health pickups
public class HealthCollectible : MonoBehaviour
{
    public AudioClip collectedClip; // Sound effect for collection
    public GameObject eatParticlePrefab; // Particle effect prefab

    // Trigger when an object enters the collectible zone
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        PlayerController controller = other.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.ChangeHealth(1); // Increase player's health

            // Play sound effect
            if (collectedClip != null)
            {
                AudioSource.PlayClipAtPoint(collectedClip, transform.position);
            }

            // Create and destroy particle effect
            if (eatParticlePrefab != null)
            {
                GameObject eatEffect = Instantiate(eatParticlePrefab, controller.transform.position, Quaternion.identity);
                eatEffect.transform.parent = controller.transform; // Attach to player
                Destroy(eatEffect, 2f); // Remove after 2 seconds
            }

            Destroy(gameObject); // Remove the collectible
        }
 
    }
}
