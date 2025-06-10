using UnityEngine;
using System.Collections;

public class ElectricityMagic_Copilot : MagicBase_Copilot
{
    // ---------------------------------------
    // Parameters for the Lightning Beam attack.
    // ---------------------------------------
    public float beamDamage = 20f;
    public float beamRange = 50f;

    // ---------------------------------------
    // Parameters for the Bouncing Lightning attack.
    // ---------------------------------------
    public int minBounceCount = 1;
    public int maxBounceCount = 2;
    public float lightningSpeed = 4;
    public int chargedBounceBonus = 2;
    public float chargedSpeedMultiplier = 1.5f;

    // ---------------------------------------
    // Boost Effect Parameters.
    // ---------------------------------------
    public float movementSpeedBoost = 1.2f;    // Multiplier for player's speed.
    // (Assuming you have similar fields for charging time, attack speed, and spell cooldowns in PlayerStats;
    // for demonstration, we only modify speed.)
    public float boostDuration = 10f;

    // ---------------------------------------
    // Passive Healing Parameters.
    // ---------------------------------------
    public float healAmount = 5f;

    // ---------------------------------------
    // Cooldown parameters (in seconds) for each ability:
    // ---------------------------------------
    /*public float magicAttackCooldown = 5f;
    public float chargedAttackCooldown = 8f;
    public float boostCooldown = 20f;
    public float passiveCooldown = 3f;*/



    /// <summary>
    /// MagicAttack(): Fires a lightning beam that auto-targets the closest enemy.
    /// </summary>
    public override void MagicAttack(float magicDamage)
    {

        if (Time.time < lastMagicAttackTime + magicAttackCooldown)
        {
            Debug.Log("MagicAttack still cooling down...");
            return;
        }
        lastMagicAttackTime = Time.time;
        magicAttacCooldownRemaining = lastMagicAttackTime + magicAttackCooldown;

        GameObject target = FindClosestEnemy();
        Vector3 direction = transform.forward;
        if (target != null)
        {
            direction = (target.transform.position - transform.position).normalized;
            Debug.Log("ElectricityMagic: Target acquired (" + target.name + "). Firing lightning beam for " + beamDamage + " damage.");
        }
        else
        {
            Debug.Log("ElectricityMagic: No enemy found. Firing lightning beam straight ahead.");
        }

        GameObject lightningBeamPrefab = Resources.Load<GameObject>("Magic/Electricity/beam");

        // Instantiate the lightning beam prefab.
        if (lightningBeamPrefab != null)
        {
            // Instantiate at a suitable position (for example, at the magic source position).
            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            GameObject beamObject = Instantiate(lightningBeamPrefab, newPos, Quaternion.LookRotation(direction));

            // Retrieve the LightningBeam component from the instantiated object.
            LightningBeam_Copilot beam = beamObject.GetComponent<LightningBeam_Copilot>();

            // Check if the component exists, then call Initialize.
            if (beam != null)
            {
                beam.Initialize(beamDamage, 2);
            }
            else
            {
                Debug.LogWarning("LightningBeam component not found on the instantiated prefab!");
            }

            // (Optional: Set beam parameters, such as damage or lifetime, on the beam’s script.)
            // e.g., beam.GetComponent<LightningBeam>().Initialize(beamDamage);
        }
        else
        {
            Debug.LogWarning("LightningBeamPrefab not assigned!");
        }
    }

