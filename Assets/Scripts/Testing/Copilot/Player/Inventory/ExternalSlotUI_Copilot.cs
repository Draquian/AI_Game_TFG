using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExternalSlotUI_Copilot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Tooltip("Slot index within this external inventory.")]
    public int slotIndex;

    [Tooltip("Reference to the Inventory Controller (the player's Inventory).")]
    public Inventory_Copilot inventoryController;

    [Tooltip("The inventory that owns this slot (should be the external container's inventory).")]
    public Inventory_Copilot owningInventory;

    // New: Reference to the target external inventory.
    [Tooltip("Reference to the external inventory (target for right click transfers).")]
    public Inventory_Copilot playerInventoryTarget;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas localCanvas;

    // Static reference to a dedicated drag canvas – ensure a Canvas in your scene is tagged as "DragCanvas"
    public static Canvas dragCanvas;

    // Flag used for drag&drop handling.
    private bool dropSuccessful = false;

    private void Awake()
    {
        // Get the required UI components.
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        localCanvas = GetComponentInParent<Canvas>();

        // Find the drag canvas using the designated tag.
        if (dragCanvas == null)
        {
            GameObject dc = GameObject.FindWithTag("DragCanvas");
            if (dc != null)
                dragCanvas = dc.GetComponent<Canvas>();
            else
                dragCanvas = localCanvas; // Fallback if a dedicated drag canvas is not found.
        }

        // Auto-assign inventoryController if not manually set.
        /*if (inventoryController == null)
        {
            ExternalInventory_Copilot extInv = GetComponentInParent<ExternalInventory_Copilot>();
            if (extInv != null && extInv.playerInventory != null)
                inventoryController = extInv.playerInventory;
            else
                inventoryController = GameObject.FindObjectOfType<Inventory_Copilot>();
            if (inventoryController == null)
                Debug.LogError("ExternalSlotUI: Inventory Controller not found in the scene.");
        }*/

        // Auto-assign owningInventory (should be the external container's inventory).
        if (owningInventory == null)
        {
            ExternalInventory_Copilot extInv = GetComponentInParent<ExternalInventory_Copilot>();
            if (extInv != null && extInv.externalInventory != null)
                owningInventory = extInv.externalInventory;
            else
            {
                owningInventory = inventoryController;
                Debug.LogWarning("ExternalSlotUI: Owning inventory not assigned; defaulting to Inventory Controller.");
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dropSuccessful = false;
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        // Reparent this slot to the drag canvas so it renders on top.
        transform.SetParent(dragCanvas.transform, false);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dropSuccessful)
        {
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
        }
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Get the external slot from the dragged object.
        ExternalSlotUI_Copilot droppedSlot = eventData.pointerDrag?.GetComponent<ExternalSlotUI_Copilot>();
        if (droppedSlot != null && droppedSlot != this)
        {
            // Determine the source and destination inventories.
            Inventory_Copilot sourceInventory = droppedSlot.owningInventory;
            Inventory_Copilot destinationInventory = this.owningInventory;
            if (sourceInventory == null || destinationInventory == null)
            {
                Debug.LogError("ExternalSlotUI.OnDrop: One or both owning inventories are null.");
                return;
            }

            // Use the player's inventory (the controller) to perform the transfer.
            inventoryController.TransferItem(sourceInventory, droppedSlot.slotIndex,
                                             destinationInventory, this.slotIndex);

            // Refresh the UI appropriately.
            if (sourceInventory == destinationInventory)
            {
                // If both slots are in the external inventory, call its dedicated refresh method.
                ExternalInventory_Copilot extInv = GetComponentInParent<ExternalInventory_Copilot>();
                if (extInv != null)
                    extInv.RefreshExternalInventoryUI();
                else
                    sourceInventory.RefreshInventoryUI();
            }
            else
            {
                // Otherwise, refresh each inventory individually.
                sourceInventory.RefreshInventoryUI();
                destinationInventory.RefreshInventoryUI();
            }

            // Remove the dragged slot UI from the drag canvas.
            Destroy(eventData.pointerDrag);
        }
    }

    // New: Implement the IPointerClickHandler for right-click logic.
    public void OnPointerClick(PointerEventData eventData)
    {
        // Check for a right mouse click.
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // We only want to move items from the player's inventory.
            // In our setup, player's slots have owningInventory == inventoryController.
            if (owningInventory == inventoryController)
            {
                // Ensure a target external inventory is set.
                if (playerInventoryTarget == null)
                {
                    playerInventoryTarget = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory_Copilot>(); ;
                }
                // Call the method on the Inventory Controller.
                bool moved = inventoryController.MoveItemToExternalInventory(owningInventory.gameObject, slotIndex, playerInventoryTarget.gameObject);
                if (moved)
                {
                    Debug.Log("Item moved to external inventory via right click.");
                }
                else
                {
                    Debug.Log("Right click move failed (slot may be empty or external inventory full).");
                }
            }
        }
    }
}
