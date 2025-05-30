using UnityEngine;

public class PlayerStats_Copilot : MonoBehaviour
{
    // Core Stats
    public float physicalDamage = 10f;
    public float physicalDefense = 5f;
    public float magicalDamage = 10f;
    public float magicalDefense = 5f;
    public float criticalDamage = 1.5f;
    public float speed = 5f;
    public float HP = 100f;
    public float Mana = 50f;
    public float strength = 10f;

    // Stamina variables.
    public float stamina = 100f;
    public float maxStamina = 100f;  // Maximum stamina value.

    // Attack radius for detecting enemies.
    public float attackRadius = 1.5f;

    // Class and level info.
    public PlayerClass playerClass = PlayerClass.None;
    public int level = 1;
    public int experience = 0;
    public int experienceThreshold = 100; // XP needed to level up

    // Magic ability (assigned in Awake).
    public MagicBase_Copilot magicAbility;

    public LayerMask enemyLayer;

    void Awake()
    {
        // Randomly assign a magic type for this game session.
        MagicType randomMagicType = (MagicType)Random.Range(0, System.Enum.GetValues(typeof(MagicType)).Length);
        Debug.Log("Assigned Magic Type: " + randomMagicType);

        // Dynamically attach the corresponding magic script to this game object.
        switch (randomMagicType)
        {
            case MagicType.Electricity:
                magicAbility = gameObject.AddComponent<ElectricityMagic_Copilot>();
                break;
            case MagicType.Fire:
                magicAbility = gameObject.AddComponent<FireMagic_Copilot>();
                break;
            case MagicType.Water:
                magicAbility = gameObject.AddComponent<WaterMagic_Copilot>();
                break;
            case MagicType.Shadow:
                magicAbility = gameObject.AddComponent<ShadowMagic_Copilot>();
                break;
            case MagicType.Light:
                magicAbility = gameObject.AddComponent<LightMagic_Copilot>();
                break;
        }
    }

    public void Attack()
    {
        float staminaCost = 10f;

        if (stamina >= staminaCost)
        {
            stamina -= staminaCost;

            // Detect enemies within the attack radius.
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);
            foreach (Collider collider in hitColliders)
            {
                EnemyBase_Copilot enemy = collider.GetComponent<EnemyBase_Copilot>();
                if (enemy != null)
                {
                    // Deal damage using the player's physicalDamage stat.
                    enemy.TakeDamage(physicalDamage);
                    Debug.Log("Hit enemy: " + enemy.enemyName + " for " + physicalDamage + " damage!");
                }
            }

            Debug.Log("Performed physical attack! Stamina left: " + stamina);
        }
        else
        {
            Debug.Log("Not enough stamina to attack.");
        }
    }

    /// <summary>
    /// Recovers a specified amount of stamina up to the maxStamina value.
    /// </summary>
    /// <param name="amount">The amount of stamina to recover.</param>
    public void RecoverStamina(float amount)
    {
        stamina += amount;
        if (stamina > maxStamina)
        {
            stamina = maxStamina;
        }
        Debug.Log("Recovered stamina. Current stamina: " + stamina);
    }

    // Deduct HP, using physical defense
    public void TakeDamage(float amount)
    {
        Debug.Log("DAMAGE TAKEN");
        float effectiveDamage = Mathf.Max(0, amount - physicalDefense);
        HP -= effectiveDamage;
        Debug.Log("Received damage: " + effectiveDamage + ". HP left: " + HP);
        if (HP <= 0)
        {
            Die();
        }
    }

    // Called when player HP drops to zero.
    public void Die()
    {
        Debug.Log("Player died!");
        // Handle death (restart level, display death screen, etc.)
    }

    // Gain experience and level up when threshold is reached.
    public void GainExperience(int xp)
    {
        experience += xp;
        Debug.Log("Gained XP: " + xp + ". Total XP: " + experience);
        if (experience >= experienceThreshold)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        level++;
        experience = 0;
        experienceThreshold = level * 100; // Increase threshold as the level rises.

        // Increase stats arbitrarily—customize as needed.
        physicalDamage += 5;
        physicalDefense += 2;
        magicalDamage += 5;
        magicalDefense += 2;
        HP += 20;
        Mana += 10;
        strength += 2;
        stamina = 100; // restore stamina

        Debug.Log("Leveled Up! New level: " + level);
    }

    /// <summary>
    /// Changes the player class to one of the available classes in the PlayerClass enum.
    /// Optionally, adjust player stats based on the chosen class.
    /// </summary>
    /// <param name="newClass">The new class to assign to the player.</param>
    public void ChangeClass(PlayerClass newClass)
    {
        playerClass = newClass;

        // Optionally, adjust stats based on the new class.
        switch (newClass)
        {
            case PlayerClass.None:
                // Default or balanced stats.
                break;
            case PlayerClass.Archer:
                physicalDamage = 12f;
                speed = 7f;
                physicalDefense = 3f;
                Debug.Log("Archer selected: Increased speed and moderate damage.");
                break;
            case PlayerClass.Knight:
                physicalDamage = 15f;
                physicalDefense = 10f;
                HP = 120f;
                Debug.Log("Knight selected: Increased damage and defense, more HP.");
                break;
            case PlayerClass.Assassin:
                physicalDamage = 18f;
                criticalDamage = 2f;
                speed = 8f;
                Debug.Log("Assassin selected: High damage and critical potential with increased speed.");
                break;
            case PlayerClass.Mage:
                magicalDamage = 18f;
                Mana = 100f;
                Debug.Log("Mage selected: High magical talent, increased mana.");
                break;
            case PlayerClass.Tank:
                HP = 150f;
                physicalDefense = 15f;
                stamina = maxStamina; // Optionally bolster stamina.
                Debug.Log("Tank selected: Significantly increased HP and defense.");
                break;
        }

        Debug.Log("Player class changed to: " + playerClass);
    }
}
