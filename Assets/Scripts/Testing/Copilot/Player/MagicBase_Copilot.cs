using UnityEngine;

public abstract class MagicBase_Copilot : MonoBehaviour
{
    public float baseDamage = 20f;
    public float manaCost = 10f;
    public float chargeMultiplier = 2f; // Multiplies damage on a charged attack

    // Normal magic attack.
    public abstract void MagicAttack();

    // Charged magic attack.
    public abstract void MagicChargedAttack();
}
