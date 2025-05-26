using UnityEngine;

public class RottingScavenger_Copilot : EnemyBase_Copilot
{
    private Transform target;
    private float attackCooldown = 1f;
    private float attackTimer = 0f;

    void Start()
    {
        // Set specific enemy stats.
        enemyName = "Rotting Scavenger";
        description = "A diseased humanoid figure with limp, staggered movements, tattered clothing, and rotting flesh. Often seen wandering the desolate corridors with a hunched posture, evoking a sense of decay and impending danger.";

        maxHP = 80f;
        currentHP = maxHP; // Assign initial HP.

        physicalAttackDamage = 12f;
        physicalDefense = 10f;
        magicalDamage = 0f;
        magicalDefense = 5f;

        attackRange = 2.5f;        // Melee range: 1 unit.
        movementSpeed = 2.5f;      // Moves slowly.

        lootDropChance = 0.02f;    // 2% loot probability.

        // Find the player target by tag (ensure the player GameObject is tagged "Player").
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Determine distance to the player.
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > attackRange)
            {
                // Movement pattern: slowly shamble toward the target.
                Vector3 direction = (target.position - transform.position).normalized;
                transform.position += direction * movementSpeed * Time.deltaTime;

                // Optionally rotate to face the target.
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
            }
            else
            {
                // If within melee range, perform heavy swipe attack every 3 seconds.
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    HeavySwipeAttack();
                    attackTimer = 0f;
                }
            }
        }
    }

    /// <summary>
    /// Performs a heavy swipe attack on the player.
    /// </summary>
    void HeavySwipeAttack()
    {
        PlayerStats_Copilot playerStats = target.GetComponent<PlayerStats_Copilot>();
        if (playerStats != null)
        {
            Debug.Log(enemyName + " performs a heavy swipe attack on the player!");
            playerStats.TakeDamage(physicalAttackDamage);
        }
    }
}