    /// <summary>
    /// MagicChargedAttack(): Launches a bouncing lightning bolt.
    /// Normal attack bounces 1–2 times; if charged, increases speed and additional bounces.
    /// </summary>
    public override void MagicChargedAttack(float magicDamage, float timeCharge)
    {
        if (Time.time < lastChargedAttackTime + chargedAttackCooldown)
        {
            Debug.Log("MagicChargedAttack still cooling down...");
            return;
        }
        lastChargedAttackTime = Time.time;
        chargedAttackCooldownRemaining = lastChargedAttackTime + chargedAttackCooldown;

        // Calculate bounce count.
        int bounceCount = Random.Range(minBounceCount, maxBounceCount + 1) + (int)timeCharge;
        float finalSpeed = lightningSpeed * (int)timeCharge;
        Debug.Log("ElectricityMagic: Launching charged bouncing lightning bolt with speed "
            + finalSpeed + " and " + bounceCount + " bounces.");

        GameObject bouncingLightningPrefab = Resources.Load<GameObject>("Magic/Electricity/bouncingLight");

        if (bouncingLightningPrefab != null)
        {
            Vector3 newPos = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
            GameObject bolt = Instantiate(bouncingLightningPrefab, newPos, Quaternion.identity);
            // Assuming the bouncing projectile has a script that accepts bounceCount & speed.
            BouncingLightning_Copilot bl = bolt.GetComponent<BouncingLightning_Copilot>();
            if (bl != null)
            {
                bl.bounceCount = bounceCount;
                bl.speed = finalSpeed;
                bl.damage = magicDamage;
                // You might also pass damage information if needed.

                // Get the camera from the children of this GameObject.
                Camera playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    Debug.LogError("PlayerShooting: No child Camera found!");
                    return;
                }

                // Get the direction in which the camera is looking.
                Vector3 direction = playerCamera.transform.forward;

                bl.Launch(direction);
            }
        }
        else
        {
            Debug.LogWarning("BouncingLightningPrefab is not assigned!");
        }
    }


    /// <summary>
    /// MagicBoost(): Increases movement speed, reduces charging time, and speeds up attacks.
    /// </summary>
    public override void MagicBoost(float magicDamage)
    {
        if (Time.time < lastBoostTime + boostCooldown)
        {
            Debug.Log("MagicBoost still cooling down...");
            return;
        }
        lastBoostTime = Time.time;
        boostCooldownRemaining = lastBoostTime + boostCooldown;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("MagicBoost: Player not found!");
            return;
        }
        PlayerStats_Copilot ps = player.GetComponent<PlayerStats_Copilot>();
        if (ps == null)
        {
            Debug.LogWarning("MagicBoost: PlayerStats component not found on player!");
            return;
        }

        // Store original stats.
        float originalSpeed = ps.speed;
        // (If you have fields for chargingTime, attackSpeed, and spell cooldown modifiers, store them similarly.)
        Debug.Log("ElectricityMagic: Activating boost! Player speed increased, charging time reduced, and attack speed boosted, with spell cooldowns reduced.");

        // Apply subtle boost modifications.
        ps.speed *= movementSpeedBoost;
        // For demonstration, we only modify speed.
        // You could imagine: ps.chargingTime *= someReduction, ps.attackSpeed *= someIncrease, ps.spellCooldownModifier *= someFactor, etc.

        // Restore stats after boostDuration seconds.
        StartCoroutine(RevertBoost(player, originalSpeed));

    }

    private IEnumerator RevertBoost(GameObject player, float originalSpeed)
    {
        yield return new WaitForSeconds(boostDuration);
        PlayerStats_Copilot ps = player.GetComponent<PlayerStats_Copilot>();
        if (ps != null)
        {
            ps.speed = originalSpeed;
            Debug.Log("ElectricityMagic: Boost ended. Player stats reverted to normal.");
        }
    }

    /// <summary>
    /// MagicPassive(): Every lightning attack heals the player.
    /// </summary>
    public override void MagicPassive()
    {
        if (Time.time < lastPassiveTime + passiveCooldown)
            return;

        lastPassiveTime = Time.time;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats_Copilot ps = player.GetComponent<PlayerStats_Copilot>();
            if (ps != null)
            {
                ps.HP = Mathf.Min(ps.HP + healAmount, ps.maxHP);
                Debug.Log("ElectricityMagic: Passive effect heals player for " + healAmount + " HP (current HP: " + ps.HP + ").");
            }
        }
    }

    /// <summary>
    /// Modified FindClosestEnemy() searches for objects tagged as "Enemy" or "Boss" within beamRange.
    /// </summary>
    /// <returns>The closest enemy GameObject, or null if none is found.</returns>
    GameObject FindClosestEnemy()
    {
        GameObject closest = null;
        float minDistance = beamRange;
        Vector3 currentPosition = transform.position;

        // Check both "Enemy" and "Boss" tagged objects.
        string[] tagsToSearch = { "Enemy", "Boss" };
        foreach (string tag in tagsToSearch)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(enemy.transform.position, currentPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = enemy;
                }
            }
        }
        return closest;
    }
}
