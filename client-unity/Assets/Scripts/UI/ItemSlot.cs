using UnityEngine;
using UnityEngine.EventSystems;

// This class represents an inventory slot that can accept draggable items.
public class ItemSlot : MonoBehaviour, IDropHandler
{
    // Called when a draggable object is dropped on this slot
    public void OnDrop(PointerEventData eventData)
    {
        // Try to get the DraggableItem component from the object being dragged
        DraggableItem item = eventData.pointerDrag?.GetComponent<DraggableItem>();

        // If the dragged object has a DraggableItem component, accept it
        if (item != null)
        {
            // Set the dragged item's parent to this slot, making it a child of the slot
            item.transform.SetParent(transform);

            // Reset the dragged item's position to align perfectly inside the slot
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
