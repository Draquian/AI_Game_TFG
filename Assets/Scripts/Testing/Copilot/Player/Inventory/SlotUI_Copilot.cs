using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI_Copilot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int slotIndex; // Index corresponding to the inventory slot.
    public InventoryUI_Copilot inventoryUI; // Set by InventoryUI when instantiating the slot.

    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTrans;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();  // Ensure you have a Canvas in the scene.
        rectTrans = GetComponent<RectTransform>();  
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// On begin drag, store the current parent and change the parent to the Canvas for clear visibility.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(canvas.transform, false);
        canvasGroup.blocksRaycasts = false;  // Allow underlying objects to receive drop events.
    }

    /// <summary>
    /// Move the slot with the pointer.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        rectTrans.position = eventData.position;
    }

    /// <summary>
    /// When the drag ends, revert back to its original parent.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent, false);
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// When another draggable slot is dropped on this slot, swap their items.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // Get the slot that is being dragged.
        SlotUI_Copilot draggedSlotUI = eventData.pointerDrag.GetComponent<SlotUI_Copilot>();
        if (draggedSlotUI != null && draggedSlotUI != this)
        {
            inventoryUI.SwapSlots(draggedSlotUI.slotIndex, this.slotIndex);
            // Refresh the UI to reflect the swap.
            inventoryUI.RefreshUI();
        }
    }
}
