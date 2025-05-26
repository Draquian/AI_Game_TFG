using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VenomousSpitter_Copilot : EnemyBase_Copilot
{
    private Transform target;

    // Attack interval in seconds.
    private float attackInterval = 4f;

    // For repositioning. The enemy will try to maintain its distance between these values (in units).
    private float desiredMinDistance = 4f;
    private float desiredMaxDistance = 6f;

    // Add these fields at the top of your VenomousSpitter class.
    public GameObject poisonProjectilePrefab;  // Optionally assign a custom prefab.
    public float projectileSpeed = 10f;
    public float projectileSpawnOffsetY = 1.0f;  // Offset to spawn the projectile higher (in world units).
    public float projectileDamage = 10f;         // Damage that each projectile will cause.

    void Start()
    {
        // Set enemy-specific stats.
        enemyName = "Venomous Spitter";
        description = "A squat, amphibian-like monster with bulging, asymmetrical eyes and a gaping, drooling maw. Its skin is slick and mottled, and it exudes an aura of toxic decay.";

        maxHP = 60f;
        currentHP = maxHP;

        physicalAttackDamage = 5f;
        physicalDefense = 5f;
        magicalDamage = 10f;
        magicalDefense = 10f;

        attackRange = 5f;      // Ranged attack effective at 5 units.
        movementSpeed = 2f;    // Slow shuffling speed.

        lootDropChance = 0.30f; // 30% loot probability.

        // Find the player (make sure the player GameObject is tagged "Player").
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }

        // Start the attack cycle coroutine.
        StartCoroutine(AttackCycle());
    }

    void Update()
    {
        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        // Reposition to maintain an optimal distance (roughly 5 units).
        if (distance > desiredMaxDistance)
        {
            // Too far: move slowly toward the target.
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * movementSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
        }
        else if (distance < desiredMinDistance)
        {
            // Too close: move slowly away from the target.
            Vector3 direction = (transform.position - target.position).normalized;
            transform.position += direction * movementSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
        }
        // Otherwise, remain mostly stationary.
    }

    /// <summary>
    /// Every 4 seconds, perform an attack: either a single projectile or a spread of 3.
    /// </summary>
    private IEnumerator AttackCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            AttackPoisonSpit();
        }
    }

    /// <summary>
    /// Chooses the attack pattern for the poison spit.
    /// On some cycles, the spitter fires a spread of three projectiles.
    /// Otherwise, it fires a single projectile toward the target.
    /// </summary>
    private void AttackPoisonSpit()
    {
        if (target == null)
            return;

        // Calculate the direction vector from this enemy to the target.
        Vector3 direction = (target.position - transform.position).normalized;

        // Randomly decide between a single shot and a spread attack.
        if (Random.value < 0.5f)
        {
            Debug.Log(enemyName + " launches a single poison spit projectile.");
            LaunchPoisonProjectile(direction);
        }
        else
        {
            Debug.Log(enemyName + " launches a spread of 3 poison spit projectiles.");
            // Fire center.
            LaunchPoisonProjectile(direction);
            // Fire left: offset by -15 degrees.
            Vector3 leftDir = Quaternion.AngleAxis(-15f, Vector3.up) * direction;
            LaunchPoisonProjectile(leftDir);
            // Fire right: offset by +15 degrees.
            Vector3 rightDir = Quaternion.AngleAxis(15f, Vector3.up) * direction;
            LaunchPoisonProjectile(rightDir);
        }
    }
    /// <summary>
    /// Launches a poison projectile in the given direction.
    /// This function now spawns the projectile from a higher position and ensures that it carries a damage script.
    /// </summary>
    /// <param name="direction">Normalized direction vector for the projectile.</param>
    private void LaunchPoisonProjectile(Vector3 direction)
    {
        // Calculate a spawn position offset upward.
        Vector3 spawnPos = transform.position + Vector3.up * projectileSpawnOffsetY;

        // If no prefab is assigned, create one dynamically.
        if (poisonProjectilePrefab == null)
        {
            // Create a simple sphere as our projectile.
            poisonProjectilePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            poisonProjectilePrefab.transform.localScale = Vector3.one * 0.3f;

            // Add a Rigidbody to enable movement.
            Rigidbody rb = poisonProjectilePrefab.AddComponent<Rigidbody>();
            rb.useGravity = false;

            // Set the collider to trigger.
            Collider col = poisonProjectilePrefab.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // Add the PoisonProjectile script.
            if (poisonProjectilePrefab.GetComponent<PoisonProjectile_Copilot>() == null)
            {
                PoisonProjectile_Copilot pp = poisonProjectilePrefab.AddComponent<PoisonProjectile_Copilot>();
                pp.damage = projectileDamage;
            }

            // Deactivate the prefab so it can be used as a template.
            poisonProjectilePrefab.SetActive(false);
        }

        // Instantiate a new projectile from the prefab.
        GameObject projectile = Instantiate(poisonProjectilePrefab, spawnPos, Quaternion.LookRotation(direction));

        // Activate the projectile (since our prefab is inactive).
        projectile.SetActive(true);

        // Set the projectile's velocity.
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }

        // Destroy the projectile after 5 seconds to clean up.
        Destroy(projectile, 5f);

        Debug.Log(enemyName + " launched a poison projectile from " + spawnPos + " in direction: " + direction);
    }
}
