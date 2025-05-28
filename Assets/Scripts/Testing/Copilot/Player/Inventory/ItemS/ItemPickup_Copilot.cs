using UnityEngine;

public class ItemPickup_Copilot : MonoBehaviour
{
    // Reference the item data that defines this pickup
    //public ItemSO_Copilot itemData;

    public string name = "";

    public void Interact(GameObject interactor)
    {

        // When a player interacts, the item data is sent to the inventory
        // Example: InventoryManager.Instance.AddItem(itemData);
        Debug.Log("Picked up: " + name);
    }
}
