using System;
using UnityEngine;

[Serializable]
public abstract class BaseItemStats_Copilot
{
    // You can include common properties here if needed.
    public string itemName;

    // Additional properties can be added here, e.g., description, stats, etc.
    [TextArea]
    public string description;
}

[Serializable]
public class EquipmentStats : BaseItemStats_Copilot
{
    public PlayerClass classTpye;

    public int weight;
    public int durability;
    public int physicalArmor;
    public int magicArmor;
    public enum Temperature_Resistance { Normal, Cold, Hot }
    public Temperature_Resistance temp;
}

[Serializable]
public class ConsumableStats : BaseItemStats_Copilot
{
    public int uses;
}

[Serializable]
public class WeaponStats : BaseItemStats_Copilot
{
    public PlayerClass classTpye;
    public MagicType magicType;

    public int weight;
    public int damage;
    public int range;
    public int durability;
    public int attackSpeed;
}

public class OtherStats : BaseItemStats_Copilot
{
    public bool isConsumable;
}
