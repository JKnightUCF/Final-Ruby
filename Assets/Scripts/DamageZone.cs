using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    // Apply damage to the player while they remain in the damage zone
    void OnTriggerStay2D(Collider2D other)
    {
        // Check if the object in the zone is the player
        PlayerController controller = other.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.ChangeHealth(-1); // Reduce player's health
        }
    }
}
