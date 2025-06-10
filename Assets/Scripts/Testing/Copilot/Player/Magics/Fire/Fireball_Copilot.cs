using UnityEngine;

public class Fireball_Copilot : MonoBehaviour
{
    [Header("Fireball Settings")]
    public float damage = 100f;               // Base damage dealt upon impact.
    public float burnMultiplier = 0.5f;       // Burn effect deals half of the initial damage.
    public float burnDuration = 5f;           // Burn effect last duration.
    public float speed = 20f;                 // Speed of the fireball projectile.

    private Rigidbody rb;

    void Awake()
    {
        // Get or add the Rigidbody.
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;

        // Ensure the collider is set as a trigger.
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// Launch the fireball with a given direction and modified damage.
    /// </summary>
    /// <param name="direction">Normalized direction vector to launch the fireball.</param>
    /// <param name="baseDamage">The base damage for the fireball.</param>
    /// <param name="damageMultiplier">Multiplier to be applied to the base damage (e.g. for charged attacks).</param>
    public void Launch(Vector3 direction, float baseDamage, float damageMultiplier)
    {
        // Adjust the damage based on parameters.
        damage = baseDamage * damageMultiplier;

        // Normalize the direction (in case it isn't) and assign velocity.
        rb.velocity = direction.normalized * speed;

        // Optionally, adjust the scale of the fireball to reflect a charged attack.
        // For example, if damageMultiplier > 1, scale the fireball accordingly:
        transform.localScale *= damageMultiplier;

        Debug.Log("Fireball launched! Damage: " + damage + ", Direction: " + direction.normalized);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we've hit an enemy (assuming they use EnemyBase).
        EnemyBase_Copilot enemy = other.GetComponent<EnemyBase_Copilot>();
        if (enemy != null)
        {
            // Deal initial damage.
            enemy.TakeDamage(damage);

            // Apply burn damage (half of initial damage over burnDuration seconds).
            ApplyBurn(other.gameObject, damage * burnMultiplier, burnDuration);
        }
        // Optionally check for the player if friendly fire is enabled, etc.

        // Destroy the fireball on impact.
        //Destroy(gameObject);
    }

    /// <summary>
    /// Applies a burn effect to the target. If a burn effect already exists, it resets it.
    /// </summary>
    /// <param name="target">Object to apply the burn effect to.</param>
    /// <param name="totalBurnDamage">Total burn damage to deal over time.</param>
    /// <param name="duration">Time in seconds over which burn damage is applied.</param>
    void ApplyBurn(GameObject target, float totalBurnDamage, float duration)
    {
        BurnEffect_Copilot burn = target.GetComponent<BurnEffect_Copilot>();
        if (burn != null)
        {
            burn.ResetBurn(totalBurnDamage, duration);
        }
        else
        {
            burn = target.AddComponent<BurnEffect_Copilot>();
            burn.StartBurn(totalBurnDamage, duration);
        }
    }
}
