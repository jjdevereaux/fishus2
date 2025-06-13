using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class InventoryWindowManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int RowCount = 2;
    public int ColumnCount = 2;
    public Vector2 CellSize = new Vector2(100, 100);
    public Vector2 Spacing = new Vector2(10, 10);
    public GameObject PanelPrefab;

    private RectTransform rectTransform;

    // List to keep track of all draggable items in the inventory
    public List<DraggableItem> draggableItems = new List<DraggableItem>();

#if UNITY_EDITOR
    [ContextMenu("Arrange Panels")]
    void EditorArrangePanels()
    {
        if (!Application.isPlaying)
            UnityEditor.EditorApplication.delayCall += ArrangePanels;
    }
#endif

    void OnEnable()
    {
        ArrangePanels();
    }

    public void ArrangePanels()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null || RowCount <= 0 || ColumnCount <= 0 || PanelPrefab == null)
            return;

        // Clear existing children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        int panelCount = RowCount * ColumnCount;

        for (int i = 0; i < panelCount; i++)
        {
            GameObject panel = Instantiate(PanelPrefab, transform);
            RectTransform child = panel.GetComponent<RectTransform>();

            if (child == null) continue;

            int row = i / ColumnCount;
            int column = i % ColumnCount;

            Vector2 startOffset = new Vector2(
                -((ColumnCount - 1) * (CellSize.x + Spacing.x)) / 2f,
                ((RowCount - 1) * (CellSize.y + Spacing.y)) / 2f
            );

            Vector2 position = new Vector2(
                startOffset.x + column * (CellSize.x + Spacing.x),
                startOffset.y - row * (CellSize.y + Spacing.y)
            );

            child.anchoredPosition = position;
            child.sizeDelta = CellSize;
        }
    }

    // Called by DraggableItem to register itself with the manager
    public void RegisterItem(DraggableItem item)
    {
        if (!draggableItems.Contains(item))
        {
            draggableItems.Add(item);
        }
    }

    // Called by DraggableItem to deregister itself (e.g., on destroy)
    public void DeregisterItem(DraggableItem item)
    {
        if (draggableItems.Contains(item))
        {
            draggableItems.Remove(item);
        }
    }

    // Example method to validate a move (can be customized further)
    public bool ValidateMove(DraggableItem item, ItemSlot targetSlot)
    {
        // Add game-specific validation logic here
        return true;
    }
}
