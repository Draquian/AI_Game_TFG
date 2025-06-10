using UnityEngine;
using System.Collections;

public class EnemyBase_Copilot : MonoBehaviour
{
    // Basic enemy identification.
    public string enemyName = "Enemy";
    public string description = "Default enemy description";

    // Health.
    public float maxHP = 100f;
    [SerializeField]protected float currentHP;

    // Offensive stats.
    public float physicalAttackDamage = 10f;
    public float physicalDefense = 5f;
    public float magicalDamage = 0f;
    public float magicalDefense = 0f;

    // Additional enemy stats.
    public float attackRange = 1f;        // Melee range.
    public float movementSpeed = 2.5f;      // Movement speed.

    // Loot drop configuration.
    public GameObject[] lootPrefabs;
    public float lootDropChance = 0.5f;     // Default 50% chance.

    void Start()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// Performs an attack on the target. Assumes a reference to a PlayerStats component.
    /// </summary>
    /// <param name="target">The player to attack.</param>
    public virtual void Attack(PlayerStats_Copilot target)
    {
        if (target != null)
        {
            Debug.Log(enemyName + " attacks the player for " + physicalAttackDamage + " damage!");
            target.TakeDamage(physicalAttackDamage);
        }
    }

    /// <summary>
    /// Handles taking damage. Damage is reduced by enemy defense.
    /// </summary>
    /// <param name="amount">The raw damage received.</param>
    public virtual void TakeDamage(float amount)
    {
        float effectiveDamage = Mathf.Max(0, amount - physicalDefense);
        currentHP -= effectiveDamage;
        Debug.Log(enemyName + " takes " + effectiveDamage + " damage. Remaining HP: " + currentHP);

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Called when the enemy's HP reaches zero or below.
    /// </summary>
    public virtual void Die()
    {
        Debug.Log(enemyName + " has died!");
        GiveLoot();
        // Optionally, add death animations or effects here.

        PlayerStats_Copilot playerObj = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats_Copilot>(); ;
        playerObj.GainExperience(Random.Range(15, 75));

        Destroy(gameObject);
    }

    /// <summary>
    /// Handles dropping loot on death.
    /// </summary>
    public virtual void GiveLoot()
    {
        if (lootPrefabs != null && lootPrefabs.Length > 0)
        {
            foreach (GameObject loot in lootPrefabs)
            {
                // Each loot item is dropped based on the determined chance.
                if (Random.value <= lootDropChance)
                {
                    Instantiate(loot, transform.position, Quaternion.identity);
                    Debug.Log(enemyName + " dropped loot: " + loot.name);
                }
            }
        }
    }

    /// <summary>
    /// Returns a formatted string of enemy stats.
    /// </summary>
    public virtual string GetEnemyStats()
    {
        return string.Format("{0}: HP {1}/{2}, Physical Damage {3}, Magical Damage {4}, Physical Defense {5}, Magical Defense {6}",
            enemyName, currentHP, maxHP, physicalAttackDamage, magicalDamage, physicalDefense, magicalDefense);
    }

    // Additional functionality—such as movement, AI decision making, etc.—can be added here and/or in derived classes.

    public virtual void ApplySlow(float slowDuration)
    {
        float OGmovementSpeed = movementSpeed;
        movementSpeed = movementSpeed / 2;

        StartCoroutine(RevertSlow(OGmovementSpeed, slowDuration));
    }

    IEnumerator RevertSlow(float og, float slowDuration)
    {
        yield return new WaitForSeconds(slowDuration);
        movementSpeed = og;
    }
}
