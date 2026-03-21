using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's inventory in a scrollable list.
/// Toggle with Tab key. Includes a Sell All button.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    private GameObject inventoryPanel;
    private Transform itemGrid;
    private TextMeshProUGUI slotsText;
    private Button sellAllButton;
    private TextMeshProUGUI sellAllText;

    private bool isOpen = false;

    /// <summary>Called by HUDBootstrapper to wire up all references at runtime.</summary>
    public void Setup(GameObject panel, Transform grid, TextMeshProUGUI slots,
                      Button sellBtn, TextMeshProUGUI sellBtnText)
    {
        inventoryPanel = panel;
        itemGrid = grid;
        slotsText = slots;
        sellAllButton = sellBtn;
        sellAllText = sellBtnText;

        sellAllButton?.onClick.AddListener(SellAll);

        // Delay subscription by one frame to ensure GameManager is ready
        StartCoroutine(SubscribeNextFrame());
    }

    private System.Collections.IEnumerator SubscribeNextFrame()
    {
        yield return null;
        if (GameManager.Instance?.Inventory != null)
            GameManager.Instance.Inventory.OnInventoryChanged.AddListener(RefreshUI);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel?.SetActive(isOpen);
        if (isOpen) RefreshUI();
    }

    private void RefreshUI()
    {
        if (!isOpen || itemGrid == null) return;

        // Clear existing item rows
        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        var items = GameManager.Instance.Inventory.GetAllItems();
        int totalValue = 0;

        foreach (var kvp in items)
        {
            var item = kvp.Value;
            totalValue += item.crop.SellValue * item.quantity;

            // Create a row for each item
            GameObject row = new GameObject($"Item_{item.crop.CropId}");
            row.transform.SetParent(itemGrid, false);

            var rowImg = row.AddComponent<Image>();
            rowImg.color = new Color(0.25f, 0.2f, 0.13f, 0.9f);

            var rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 55f);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 15f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = true;

            // Crop name
            CreateRowText(row.transform, item.crop.CropName, 22, Color.white, TextAnchor.MiddleLeft, true);

            // Quantity
            CreateRowText(row.transform, $"x{item.quantity}", 20,
                new Color(0.8f, 0.9f, 0.8f), TextAnchor.MiddleCenter, false);

            // Value
            CreateRowText(row.transform, $"{item.crop.SellValue * item.quantity} 🪙", 20,
                new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleRight, false);
        }

        // Empty state
        if (items.Count == 0)
        {
            CreateRowText(itemGrid, "Nothing in inventory — go harvest some crops! 🌱",
                20, new Color(0.6f, 0.6f, 0.6f), TextAnchor.MiddleCenter, false);
        }

        // Update sell button
        if (sellAllText) sellAllText.text = $"💰 Sell All ({totalValue} 🪙)";

        // Update slots
        var inv = GameManager.Instance.Inventory;
        if (slotsText) slotsText.text = $"Slots: {inv.UsedSlots}/{inv.MaxSlots}";
    }

    private void CreateRowText(Transform parent, string text, float size,
                                Color color, TextAnchor anchor, bool expand)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;

        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = expand ? 1f : 0f;
        le.minWidth = expand ? 0f : 100f;
    }

    private void SellAll()
    {
        int earned = GameManager.Instance.Inventory.SellAll();
        if (earned > 0)
            HUDManager.Instance?.ShowNotification($"Sold everything for {earned} 🪙!");
        else
            HUDManager.Instance?.ShowNotification("Nothing to sell!");
        RefreshUI();
    }
}
