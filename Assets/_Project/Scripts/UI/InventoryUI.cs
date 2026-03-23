using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's inventory in a scrollable list.
/// Toggle with Tab key. Each row shows a colour swatch, crop name,
/// quantity, sell value, and a per-item sell button.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private TextMeshProUGUI slotsText;
    [SerializeField] private Button sellAllButton;
    [SerializeField] private TextMeshProUGUI sellAllText;

    private bool isOpen = false;

    // Crop accent colours — matches CropGrowthVisual palette
    private static readonly Dictionary<string, Color> CropColors = new Dictionary<string, Color>
    {
        { "carrot",     new Color(1.0f, 0.55f, 0.1f)  },
        { "sunflower",  new Color(1.0f, 0.85f, 0.1f)  },
        { "tomato",     new Color(0.9f, 0.2f,  0.1f)  },
        { "potato",     new Color(0.7f, 0.55f, 0.2f)  },
        { "strawberry", new Color(0.9f, 0.15f, 0.25f) },
        { "corn",       new Color(1.0f, 0.9f,  0.2f)  },
        { "pumpkin",    new Color(0.9f, 0.45f, 0.05f) },
        { "grapes",     new Color(0.5f, 0.1f,  0.7f)  },
        { "chilli",     new Color(0.9f, 0.1f,  0.05f) },
        { "lavender",   new Color(0.7f, 0.5f,  0.9f)  },
    };

    private void Start()
    {
        sellAllButton?.onClick.AddListener(SellAll);
        StartCoroutine(SubscribeNextFrame());
    }

    /// <summary>Legacy runtime setup — kept for compatibility.</summary>
    public void Setup(GameObject panel, Transform grid, TextMeshProUGUI slots,
                      Button sellBtn, TextMeshProUGUI sellBtnText)
    {
        inventoryPanel = panel;
        itemGrid = grid;
        slotsText = slots;
        sellAllButton = sellBtn;
        sellAllText = sellBtnText;
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

        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        var items = GameManager.Instance.Inventory.GetAllItems();
        int totalValue = 0;

        foreach (var kvp in items)
        {
            var item = kvp.Value;
            totalValue += item.crop.SellValue * item.quantity;

            // Row background
            GameObject row = new GameObject($"Item_{item.crop.CropId}");
            row.transform.SetParent(itemGrid, false);
            var rowImg = row.AddComponent<Image>();
            rowImg.color = new Color(0.18f, 0.14f, 0.09f, 0.95f);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 56f);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 10, 10);
            hlg.spacing = 12f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;

            // Icon / colour swatch
            var swatch = new GameObject("Swatch");
            swatch.transform.SetParent(row.transform, false);
            var swatchImg = swatch.AddComponent<Image>();
            if (item.crop.Icon != null)
            {
                swatchImg.sprite = item.crop.Icon;
                swatchImg.color = Color.white;
                swatchImg.preserveAspect = true;
            }
            else
            {
                Color cropCol = CropColors.TryGetValue(item.crop.CropId, out Color c) ? c : new Color(0.5f, 0.8f, 0.5f);
                swatchImg.color = cropCol;
            }
            var swatchLE = swatch.AddComponent<LayoutElement>();
            swatchLE.minWidth = 48f;
            swatchLE.preferredWidth = 48f;
            swatchLE.minHeight = 48f;
            swatchLE.preferredHeight = 48f;

            // Crop name
            MakeText(row.transform, item.crop.CropName, 20f, Color.white, TextAlignmentOptions.Left,
                flexibleWidth: 1f);

            // Quantity  "x 6"
            MakeText(row.transform, $"x{item.quantity}", 18f,
                new Color(0.75f, 0.9f, 0.75f), TextAlignmentOptions.Center,
                minWidth: 48f);

            // Per-unit sell value  "5g ea."
            MakeText(row.transform, $"{item.crop.SellValue}g ea.", 17f,
                new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Right,
                minWidth: 72f);

            // Per-item sell button
            string cropId = item.crop.CropId;   // capture for closure
            int rowValue = item.crop.SellValue * item.quantity;
            MakeSellButton(row.transform, $"Sell\n{rowValue}g",
                () =>
                {
                    int earned = GameManager.Instance.Inventory.SellItem(cropId);
                    if (earned > 0)
                        HUDManager.Instance?.ShowNotification($"Sold {item.crop.CropName} for {earned} coins!");
                });
        }

        // Empty state
        if (items.Count == 0)
        {
            var empty = new GameObject("EmptyText");
            empty.transform.SetParent(itemGrid, false);
            var tmp = empty.AddComponent<TextMeshProUGUI>();
            tmp.text = "Nothing here yet - go harvest some crops!";
            tmp.fontSize = 18f;
            tmp.color = new Color(0.55f, 0.55f, 0.55f);
            tmp.alignment = TextAlignmentOptions.Center;
            var le = empty.AddComponent<LayoutElement>();
            le.preferredHeight = 60f;
        }

        // Sell All button text
        if (sellAllText) sellAllText.text = totalValue > 0
            ? $"Sell All  ({totalValue} coins)"
            : "Sell All";

        // Slots
        var inv = GameManager.Instance.Inventory;
        if (slotsText) slotsText.text = $"{inv.UsedSlots}/{inv.MaxSlots} slots";
    }

    // ── Row helpers ────────────────────────────────────────────────────────────

    private static void MakeText(Transform parent, string text, float size, Color color,
        TextAlignmentOptions align, float flexibleWidth = 0f, float minWidth = 0f)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = FontStyles.Bold;
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = flexibleWidth;
        if (minWidth > 0f) le.minWidth = minWidth;
    }

    private static void MakeSellButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("SellBtn");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.22f, 0.58f, 0.22f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.75f, 0.3f);
        colors.pressedColor     = new Color(0.15f, 0.45f, 0.15f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);
        var le = go.AddComponent<LayoutElement>();
        le.minWidth      = 68f;
        le.preferredWidth = 68f;

        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 13f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        var rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(2f, 2f);
        rect.offsetMax = new Vector2(-2f, -2f);
    }

    // ── Actions ────────────────────────────────────────────────────────────────

    private void SellAll()
    {
        int earned = GameManager.Instance.Inventory.SellAll();
        HUDManager.Instance?.ShowNotification(earned > 0
            ? $"Sold everything for {earned} coins!"
            : "Nothing to sell!");
        RefreshUI();
    }
}
