using UnityEngine;
using System.Collections;

public class MutatedProwler_Copilot : EnemyBase_Copilot
{
    private Transform target;

    // Attack cycle variables.
    private bool isAttacking = false;
    private float attackCycle = 4f; // Total cycle length (s)

    // Movement dash variables.
    private float dashCycle = 1f;      // Total dash cycle (s)
    private float dashDuration = 0.3f;   // Duration of an active dash burst (s)
    private float dashTimer = 0f;
    private float dashSpeedMultiplier = 2.0f; // Multiplier for dash bursts

    void Start()
    {
        // Set specific enemy stats.
        enemyName = "Mutated Prowler";
        description = "A diseased humanoid figure with limp, staggered movements, tattered clothing, and rotting flesh. Often seen wandering the desolate corridors with a hunched posture, evoking a sense of decay and impending danger.";

        maxHP = 70f;
        currentHP = maxHP;

        physicalAttackDamage = 15f;
        physicalDefense = 8f;
        magicalDamage = 0f;
        magicalDefense = 8f;

        attackRange = 1f;              // Melee range
        movementSpeed = 3.5f;          // Base speed

        lootDropChance = 0.25f;        // 25% loot probability

        // Locate the player by tag ("Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange)
        {
            // --- Movement Phase: Dash in bursts ---
            dashTimer += Time.deltaTime;
            if (dashTimer < dashDuration)
            {
                // During active dash burst.
                Vector3 direction = (target.position - transform.position).normalized;
                transform.position += direction * movementSpeed * dashSpeedMultiplier * Time.deltaTime;
                // Optionally, rotate smoothly to face the target.
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.2f);
            }
            else if (dashTimer >= dashCycle)
            {
                // Reset dash cycle.
                dashTimer = 0f;
            }
            // (During the remainder of the dash cycle, the enemy pauses.)
        }
        else
        {
            // --- Attack Phase: If in melee range, execute attack cycle ---
            if (!isAttacking)
            {
                StartCoroutine(AttackSequence());
            }
        }
    }

    /// <summary>
    /// Executes the attack sequence:
    ///  - Performs a claw slash,
    ///  - Waits 0.5 seconds,
    ///  - Performs a second claw slash,
    ///  - Then pauses until the 4-second cycle completes.
    /// </summary>
    private IEnumerator AttackSequence()
    {
        isAttacking = true;

        // First claw slash.
        ClawSlash();

        // Wait 0.5 seconds for the second slash.
        yield return new WaitForSeconds(0.5f);

        // Second claw slash.
        ClawSlash();

        // Log the completed attack cycle.
        Debug.Log(enemyName + " completed its rapid consecutive claw slashes.");

        // Wait for the remainder of the 4-second cycle.
        yield return new WaitForSeconds(attackCycle - 0.5f);

        isAttacking = false;
    }

    /// <summary>
    /// Performs a claw slash attack on the player by applying damage through the PlayerStats component.
    /// </summary>
    private void ClawSlash()
    {
        if (target != null)
        {
            PlayerStats_Copilot playerStats = target.GetComponent<PlayerStats_Copilot>();
            if (playerStats != null)
            {
                Debug.Log(enemyName + " delivers a claw slash, inflicting " + physicalAttackDamage + " damage.");
                playerStats.TakeDamage(physicalAttackDamage);
            }
        }
    }
}
