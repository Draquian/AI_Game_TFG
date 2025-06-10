using UnityEngine;
using System.Collections;

public class WaterMagic_Copilot : MagicBase_Copilot
{
    [Header("Wave Attack Settings")]
    public float waveDamage = 15f;          // Damage dealt by the wave.
    public float wavePushForce = 500f;      // Force that pushes enemies away.
    public float waveRadius = 7f;           // Radius of the wave.

    [Header("Charged Attack Settings")]
    public int baseBulletCount = 3;         // Base number of water bullets.
    public float baseSpreadAngle = 15f;     // Base dispersion angle in degrees.
    //public GameObject waterBulletPrefab;    // Prefab for the water bullet.
    public float waterBulletSpeed = 25f;      // Speed of the water bullet.
    public float waterBulletDamage = 10f;     // Damage dealt by each water bullet.
    public float waterSize = 10f;     // Damage dealt by each water bullet.

    [Header("Boost Settings")]
    public float boostMultiplier = 1.5f;    // Multiplier to enhance wave and bullet properties.
    public float boostDuration = 10f;       // Duration of the boost in seconds.

    [Header("Passive Settings")]
    [Range(0f, 1f)]
    public float waterDropChance = 0.3f;      // Chance (30%) to drop water on each attack.
    //public GameObject waterDropPrefab;        // Prefab for the water drop on ground.
    public float waterDropHealAmount = 10f;     // Healing amount when the drop is absorbed.
    public float waterDropManaRestore = 15f;    // Mana restored when the drop is absorbed.
    public float waterDropOffsetDistance = 5f;  // Distance from the player where the drop is generated.

    // The radius in which the WaterMagic script will search for water drops to collect.
    public float waterDropCollectRadius = 1f;

    // (Optional) Reference to the player stats. You can assign this in the Inspector,
    // or retrieve it in Start() (e.g., via GetComponent<PlayerStats>() if WaterMagic is on the player).
    public PlayerStats_Copilot playerStats;

    // (Optional) This value would ideally be set by your charging mechanic.
    // For demonstration, we use a default charge level of 1 (not charged) to 3 (fully charged).
    // In a real scenario, you could update this based on how long the player has been charging.
    public float chargeLevel = 1f;

    // --- Private fields for original values (for boost reversion)
    private float originalWaveRadius;
    private float originalWaveDamage;
    private float originalWavePushForce;
    private float originalBulletDamage;
    private float originalBulletSpeed;
    private float originalwaterSize;

