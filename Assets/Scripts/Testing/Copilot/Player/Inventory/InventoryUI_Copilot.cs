using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI_Copilot : MonoBehaviour
{
    [Header("UI References")]
    public Inventory_Copilot playerInventory;
    public GameObject slotPrefab;   // Must include an Image and the SlotUI component.
    public Transform inventoryPanel; // Parent panel for the inventory grid.

    [Header("Grid Layout Options")]
    public int columns = 5;
    public float slotSpacing = 10f;
    public Vector2 slotSize = new Vector2(80, 80);

    // List of instantiated slot GameObjects.
    private List<GameObject> slotGameObjects = new List<GameObject>();

    private void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// Instantiates slots based on the player's inventory data, assigns drag-and-drop functionality,
    /// and lays out the slots on the panel.
    /// </summary>
    public void RefreshUI()
    {
        // Clear previous slots.
        foreach (var slotGO in slotGameObjects)
        {
            Destroy(slotGO);
        }
        slotGameObjects.Clear();

        for (int i = 0; i < playerInventory.totalSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, inventoryPanel);
            slotGO.name = "Slot " + i;

            // Update the UI with the appropriate icon if an item exists in this inventory slot.
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

            // Example: for the first few slots (hotbar), add a special marker/color.
            if (i < playerInventory.hotbarSlots)
            {
                if (i == 0)
                    slotGO.GetComponent<Image>().color = Color.cyan;  // The class slot.
            }

            // Make sure the slot has the drag-and-drop script.
            SlotUI_Copilot slotUI = slotGO.GetComponent<SlotUI_Copilot>();
            if (slotUI == null)
                slotUI = slotGO.AddComponent<SlotUI_Copilot>();
            slotUI.slotIndex = i;
            slotUI.inventoryUI = this;

            slotGameObjects.Add(slotGO);
        }

        // Arrange slots in a grid layout.
        DistributeSlots();
    }

    /// <summary>
    /// Manually positions each slot in the inventory panel using grid parameters.
    /// </summary>
    private void DistributeSlots()
    {
        // Compute total rows required.
        int totalSlots = slotGameObjects.Count;
        int rows = Mathf.CeilToInt((float)totalSlots / columns);

        // Iterate over each slot and calculate a new anchored position.
        for (int i = 0; i < totalSlots; i++)
        {
            int row = i / columns;
            int col = i % columns;
            RectTransform slotRect = slotGameObjects[i].GetComponent<RectTransform>();

            // Calculate the position based on column, row, slot size, and spacing.
            float xPos = col * (slotSize.x + slotSpacing);
            float yPos = -row * (slotSize.y + slotSpacing);  // negative y moves the slot downward

            slotRect.anchoredPosition = new Vector2(xPos, yPos);
            slotRect.sizeDelta = slotSize;
        }
    }

    /// <summary>
    /// Called when a drag-and-drop operation completes.
    /// Swaps the items in the inventory between two slot indices.
    /// </summary>
    public void SwapSlots(int indexA, int indexB)
    {
        playerInventory.SwapItems(indexA, indexB);
    }

    private void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged += RefreshUI;
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshUI;
    }
}
