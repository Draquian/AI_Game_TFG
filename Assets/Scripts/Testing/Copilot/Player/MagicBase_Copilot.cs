using UnityEngine;

public abstract class MagicBase_Copilot : MonoBehaviour
{
    public float baseDamage = 20f;
    public float manaCost = 10f;
    public float chargeMultiplier = 2f; // Multiplies damage on a charged attack

    public float magicAttackCooldown = 5f;
    public float chargedAttackCooldown = 8f;
    public float boostCooldown = 20f;
    public float passiveCooldown = 3f;

    public float magicAttacCooldownRemaining = 0f; 
    public float chargedAttackCooldownRemaining = 0f; 
    public float boostCooldownRemaining = 0f; 
    public float passiveCooldownRemaining = 0f;

    // Last usage timestamps.
    protected float lastMagicAttackTime = -100f;
    protected float lastChargedAttackTime = -100f;
    protected float lastBoostTime = -100f;
    protected float lastPassiveTime = -100f;

    // Normal magic attack.
    public abstract void MagicAttack(float magicDamage);

    // Charged magic attack.
    public abstract void MagicChargedAttack(float magicDamage, float timeCharge);

    public abstract void MagicPassive();

    public abstract void MagicBoost(float magicDamage);

    private void Update()
    {
        if (magicAttacCooldownRemaining > 0)
            magicAttacCooldownRemaining = (lastMagicAttackTime + magicAttackCooldown) - Time.time;

        if (chargedAttackCooldownRemaining > 0)
            chargedAttackCooldownRemaining = (lastChargedAttackTime + chargedAttackCooldown) - Time.time;

        if (boostCooldownRemaining > 0)
            boostCooldownRemaining = (lastBoostTime + boostCooldown) - Time.time;
    }
}
