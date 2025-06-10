using UnityEngine;

public class ShadowMagic_Copilot : MagicBase_Copilot
{
    public override void MagicAttack(float magicDamage)
    {
        Debug.Log("Casting a shadow bolt with Shadow Magic!");
        // Implement your shadow bolt effect.
    }

    public override void MagicChargedAttack(float magicDamage, float timeCharge)
    {
        Debug.Log("Engulfing the area in darkness with a charged Shadow Magic attack!");
        // Implement a charged shadow effect.
    }

    public override void MagicPassive()
    {
        Debug.Log("Casting a lightning storm with a charged Electricity Magic attack!");
        // Implement a more powerful charged effect.
    }

    public override void MagicBoost(float magicDamage)
    {
        Debug.Log("Casting a lightning storm with a charged Electricity Magic attack!");
        // Implement a more powerful charged effect.
    }
}
