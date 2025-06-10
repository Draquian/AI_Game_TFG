using UnityEngine;

public class FireMagic_Copilot : MagicBase_Copilot
{
    // Parameters for the explosion attack.
    public float explosionRadius = 5f;
    public float explosionForce = 700f;

    // Boost parameters.
    public float boostBurnChance = 0.1f; // 10% chance to apply a burn effect.

    // Passive parameters.
    public float passiveDamageIncrease = 0.1f; // 10% increase in non-magical attacks.
    public float passiveBurnChance = 0.05f;      // 5% chance to burn additionally.

    /// <summary>
    /// MagicAttack: Creates an explosion that deals damage to all entities in the area  
    /// and pushes nearby rigidbodies away.
    /// </summary>
    public override void MagicAttack(float magicDamage)
    {
        if (Time.time < lastMagicAttackTime + magicAttackCooldown)
        {
            Debug.Log("FireMagic: Explosion attack is cooling down.");
            return;
        }
        lastMagicAttackTime = Time.time;
        magicAttacCooldownRemaining = lastMagicAttackTime - magicAttackCooldown;

        Debug.Log("FireMagic: Casting explosion attack!");

        // Find all colliders within the explosion radius centered at this object.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            // Apply damage by using SendMessage so any component with TakeDamage may respond.
            // Your PlayerStats and EnemyBase scripts could be set up with a TakeDamage(float) method.
            hit.SendMessage("TakeDamage", magicDamage * 5, SendMessageOptions.DontRequireReceiver);

            // If the hit object has a Rigidbody, apply an explosion force to push it away.
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
    }

    /// <summary>
    /// MagicChargedAttack: Launches a fireball that increases in size and damage when charged.
    /// </summary>
    public override void MagicChargedAttack(float magicDamage, float timeCharge)
    {
        if (Time.time < lastChargedAttackTime + chargedAttackCooldown)
        {
            Debug.Log("FireMagic: Charged attack is cooling down.");
            return;
        }
        lastChargedAttackTime = Time.time;
        chargedAttackCooldownRemaining = lastChargedAttackTime + chargedAttackCooldown;

        // In a full implementation, instantiate and launch a fireball projectile.
        // For example:

        GameObject fireballPrefab = Resources.Load<GameObject>("Magic/Fire/FireBall");
        // Assume fireballPrefab is a Fireball prefab already assigned in the Inspector.

        Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);

        GameObject fireballInstance = Instantiate(fireballPrefab, newPos, Quaternion.identity);
        Fireball_Copilot fb = fireballInstance.GetComponent<Fireball_Copilot>();
        if (fb != null)
        {
            // Launch the fireball in the forward direction.
            // For a charged attack, perhaps damageMultiplier > 1.
            fb.Launch(transform.forward, magicDamage, timeCharge);
        }
    }

    /// <summary>
    /// MagicBoost: Temporarily doubles the damage of FireMagic and has a chance to burn the enemy.
    /// </summary>
    public override void MagicBoost(float magicDamage)
    {
        if (Time.time < lastBoostTime + boostCooldown)
        {
            Debug.Log("FireMagic: Boost attack is cooling down.");
            return;
        }
        lastBoostTime = Time.time;
        boostCooldownRemaining = lastBoostTime + boostCooldown;

        float boostedDamage = magicDamage * 2;
        Debug.Log("FireMagic: Activating boost! Damage doubled to " + boostedDamage + ".");

        // Here you might temporarily set a modifier on magicDamage or on the player's next attack.
        // Simulate a chance to burn.
        if (Random.value < boostBurnChance)
        {
            Debug.Log("FireMagic: Boost effect burned the enemy! (Applying burn damage over time.)");
            // You could call a method on the enemy to add a burning status.

        }
    }

    /// <summary>
    /// MagicPassive: Increases non-magical (physical) attack damage slightly
    /// and adds a small chance to burn enemies on hit.
    /// </summary>
    public override void MagicPassive()
    {
        Debug.Log("FireMagic: Passive effect activated: Non-magical attacks deal +"
                  + (passiveDamageIncrease * 100) + "% extra damage.");

        // This passive might add a global modifier to the player's physical attacks.
        // Also simulate the possibility to burn on non-magical attacks.
        if (Random.value < passiveBurnChance)
        {
            Debug.Log("FireMagic: Passive effect burned the enemy on a non-magical attack!");
            // Implement the burn effect accordingly.
        }
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
