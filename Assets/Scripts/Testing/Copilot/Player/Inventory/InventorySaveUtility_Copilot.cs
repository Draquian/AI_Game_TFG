using System.Collections.Generic;
using UnityEngine;

public static class InventorySaveUtility_Copilot
{
    /// <summary>
    /// Creates a data snapshot of the current inventory.
    /// </summary>
    public static InventoryData_Copilot CreateInventoryData(Inventory_Copilot inventory)
    {
        InventoryData_Copilot data = new InventoryData_Copilot();

        // Go through each slot in your live inventory.
        foreach (var slot in inventory.slots)
        {
            InventorySlotData_Copilot slotData = new InventorySlotData_Copilot();
            if (slot.item != null)
            {
                slotData.hasItem = true;
                // Use a unique identifier from your item.
                slotData.itemID = slot.item.name;
            }
            else
            {
                slotData.hasItem = false;
                slotData.itemID = "";
            }
            data.slots.Add(slotData);
        }
        return data;
    }

    /// <summary>
    /// Applies saved inventory data back into the live inventory.
    /// </summary>
    /// <remarks>
    /// This method assumes you have a way to map an itemID back to your ItemSO_Copilot instance.
    /// In this example, we're using a hypothetical InventoryDatabase_Copilot.GetItemByID method.
    /// </remarks>
    public static void ApplyInventoryData(Inventory_Copilot inventory, InventoryData_Copilot data)
    {
        // Ensure that the number of slots in the saved data matches or is compatible.
        for (int i = 0; i < data.slots.Count && i < inventory.slots.Count; i++)
        {
            InventorySlotData_Copilot slotData = data.slots[i];
            if (slotData.hasItem)
            {
                // Retrieve the ItemSO_Copilot instance based on the itemID.
                // You must implement this lookup (for example, by using a dictionary or a database).
                //inventory.slots[i].item = InventoryDatabase_Copilot.GetItemByID(slotData.itemID);
            }
            else
            {
                inventory.slots[i].item = null;
            }
        }
        // Refresh the UI if needed.
        inventory.RefreshInventoryUI();
    }
}
