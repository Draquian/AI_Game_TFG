using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory_Copilot : MonoBehaviour
{
    [Header("Inventory Setup")]
    [Tooltip("Total number of slots in the inventory.")]
    public int totalSlots = 20;

    [Tooltip("Number of special (hotbar) slots. The first slot is used to assign player class.")]
    public int hotbarSlots = 4;

    // List of slots that holds the item data.
    public List<InventorySlot_Copilot> slots = new List<InventorySlot_Copilot>();

    public event Action OnInventoryChanged;

    public ItemSO_Copilot testItem1;
    public ItemSO_Copilot testItem2;

    private void Awake()
    {
        // Initialize empty slots on load.
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
            RemoveItem(0);
        }

    }

    /// <summary>
    /// Tries to add an item (as a ScriptableObject) to the first available empty slot in the inventory.
    /// If the item goes to slot 0 and is a class item, the player’s class is updated.
    /// </summary>
    public bool AddItem(ItemSO_Copilot itemToAdd)
    {
        Debug.Log("Add item: " + itemToAdd.itemName);
        for (int i = 0; i < slots.Count; i++)
        {
            Debug.Log("Test 1");

            if (slots[i].item == null)
            {
                Debug.Log("Test 2");

                slots[i].item = itemToAdd;

                // Check if the added item in slot 0 is a class item.
                if (i == 0 && itemToAdd.itemType == ItemSO_Copilot.ItemType.ClassItem)
                {
                    Debug.Log("Test 3");

                    // PlayerController.Instance.AssignClass(itemToAdd); // Your class assignment logic here.
                    Debug.Log("Player class updated based on item: " + itemToAdd.itemName);
                }

                // Trigger the inventory changed event
                OnInventoryChanged?.Invoke();

                return true;
                Debug.Log("Test 4");

            }
            Debug.Log("Test5");

        }
        Debug.Log("Inventory is full. Could not add: " + itemToAdd.itemName);
        return false;
    }

    /// <summary>
    /// Removes the item from the provided slot index.
    /// If the removed slot is the class slot, update the player’s class accordingly.
    /// </summary>
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            Debug.LogWarning("Invalid inventory slot index: " + index);
            return;
        }
        if (index == 0)
        {
            // PlayerController.Instance.ResetClass();
            Debug.Log("Player class removed from slot 0.");
        }
        slots[index].item = null;
    }

    /// <summary>
    /// Swap items between two slot indices and update any associated logic.
    /// </summary>
    public void SwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= slots.Count || indexB < 0 || indexB >= slots.Count)
        {
            Debug.LogWarning("Invalid indices for swap: " + indexA + " & " + indexB);
            return;
        }

        ItemSO_Copilot temp = slots[indexA].item;
        slots[indexA].item = slots[indexB].item;
        slots[indexB].item = temp;

        // Update class slot if the first slot is involved.
        if (indexA == 0 || indexB == 0)
        {
            ItemSO_Copilot potentialClassItem = slots[0].item;
            if (potentialClassItem != null && potentialClassItem.itemType == ItemSO_Copilot.ItemType.ClassItem)
            {
                // PlayerController.Instance.AssignClass(potentialClassItem);
                Debug.Log("Player class updated after swap.");
            }
            else
            {
                // PlayerController.Instance.ResetClass();
                Debug.Log("Player class reset after swap.");
            }
        }
    }

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public void ClearInventory()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].item = null;
        }
        Debug.Log("Inventory cleared.");
    }
}