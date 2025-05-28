using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HotbarUI_Copilot : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player's Inventory component.")]
    public Inventory_Copilot playerInventory;

    [Tooltip("The parent panel where hotbar slots will be instantiated.")]
    public Transform hotbarPanel;

    [Tooltip("Prefab for an individual hotbar slot. It should have an Image component (and optionally a SlotUI script if you enable drag&drop).")]
    public GameObject slotPrefab;

    [Header("Layout Settings")]
    [Tooltip("Spacing between hotbar slots.")]
    public float slotSpacing = 10f;

    [Tooltip("Size for each hotbar slot (width x height).")]
    public Vector2 slotSize = new Vector2(80, 80);

    // Keeps a reference to instantiated slot GameObjects so you can refresh and reposition them.
    private List<GameObject> slotGameObjects = new List<GameObject>();

    private void Start()
    {
        RefreshHotbar();
    }

    // Subscribe to the inventory event so that the hotbar automatically refreshes
    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged += RefreshHotbar;
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshHotbar;
    }

    /// <summary>
    /// Checks if any of the hotbar slots (the first "hotbarSlots" in the Inventory)
    /// have changed, then updates the hotbar accordingly.
    /// Currently, it always refreshes when the inventory event occurs.
    /// </summary>
    private void CheckAndRefreshHotbar()
    {
        // In a more advanced implementation, you might pass slot indices or data differences.
        // For now, we simply refresh the entire hotbar if there’s any change.
        RefreshHotbar();
    }

    /// <summary>
    /// Clears and re-instantiates the hotbar UI elements based on the player's inventory hotbar settings.
    /// </summary>
    public void RefreshHotbar()
    {
        // Clear any old slots from the hotbar panel.
        foreach (GameObject slot in slotGameObjects)
        {
            Destroy(slot);
        }
        slotGameObjects.Clear();

        // Use the hotbar slots count from the Inventory script.
        int hotbarCount = playerInventory.hotbarSlots;

        // Instantiate a UI slot for each hotbar slot.
        for (int i = 0; i < hotbarCount; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, hotbarPanel);
            slotGO.name = "Hotbar Slot " + i;

            // If the Inventory has an assigned item in this slot, show its icon.
            // (Assuming the hotbar uses the first few slots in your Inventory.slots list.)
            if (i < playerInventory.slots.Count)
            {
                InventorySlot_Copilot slotData = playerInventory.slots[i];
                Image iconImage = slotGO.GetComponentInChildren<Image>();

                if (slotData.item != null && slotData.item.icon != null)
                {
                    iconImage.sprite = slotData.item.icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            slotGameObjects.Add(slotGO);
        }

        // Position the hotbar slots within the panel.
        DistributeSlots();
    }

    /// <summary>
    /// Arranges the hotbar slots in a horizontal row.
    /// </summary>
    private void DistributeSlots()
    {
        // We'll assume a simple horizontal row.
        for (int i = 0; i < slotGameObjects.Count; i++)
        {
            RectTransform rt = slotGameObjects[i].GetComponent<RectTransform>();

            // Calculate a new anchored position so that slots are spaced out evenly.
            float xPos = (i * (slotSize.x + slotSpacing)) - 150;
            rt.anchoredPosition = new Vector2(xPos, 0);
            rt.sizeDelta = slotSize;
        }
    }
}
