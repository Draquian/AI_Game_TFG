using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI_Copilot : MonoBehaviour
{
    [Header("Player Inventory UI Setup")]
    [Tooltip("Reference to the player's Inventory component.")]
    public Inventory_Copilot playerInventory;

    [Tooltip("Prefab for an individual inventory slot. This prefab should have the SlotUI component.")]
    public GameObject slotPrefab;

    [Tooltip("Parent transform for the slot UI objects.")]
    public Transform slotParent;

    [Tooltip("Number of columns in the grid layout.")]
    public int columns = 5;
    [Tooltip("Width and height size of each slot.")]
    public Vector2 slotSize = new Vector2(80, 80);
    [Tooltip("Spacing between slots.")]
    public float slotSpacing = 10f;
    [Tooltip("Margin (offset) from the top-left corner of the panel.")]
    public Vector2 margin = new Vector2(10, 10);

    // List to keep track of instantiated slot UI objects.
    private List<GameObject> slotGameObjects = new List<GameObject>();

    private void Start()
    {
        if (playerInventory == null)
        {
            Debug.LogError("InventoryUI: No player inventory assigned!");
            return;
        }

        // Subscribe to inventory changes.
        playerInventory.OnInventoryChanged += RefreshUI;

        // Build the initial UI.
        RefreshUI();
    }

    /// <summary>
    /// Refreshes the player's Inventory UI:
    /// Destroys any existing slot objects and instantiates new ones based on the current contents of the inventory.
    /// </summary>
    public void RefreshUI()
    {
        // Clear previous slots.
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }
        slotGameObjects.Clear();

        int numSlots = playerInventory.slots.Count;
        for (int i = 0; i < numSlots; i++)
        {
            // Instantiate slot prefab as child of the parent panel.
            GameObject newSlot = Instantiate(slotPrefab, slotParent);
            newSlot.name = "Slot " + i;

            // Get the RectTransform of the slot.
            RectTransform slotRect = newSlot.GetComponent<RectTransform>();
            // Set the slot size.
            slotRect.sizeDelta = slotSize;

            // Calculate grid position.
            int col = i % columns;
            int row = i / columns;

            // x-position: margin + (slot width + spacing) * column index.
            float xPos = margin.x + col * (slotSize.x + slotSpacing);
            // y-position: negative margin - (slot height + spacing) * row index.
            // Negative because in UI anchoredPosition, moving down the panel decreases Y.
            float yPos = -margin.y - row * (slotSize.y + slotSpacing);

            // Set the anchored position. (If the parent's pivot is top-left (0,1), this will properly start at the top-left corner.)
            slotRect.anchoredPosition = new Vector2(xPos, yPos);

            // Here, update the visuals for each slot.
            SlotUI_Copilot slotUI = newSlot.GetComponent<SlotUI_Copilot>();
            if (slotUI == null)
            {
                slotUI = newSlot.AddComponent<SlotUI_Copilot>();
            }
            slotUI.slotIndex = i;
            slotUI.owningInventory = playerInventory;
            slotUI.inventoryController = playerInventory; // For player inventory, the controller is itself.

            // Assume that in your slot prefab you have a background Image that is always enabled.
            Image backgroundImage = newSlot.GetComponent<Image>();
            if (backgroundImage != null)
            {
                // Make sure the Raycast Target is enabled.
                backgroundImage.raycastTarget = true;
                // Optionally, if you want the background to be invisible but still interactable:
                Color bgColor = backgroundImage.color;
                bgColor.a = 0.0f; // Make it fully transparent.
                backgroundImage.color = bgColor;
            }

            // For the item icon (child Image that shows the item) do not disable its Raycast Target,
            // just set its alpha to 0 when there's no item.
            Image iconImage = newSlot.GetComponentInChildren<Image>(); // This could be a separate object.
            InventorySlot_Copilot slotData = playerInventory.slots[i];
            if (slotData.item != null && slotData.item.icon != null)
            {
                iconImage.sprite = slotData.item.icon;
                // Fully opaque for an item.
                iconImage.color = new Color(1, 1, 1, 1);
            }
            else
            {
                // Clear the icon but ensure it still receives events (if needed).
                iconImage.color = new Color(1, 1, 1, 0);
                // Or, if you rely solely on the background for drop detection, you might not
                // need to modify iconImage's Raycast Target.
            }

            slotGameObjects.Add(newSlot);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks.
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshUI;
        }
    }
}