    /// <summary>
    /// MagicAttack creates a water wave that damages and pushes away enemies.
    /// A cooldown is enforced using magicAttackCooldown (and lastMagicAttackTime from MagicBase).
    /// </summary>
    public override void MagicAttack(float magicDamage)
    {
        if (Time.time < lastMagicAttackTime + magicAttackCooldown)
        {
            Debug.Log("WaterMagic: Wave attack is cooling down.");
            return;
        }
        lastMagicAttackTime = Time.time;
        magicAttacCooldownRemaining = lastMagicAttackTime - magicAttackCooldown;


        Debug.Log("WaterMagic: Casting wave attack.");

        // Detect enemies within the wave radius.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, waveRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy") || hit.CompareTag("Boss"))
            {
                // Damage the enemy.
                hit.SendMessage("TakeDamage", magicDamage, SendMessageOptions.DontRequireReceiver);
                // Push enemy away.
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(pushDirection * wavePushForce);
                }
            }
        }

        // Passive: chance to drop water on the ground.
        if (Random.value < waterDropChance)
        {
            Debug.Log("DROP ON NORMAL ATTACK");
            DropWater();
        }
    }

    /// <summary>
    /// MagicChargedAttack creates a charged water mass that, when released, fires water bullets in a spread.
    /// The bullet count and spread dispersion increase with the charge level.
    /// A cooldown is enforced using chargedAttackCooldown (from MagicBase).
    /// </summary>
    public override void MagicChargedAttack(float magicDamage, float timeCharge)
    {
        if (Time.time < lastChargedAttackTime + chargedAttackCooldown)
        {
            Debug.Log("WaterMagic: Charged attack is cooling down.");
            return;
        }
        lastChargedAttackTime = Time.time;
        chargedAttackCooldownRemaining = lastChargedAttackTime + chargedAttackCooldown;

        // Calculate bullet count and dispersion based on charge level.
        int bulletCount = baseBulletCount + Mathf.RoundToInt((timeCharge - 1f) * 2f);
        float spreadAngle = baseSpreadAngle * timeCharge;
        Debug.Log("WaterMagic: Casting charged attack with " + bulletCount + " bullets and a spread of " + spreadAngle + " degrees.");

        GameObject waterBulletPrefab = Resources.Load<GameObject>("Magic/Water/WaterBullet");

        if (waterBulletPrefab != null)
        {
            Vector3 forwardDir = transform.forward;
            // Calculate spread: evenly spread the bullets over the dispersion angle.
            float angleStep = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0;
            float startAngle = -spreadAngle / 2f;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector3 bulletDirection = Quaternion.Euler(0, angle, 0) * forwardDir;

                Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);

                GameObject bulletObj = Instantiate(waterBulletPrefab, newPos, Quaternion.LookRotation(bulletDirection));

                // Set the bullet's velocity.
                Rigidbody bulletRb = bulletObj.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = bulletDirection * waterBulletSpeed;
                }

                // If the bullet has a script to define its damage, set it.
                WaterBullet_Copilot bulletScript = bulletObj.GetComponent<WaterBullet_Copilot>();
                if (bulletScript != null)
                {
                    bulletScript.damage = waterBulletDamage;
                }
            }
        }
        else
        {
            Debug.LogWarning("WaterMagic: WaterBulletPrefab is not assigned.");
        }

        // Passive: chance to drop water on the ground.
        if (Random.value < waterDropChance)
        {
            Debug.Log("DROP ON Charged ATTACK");
            DropWater();
        }
    }

    /// <summary>
    /// MagicBoost temporarily increases the water output.
    /// While boosted, the wave becomes larger and stronger, and the charged attack fires more and bigger water bullets.
    /// A cooldown is enforced using boostCooldown (from MagicBase).
    /// </summary>
    public override void MagicBoost(float magicDamage)
    {
        if (Time.time < lastBoostTime + boostCooldown)
        {
            Debug.Log("WaterMagic: Boost is cooling down.");
            return;
        }
        lastBoostTime = Time.time;
        boostCooldownRemaining = lastBoostTime + boostCooldown;

        Debug.Log("WaterMagic: Activating water boost.");

        // Save original values once before boosting.
        originalWaveRadius = waveRadius;
        originalWaveDamage = waveDamage;
        originalWavePushForce = wavePushForce;
        originalBulletDamage = waterBulletDamage;
        originalBulletSpeed = waterBulletSpeed;
        originalwaterSize = waterSize;

        // Multiply properties by boostMultiplier.
        waveRadius *= boostMultiplier;
        waveDamage *= boostMultiplier;
        wavePushForce *= boostMultiplier;
        waterBulletDamage *= boostMultiplier;
        waterBulletSpeed *= boostMultiplier;
        waterSize *= boostMultiplier;

        // You may also want to adjust visual sizes on the wave and water bullets here.

        // Start coroutine to revert boost after boostDuration seconds.
        StartCoroutine(RevertBoost());

        // Passive: chance to drop water on the ground.
        if (Random.value < waterDropChance)
        {
            DropWater();
        }
    }

    /// <summary>
    /// MagicPassive is a passive effect where attacks have a chance to drop water on the ground.
    /// The water drops can later be absorbed by the player to heal and restore mana.
    /// </summary>
    public override void MagicPassive()
    {
        // Ensure we have a valid playerStats reference.
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats_Copilot>();
        }

        // Check for water drops within collection range.
        Collider[] dropColliders = Physics.OverlapSphere(transform.position, waterDropCollectRadius);
        foreach (Collider col in dropColliders)
        {
            if (col.CompareTag("Water"))
            {
                // Apply healing and mana restoration.
                if (playerStats != null)
                {
                    playerStats.HP = Mathf.Min(playerStats.HP + waterDropHealAmount, playerStats.maxHP);
                    playerStats.Mana = Mathf.Min(playerStats.Mana + waterDropManaRestore, playerStats.maxMana);
                    Debug.Log("WaterMagic: Collected water drop to heal for " + waterDropHealAmount + " and restore " + waterDropManaRestore + " mana.");
                }
                // Destroy the water drop after collecting.
                Destroy(col.gameObject);
            }
        }
    }

    /// <summary>
    /// Reverts the boosted values back to their normal state after boostDuration.
    /// In a full implementation, you should store the original values before boosting.
    /// </summary>
    private IEnumerator RevertBoost()
    {
        yield return new WaitForSeconds(boostDuration);
        // Restore original values.
        waveRadius = originalWaveRadius;
        waveDamage = originalWaveDamage;
        wavePushForce = originalWavePushForce;
        waterBulletDamage = originalBulletDamage;
        waterBulletSpeed = originalBulletSpeed;
        waterSize = originalwaterSize;
        Debug.Log("WaterMagic: Boost ended; stats reverted.");
    }

    /// <summary>
    /// DropWater instantiates a water drop prefab at a specified offset from the player's position.
    /// </summary>
    private void DropWater()
    {
        Debug.Log("WaterMagic: Dropping water on the ground.");
        GameObject waterDropPrefab = Resources.Load<GameObject>("Magic/Water/WaterDrop");

        if (waterDropPrefab != null)
        {
            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 0.01f, transform.position.z);
            // Calculate the drop position offset from the player's current position.
            Vector3 dropPosition = newPos + transform.forward * waterDropOffsetDistance;
            GameObject drop = Instantiate(waterDropPrefab, dropPosition, Quaternion.identity);
            drop.tag = "Water"; // Ensure the water drop carries the "Water" tag.
        }
        else
        {
            Debug.LogWarning("WaterMagic: WaterDrop prefab not assigned.");
        }
    }

    /// <summary>
    /// In Update we look for nearby water drops (by tag "Water").
    /// If found, we assume the player collects them, healing and restoring mana.
    /// </summary>
    void Update()
    {
        MagicPassive();

        if (magicAttacCooldownRemaining > 0)
            magicAttacCooldownRemaining = (lastMagicAttackTime + magicAttackCooldown) - Time.time;

        if (chargedAttackCooldownRemaining > 0)
            chargedAttackCooldownRemaining = (lastChargedAttackTime + chargedAttackCooldown) - Time.time;

        if (boostCooldownRemaining > 0)
            boostCooldownRemaining = (lastBoostTime + boostCooldown) - Time.time;
    }
}
