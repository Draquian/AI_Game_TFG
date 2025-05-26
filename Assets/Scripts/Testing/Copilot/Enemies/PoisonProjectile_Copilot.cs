using UnityEngine;

public class PoisonProjectile_Copilot : MonoBehaviour
{
    // Set the damage that the projectile deals when it hits the player.
    public float damage = 10f;

    void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has a PlayerStats component.
        PlayerStats_Copilot playerStats = other.GetComponent<PlayerStats_Copilot>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            Debug.Log("Poison projectile hit the player, dealing " + damage + " damage.");
            Destroy(gameObject);
        }
        else
        {
            // Optionally, destroy the projectile upon hitting anything else.
            //Destroy(gameObject);
        }
    }
}
