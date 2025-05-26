using UnityEngine;

public class ArcaneProjectile_Copilot : MonoBehaviour
{
    public float damage = 40f;
    // If true, a 3‑second slowing effect is applied to the target.
    public bool applySlow = false;
    public float slowDuration = 3f;

    void OnTriggerEnter(Collider other)
    {
        // Check if the projectile hit the player.
        PlayerStats_Copilot playerStats = other.GetComponent<PlayerStats_Copilot>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            if (applySlow)
            {
                // Insert your slow–effect logic here.
                // For example, you could call playerStats.ApplySlow(slowDuration);
                Debug.Log("Player hit by arcane projectile! Slowed for " + slowDuration + " seconds.");
            }
            Destroy(gameObject);
        }
    }
}
