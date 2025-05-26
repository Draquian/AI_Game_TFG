using UnityEngine;

public class ExternalInventory_Copilot : MonoBehaviour
{
    // This could be a smaller inventory, for instance.
    private Inventory_Copilot inventory;

    private void Start()
    {
        inventory = gameObject.AddComponent<Inventory_Copilot>();
        // Customize for this external inventory. For example, a chest might only have 10 slots:
        inventory.totalSlots = 10;
        // You can disable or repurpose the special hotbar behavior if needed.
        inventory.hotbarSlots = 0;
    }

    // You can add functions here that the player calls when interacting with this object,
    // such as opening the chest UI.
    public void OpenInventory()
    {
        // Implement UI opening logic.
        Debug.Log("External inventory opened.");
    }
}
