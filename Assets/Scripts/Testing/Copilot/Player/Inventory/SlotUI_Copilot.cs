using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI_Copilot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int slotIndex;                      // This slot's index in the inventory.
    public InventoryUI_Copilot inventoryUI;            // Reference to the Inventory UI manager.

    private Transform originalParent;          // The original parent container of this UI element.
    private Vector2 originalAnchoredPos;       // Stored anchored position to snap back if needed.
    private Canvas canvas;                     // The canvas containing this UI, used for proper drag layering.
    private CanvasGroup canvasGroup;           // Used to control blocking of Raycasts during dragging.
    private RectTransform rectTrans;           // The RectTransform component.
    private bool droppedOnValidSlot = false;   // Flag: was a valid slot drop detected?

    private void Awake()
    {
        // Locate the canvas and RectTransform references.
        canvas = GetComponentInParent<Canvas>();
        rectTrans = GetComponent<RectTransform>();

        // Ensure there is a CanvasGroup component.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Reset our flag for a valid drop.
        droppedOnValidSlot = false;
        // Store the original parent (the slot container) and anchored position.
        originalParent = transform.parent;
        originalAnchoredPos = rectTrans.anchoredPosition;
        // Bring the dragged item to the top-level canvas (for correct rendering order).
        transform.SetParent(canvas.transform, false);
        // Allow underlying UI elements to receive raycast events.
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow the pointer as the item is dragged.
        rectTrans.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // If the item wasn't dropped on a valid slot, snap it back.
        if (!droppedOnValidSlot)
        {
            transform.SetParent(originalParent, false);
            rectTrans.anchoredPosition = originalAnchoredPos;
        }
        // Re-enable raycast blocking.
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Check if what was dropped is another SlotUI.
        SlotUI_Copilot draggedSlotUI = eventData.pointerDrag.GetComponent<SlotUI_Copilot>();
        if (draggedSlotUI != null && draggedSlotUI != this)
        {
            // Inform the InventoryUI to swap items based on the underlying inventory data.
            inventoryUI.SwapSlots(draggedSlotUI.slotIndex, this.slotIndex);
            // Mark that a valid drop has occurred.
            droppedOnValidSlot = true;
            // Refresh the UI to reassign slot positions (this will "snap" items into place).
            inventoryUI.RefreshUI();
        }
    }
}
