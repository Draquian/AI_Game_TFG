using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item", order = 0)]
public class ItemSO_Copilot : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    public enum ItemType { Generic, Equipment, Consumable, ClassItem }
    public ItemType itemType;

    // Additional properties can be added here, e.g., description, stats, etc.
    [TextArea]
    public string description;

    public void Interact(GameObject interactor)
    {
        Debug.Log("Item Interaction");
    }
}
