using UnityEngine;

public class LightMagic_Copilot : MagicBase_Copilot
{
    public override void MagicAttack(float magicDamage)
    {
        Debug.Log("Casting a beam of light with Light Magic!");
        // Implement your light beam effect here.
    }

    public override void MagicChargedAttack(float magicDamage, float timeCharge)
    {
        Debug.Log("Unleashing a radiant burst with a charged Light Magic attack!");
        // Implement a charged light effect.
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