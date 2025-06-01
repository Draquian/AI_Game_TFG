using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI_Copilot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Tooltip("Slot index within the inventory.")]
    public int slotIndex;

    [Tooltip("Reference to the Inventory Controller—the player's Inventory.")]
    public Inventory_Copilot inventoryController;

    [Tooltip("The inventory that owns this slot.")]
    public Inventory_Copilot owningInventory;

    // New: Reference to the target external inventory.
    [Tooltip("Reference to the external inventory (target for right click transfers).")]
    public Inventory_Copilot externalInventoryTarget;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas localCanvas;

    // Static reference to a dedicated drag canvas—ensure a Canvas in your scene is tagged "DragCanvas".
    public static Canvas dragCanvas;

    // Flag to indicate if the drop was successfully handled.
    private bool dropSuccessful = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        localCanvas = GetComponentInParent<Canvas>();

        // Try to find a drag canvas if not already assigned.
        if (dragCanvas == null)
        {
            GameObject dcObj = GameObject.FindWithTag("DragCanvas");
            if (dcObj != null)
                dragCanvas = dcObj.GetComponent<Canvas>();
            else
                dragCanvas = localCanvas; // Fallback.
        }

        // If inventoryController is not assigned, assume the player's Inventory is the controller.
        if (inventoryController == null)
        {
            inventoryController = GameObject.FindObjectOfType<Inventory_Copilot>();
            // (Optionally, you might narrow this search to the player using tags or a name.)
            if (inventoryController == null)
                Debug.LogError("SlotUI: Inventory Controller (Player Inventory) not found in scene.");
        }

        // If owningInventory is not set, try a simple heuristic: if this slot is under the external inventory panel, use that.
        if (owningInventory == null && inventoryController != null)
        {
            // In a multi-panel setup, you might inspect the hierarchy.
            // For this example, default to the player's inventory.
            owningInventory = inventoryController;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dropSuccessful = false; // reset flag on each drag
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        // Reparent to drag canvas so this slot appears on top.
        transform.SetParent(dragCanvas.transform, false);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Only revert to original parent if no valid drop occurred.
        if (!dropSuccessful)
        {
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
        }
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Get the SlotUI component from the dragged object.
        SlotUI_Copilot droppedSlot = eventData.pointerDrag?.GetComponent<SlotUI_Copilot>();
        if (droppedSlot != null && droppedSlot != this)
        {
            Inventory_Copilot sourceInventory = droppedSlot.owningInventory;
            Inventory_Copilot destinationInventory = this.owningInventory;

            if (sourceInventory == null || destinationInventory == null)
            {
                Debug.LogError("SlotUI.OnDrop: One or both owning inventories are null.");
                return;
            }

            // Use the inventory controller (the player's inventory) to transfer the item.
            inventoryController.TransferItem(sourceInventory, droppedSlot.slotIndex,
                                             destinationInventory, this.slotIndex);


            // Refresh both inventories' UIs.
            sourceInventory.RefreshInventoryUI();
            if (sourceInventory != destinationInventory)
                destinationInventory.RefreshInventoryUI();

            // Mark drop as successful so OnEndDrag won't revert the dragged object.
            dropSuccessful = true;

            // Remove the dragged UI slot so that it doesn't remain on the drag canvas.
            Destroy(eventData.pointerDrag);

            // (Optional) You can also call Destroy(gameObject) here if you want to force the target slot UI to be rebuilt.
            // But if your UI refresh properly destroys all children in the container then this may not be necessary.
        }
    }

    // New: Implement the IPointerClickHandler for right-click logic.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Check if the external inventory is open.
            // Retrieve the ExternalInventory component (assumes one exists in the scene).
            ExternalInventory_Copilot extInv = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory_Copilot>().externalInv;
            if (extInv == null || extInv.externalInventoryUIPanel == null ||
                !extInv.externalInventoryUIPanel.activeSelf)
            {
                Debug.Log("External inventory is not open; cannot move item.");
                return;
            }

            // Only move items from the player's inventory (we assume owningInventory == inventoryController for player slots).
            if (owningInventory == inventoryController)
            {
                // Ensure the external inventory reference is assigned.
                if (externalInventoryTarget == null)
                {
                    if (extInv != null)
                        externalInventoryTarget = extInv.externalInventory;
                }
                // Call the function to move the item.
                bool moved = inventoryController.MoveItemToExternalInventory(owningInventory.gameObject, slotIndex, externalInventoryTarget.gameObject);
                if (moved)
                {
                    Debug.Log("Item moved to external inventory via right click.");
                }
                else
                {
                    Debug.Log("Right-click move failed (slot empty or external inventory full).");
                }
            }
        }
    }
}