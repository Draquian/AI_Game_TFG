#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemSO_Copilot))]
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default GUI first.
        base.OnInspectorGUI();

        // Get a reference to the target ItemSO.
        ItemSO_Copilot item = (ItemSO_Copilot)target;

        // If the stats field is null or of an unexpected type, re-initialize it.
        if (item.stats == null || !IsStatsTypeMatching(item.itemType, item.stats))
        {
            switch (item.itemType)
            {
                case ItemSO_Copilot.ItemType.Equipment:
                    item.stats = new EquipmentStats();
                    break;
                case ItemSO_Copilot.ItemType.Consumable:
                    item.stats = new ConsumableStats();
                    break;
                case ItemSO_Copilot.ItemType.Weapon:
                    item.stats = new WeaponStats();
                    break;
                case ItemSO_Copilot.ItemType.Other:
                    item.stats = new OtherStats();
                    break;
                default:
                    item.stats = null;
                    break;
            }
            EditorUtility.SetDirty(item);
        }
    }

    private bool IsStatsTypeMatching(ItemSO_Copilot.ItemType itemType, BaseItemStats_Copilot stats)
    {
        switch (itemType)
        {
            case ItemSO_Copilot.ItemType.Equipment:
                return stats is EquipmentStats;
            case ItemSO_Copilot.ItemType.Consumable:
                return stats is ConsumableStats;
            case ItemSO_Copilot.ItemType.Weapon:
                return stats is WeaponStats;
            case ItemSO_Copilot.ItemType.Other:
                return stats is OtherStats;
            default:
                return stats == null;
        }
    }
}
#endif
