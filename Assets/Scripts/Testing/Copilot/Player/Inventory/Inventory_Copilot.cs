using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Copilot : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int totalSlots = 20;
    public int hotbarSlots = 4;
    public List<InventorySlot_Copilot> slots = new List<InventorySlot_Copilot>();

    // Event for UI refresh; UI scripts subscribe to this event.
    public event Action OnInventoryChanged;

    public ItemSO_Copilot testItem1;
    public ItemSO_Copilot testItem2;

    public GameObject externalInv;

    private void Awake()
    {
        // Initialize the slots list.
        for (int i = 0; i < totalSlots; i++)
        {
            InventorySlot_Copilot newSlot = new InventorySlot_Copilot();
            newSlot.item = null;
            slots.Add(newSlot);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            AddItem(testItem1);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddItem(testItem2);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            MoveItemToExternalInventory(gameObject, 1, externalInv);
        }

    }

    /// <summary>
    /// Basic method to add an item to the inventory.
    /// </summary>
    public bool AddItem(ItemSO_Copilot itemToAdd)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == null)
            {
                slots[i].item = itemToAdd;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        Debug.Log("Inventory is full!");
        return false;
    }

    /// <summary>
    /// Removes an item from a given slot.
    /// </summary>
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= slots.Count)
            return;
        slots[index].item = null;
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Refresh method that simply invokes the change event.
    /// (UI scripts subscribe to this method.)
    /// </summary>
    public void RefreshInventoryUI()
    {
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// TransferItem swaps items between two inventories. Since the player's Inventory
    /// becomes the InventoryController, you call this method on the player's inventory.
    /// </summary>
    /// <param name="sourceInventory">Inventory containing the dragged item.</param>
    /// <param name="sourceIndex">Index in the source inventory.</param>
    /// <param name="destinationInventory">Inventory where the item is being dropped.</param>
    /// <param name="destinationIndex">Index in the destination inventory.</param>
    public void TransferItem(Inventory_Copilot sourceInventory, int sourceIndex, Inventory_Copilot destinationInventory, int destinationIndex)
    {
        // Validate inputs.
        if (sourceInventory == null || destinationInventory == null)
        {
            Debug.LogWarning("TransferItem: One or both inventories are null.");
            return;
        }
        if (sourceIndex < 0 || sourceIndex >= sourceInventory.slots.Count ||
            destinationIndex < 0 || destinationIndex >= destinationInventory.slots.Count)
        {
            Debug.LogWarning("TransferItem: Invalid slot indices.");
            return;
        }

        InventorySlot_Copilot sourceSlot = sourceInventory.slots[sourceIndex];
        InventorySlot_Copilot destSlot = destinationInventory.slots[destinationIndex];

        // Swap the items (this logic works whether destination is empty or not).
        ItemSO_Copilot temp = destSlot.item;
        destSlot.item = sourceSlot.item;
        sourceSlot.item = temp;

        // Refresh UI on both inventories.
        sourceInventory.RefreshInventoryUI();
        if (sourceInventory != destinationInventory)
            destinationInventory.RefreshInventoryUI();
    }

    /// <summary>
    /// Transfers an item from a source inventory at a given slot index to the first empty slot of the external inventory.
    /// </summary>
    /// <param name="sourceInventory">The Inventory from which to remove the item.</param>
    /// <param name="sourceIndex">The index in the source inventory where the item currently is.</param>
    /// <param name="externalInventory">The target external Inventory in which to place the item.</param>
    /// <returns>True if the transfer was successful, false otherwise.</returns>
    public bool MoveItemToExternalInventory(GameObject sourceInventory, int sourceIndex, GameObject externalInventory)
    {
        // Validate inventories and source index.
        if (sourceInventory == null || externalInventory == null)
        {
            Debug.LogError("MoveItemToExternalInventory: One or both inventory references are null.");

            if (sourceInventory == null)
                Debug.LogError(this.name + " MoveItemToExternalInventory: sourceInventory references are null.");

            if (externalInventory == null)
                Debug.LogError("MoveItemToExternalInventory: externalInventory references are null.");

            return false;
        }

        Inventory_Copilot invnetory = sourceInventory.GetComponent<Inventory_Copilot>();

        if (sourceIndex < 0 || sourceIndex >= invnetory.slots.Count)
        {
            Debug.LogError("MoveItemToExternalInventory: Invalid source index.");
            return false;
        }
        // Check if the source slot actually has an item.
        if (invnetory.slots[sourceIndex].item == null)
        {
            Debug.Log("MoveItemToExternalInventory: Source slot is empty. No item to move.");
            return false;
        }

        Inventory_Copilot ex_invnetory = externalInventory.GetComponent<Inventory_Copilot>();

        // Scan for the first empty slot in the external inventory.
        int targetIndex = -1;
        for (int i = 0; i < ex_invnetory.slots.Count; i++)
        {
            if (ex_invnetory.slots[i].item == null)
            {
                targetIndex = i;
                break;
            }
        }
        if (targetIndex == -1)
        {
            Debug.Log("MoveItemToExternalInventory: External inventory is full.");
            return false;
        }

        // Transfer the item.
        ex_invnetory.slots[targetIndex].item = invnetory.slots[sourceIndex].item;
        invnetory.slots[sourceIndex].item = null;

        // Refresh both UI panels.
        invnetory.RefreshInventoryUI();
        if (sourceInventory.GetComponent<ExternalInventory_Copilot>()) sourceInventory.GetComponent<ExternalInventory_Copilot>().RefreshExternalInventoryUI();

        ex_invnetory.RefreshInventoryUI();
        if (externalInventory.GetComponent<ExternalInventory_Copilot>()) externalInventory.GetComponent<ExternalInventory_Copilot>().RefreshExternalInventoryUI();

        Debug.Log("Item moved from source slot " + sourceIndex +
                  " to external inventory slot " + targetIndex);
        return true;
    }
}
