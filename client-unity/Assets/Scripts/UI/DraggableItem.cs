using UnityEngine;
using UnityEngine.EventSystems;

// This class allows a UI item to be draggable and supports swap-on-click behavior
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Canvas canvas;                      // Reference to the root canvas
    private RectTransform rectTransform;        // Cached RectTransform of this item
    private CanvasGroup canvasGroup;            // Controls raycast blocking

    public Transform OriginalParent { get; private set; } // Public reference to original slot

    private InventoryWindowManager windowManager;
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        // Locate InventoryWindowManager by traversing the known hierarchy upward
        Transform current = transform;
        while (current != null && windowManager == null)
        {
            windowManager = current.GetComponent<InventoryWindowManager>();
            current = current.parent;
        }
    }

    void Start()
    {
        if (windowManager != null)
        {
            windowManager.RegisterItem(this);
        }
    }

    void OnDestroy()
    {
        if (windowManager != null)
        {
            windowManager.DeregisterItem(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        OriginalParent = transform.parent;
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        // Return if not over a valid item slot
        ItemSlot targetSlot = eventData.pointerEnter?.GetComponent<ItemSlot>();
        if (targetSlot == null || windowManager == null || !windowManager.ValidateMove(this, targetSlot))
        {
            transform.SetParent(OriginalParent);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging && eventData.button == PointerEventData.InputButton.Left)
        {
            // Start drag manually on click
            ExecuteEvents.Execute<IBeginDragHandler>(gameObject, eventData, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute<IDragHandler>(gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }
}
