using System;
using System.Collections.Generic;

[Serializable]
public class InventorySlotData_Copilot
{
    // True if an item is present in this slot.
    public bool hasItem;
    // Unique identifier for the item (using the item's name in this example).
    public string itemID;
}

[Serializable]
public class InventoryData_Copilot
{
    public List<InventorySlotData_Copilot> slots = new List<InventorySlotData_Copilot>();
}
