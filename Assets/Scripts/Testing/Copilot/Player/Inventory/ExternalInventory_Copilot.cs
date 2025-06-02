using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExternalInventory_Copilot : MonoBehaviour, IInteractable_Copilot
{
    // The external Inventory component (e.g., for a chest)
    public Inventory_Copilot externalInventory;

    // UI Panels for the external inventory and for the player's inventory.
    [Tooltip("Panel for the external inventory UI (e.g., chest UI)")]
    public GameObject externalInventoryUIPanel;
    [Tooltip("Panel for the player's inventory UI")]
    public GameObject playerInventoryUIPanel;

    // Reference to the player's Inventory component.
    public Inventory_Copilot playerInventory;
    PlayerController_Copilot playerController;

    // --- UI-related fields for external inventory ---
    [Tooltip("Prefab for an individual slot in the external inventory UI")]
    public GameObject externalSlotPrefab;
    [Tooltip("Parent transform under the external inventory panel where slots will be instantiated")]
    public Transform externalSlotParent;

    // List to hold the instantiated external inventory slot GameObjects.
    private List<GameObject> externalSlotGameObjects = new List<GameObject>();

    // --- Layout settings for distributing the slots ---
    [Header("Layout Settings")]
    [Tooltip("Number of columns for the external inventory slots.")]
    public int columns = 5;
    [Tooltip("Spacing between external inventory slots.")]
    public float slotSpacing = 10f;
    [Tooltip("Size for each external inventory slot (width x height).")]
    public Vector2 slotSize = new Vector2(80, 80);


    private void Start()
    {
        // Initialize external inventory if it has not been assigned.
        if (externalInventory == null)
        {
            externalInventory = gameObject.AddComponent<Inventory_Copilot>();
            externalInventory.totalSlots = 10;  // For example, a chest might have 10 slots.
            externalInventory.hotbarSlots = 0;    // External inventories do not have hotbar slots.
        }

        if (playerController == null)
        {
            GameObject aux = GameObject.FindGameObjectWithTag("Player");
            playerController = aux.GetComponent<PlayerController_Copilot>();
        }

        // Initially hide the inventory panels.
        if (externalInventoryUIPanel != null)
            externalInventoryUIPanel.SetActive(false);
        if (playerInventoryUIPanel != null)
            playerInventoryUIPanel.SetActive(false);

        externalSlotParent = externalInventoryUIPanel.transform;

        PopulateChest();
    }

    // Subscribe to the external inventory change event so that the UI refreshes automatically.
    private void OnEnable()
    {
        if (externalInventory != null)
            externalInventory.OnInventoryChanged += RefreshExternalInventoryUI;
    }

    private void OnDisable()
    {
        if (externalInventory != null)
            externalInventory.OnInventoryChanged -= RefreshExternalInventoryUI;
    }

    /// <summary>
    /// Called when the player interacts with this object (e.g., a chest).
    /// Displays both the external inventory UI panel and the player's inventory UI panel.
    /// Also refreshes both UIs.
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (externalInventoryUIPanel != null)
            externalInventoryUIPanel.SetActive(true);
        if (playerInventoryUIPanel != null)
            playerInventoryUIPanel.SetActive(true);

        // Refresh the external inventory UI slots.
        RefreshExternalInventoryUI();

        // Refresh the player's inventory UI if your Inventory script supports it.
        if (playerInventory != null)
        {
            playerInventory.RefreshInventoryUI();
            playerInventory.externalInv = gameObject.GetComponent<ExternalInventory_Copilot>();
        }

        playerController.lockCameraRotation = true;

        // Show and unlock the cursor when paused.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Closes both the external and player inventory UI panels.
    /// </summary>
    public void CloseInventory()
    {
        if (externalInventoryUIPanel != null)
            externalInventoryUIPanel.SetActive(false);
        if (playerInventoryUIPanel != null)
            playerInventoryUIPanel.SetActive(false);

        playerController.lockCameraRotation = false;

        // Hide and lock the cursor when the game is resumed.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Transfers an item from one inventory to the other.
    /// This function can be called by your drag-and-drop handlers.
    /// </summary>
    /// <param name="sourceInventory">Inventory from which the item is moved.</param>
    /// <param name="sourceIndex">Slot index in the source inventory.</param>
    /// <param name="destinationInventory">Inventory to which the item is moved.</param>
    /// <param name="destinationIndex">Slot index in the destination inventory.</param>
    public void TransferItem(Inventory_Copilot sourceInventory, int sourceIndex, Inventory_Copilot destinationInventory, int destinationIndex)
    {
        if (sourceIndex < 0 || sourceIndex >= sourceInventory.slots.Count ||
            destinationIndex < 0 || destinationIndex >= destinationInventory.slots.Count)
        {
            Debug.LogWarning("TransferItem: Invalid slot indices.");
            return;
        }

        InventorySlot_Copilot sourceSlot = sourceInventory.slots[sourceIndex];
        InventorySlot_Copilot destinationSlot = destinationInventory.slots[destinationIndex];

        // If destination slot is empty, move the item; if not, swap them.
        if (destinationSlot.item == null)
        {
            destinationSlot.item = sourceSlot.item;
            sourceSlot.item = null;
        }
        else
        {
            ItemSO_Copilot temp = destinationSlot.item;
            destinationSlot.item = sourceSlot.item;
            sourceSlot.item = temp;
        }

        // Update both inventories' UI.
        sourceInventory.RefreshInventoryUI();
        destinationInventory.RefreshInventoryUI();
    }

    // Call this function to refresh the external inventory UI,
    // placing all slots in a nicely distributed grid.
    public void RefreshExternalInventoryUI()
    {
        // Clear previous UI objects.
        foreach (GameObject slot in externalSlotGameObjects)
        {
            Destroy(slot);
        }
        externalSlotGameObjects.Clear();

        // Grid layout settings.
        int columns = 4;         // number of columns per row
        float slotSpacing = 10f; // space between slots
        Vector2 slotSize = new Vector2(80, 80);
        Vector2 margin = new Vector2(10, 10); // offset from the top-left corner

        // Make sure the parent has a RectTransform.
        RectTransform parentRect = externalSlotParent.GetComponent<RectTransform>();
        if (parentRect == null)
        {
            Debug.LogWarning("ExternalInventory: The externalSlotParent must have a RectTransform.");
            return;
        }

        // Create a slot UI for each slot in the external inventory.
        for (int i = 0; i < externalInventory.slots.Count; i++)
        {
            GameObject slotGO = Instantiate(externalSlotPrefab, externalSlotParent);
            slotGO.name = "ExternalSlot " + i;

            // Setup the RectTransform.
            RectTransform slotRect = slotGO.GetComponent<RectTransform>();
            slotRect.sizeDelta = slotSize;

            int row = i / columns;
            int column = i % columns;
            float xPos = margin.x + column * (slotSize.x + slotSpacing);
            float yPos = -margin.y - row * (slotSize.y + slotSpacing);
            slotRect.anchoredPosition = new Vector2(xPos, yPos);

            // (Assume each externalSlotPrefab has an ExternalSlotUI component attached)
            ExternalSlotUI_Copilot slotUI = slotGO.GetComponent<ExternalSlotUI_Copilot>();
            if (slotUI == null)
            {
                slotUI = slotGO.AddComponent<ExternalSlotUI_Copilot>();
            }
            // Set up the slot's index and reference its owning inventory.
            slotUI.slotIndex = i;
            slotUI.owningInventory = externalInventory;
            // Set the inventory controller to the player inventory.
            slotUI.inventoryController = externalInventory;

            // Assume that in your slot prefab you have a background Image that is always enabled.
            Image backgroundImage = slotGO.GetComponent<Image>();
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
            Image iconImage = slotGO.GetComponentInChildren<Image>(); // This could be a separate object.
            InventorySlot_Copilot slotData = externalInventory.slots[i];
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

            externalSlotGameObjects.Add(slotGO);
        }
    }

    /// <summary>
    /// Swaps (or moves) items between two slots in the external inventory.
    /// This function mimics the function in the Inventory controller but is applied here for internal swaps.
    /// </summary>
    /// <param name="sourceIndex">The index of the slot that is being dragged.</param>
    /// <param name="destinationIndex">The index of the slot where the item is dropped.</param>
    public void SwapItemsInExternalInventory(int sourceIndex, int destinationIndex)
    {
        // Validate indices.
        if (sourceIndex < 0 || sourceIndex >= externalInventory.slots.Count ||
            destinationIndex < 0 || destinationIndex >= externalInventory.slots.Count)
        {
            Debug.LogWarning("SwapItemsInExternalInventory: Provided slot indices are out of range.");
            return;
        }

        // If moving to the same slot, do nothing.
        if (sourceIndex == destinationIndex)
            return;

        // Retrieve the two slots.
        InventorySlot_Copilot sourceSlot = externalInventory.slots[sourceIndex];
        InventorySlot_Copilot destSlot = externalInventory.slots[destinationIndex];

        // Swap items.
        ItemSO_Copilot temp = destSlot.item;
        destSlot.item = sourceSlot.item;
        sourceSlot.item = temp;

        // Refresh the external inventory UI so that the changes appear.
        RefreshExternalInventoryUI();
    }

    /// <summary>
    /// Distributes all external inventory slots evenly in the externalSlotParent panel using grid calculations.
    /// </summary>
    private void DistributeSlots()
    {
        // Ensure externalSlotParent is a RectTransform.
        RectTransform parentRect = externalSlotParent.GetComponent<RectTransform>();
        if (parentRect == null) return;

        int totalSlots = externalSlotGameObjects.Count;
        int rows = Mathf.CeilToInt((float)totalSlots / columns);

        // Iterate over each slot and assign a new anchored position.
        for (int i = 0; i < totalSlots; i++)
        {
            int row = i / columns;
            int column = i % columns;
            RectTransform slotRect = externalSlotGameObjects[i].GetComponent<RectTransform>();

            // Calculate position: X based on column (plus spacing) and Y based on row.
            float xPos = column * (slotSize.x + slotSpacing);
            float yPos = -row * (slotSize.y + slotSpacing);  // negative y moves the slot downward

            slotRect.anchoredPosition = new Vector2(xPos, yPos);
        }
    }

    public void PopulateChest()
    {
        // Load all items of type ItemSO from the Resources/Items folder.
        ItemSO_Copilot[] availableItems = Resources.LoadAll<ItemSO_Copilot>("Items");
        if (availableItems == null || availableItems.Length == 0)
        {
            Debug.LogWarning("No items found in Resources/Items folder!");
            return;
        }

        // Define the probability to place an item in each slot (e.g., 30%).
        float dropChance = 0.1f;

        // Loop through each slot in the external inventory.
        for (int i = 0; i < externalInventory.slots.Count; i++)
        {
            // With a probability of dropChance, randomly assign an item.
            if (Random.value < dropChance)
            {
                int randomIndex = Random.Range(0, availableItems.Length);
                externalInventory.slots[i].item = availableItems[randomIndex];
            }
            else
            {
                // Otherwise, leave the slot empty.
                externalInventory.slots[i].item = null;
            }
        }

        // Refresh the external inventory UI to reflect these changes.
        RefreshExternalInventoryUI();
    }
}
