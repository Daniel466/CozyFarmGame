using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shop UI for buying seeds. Toggle with B key.
/// Built at runtime by HUDBootstrapper. Shows all crops with unlock levels.
/// </summary>
public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private CropDatabase cropDatabase;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        // Refresh while open if coins or level change (e.g. after buying a seed)
        StartCoroutine(SubscribeNextFrame());
    }

    private System.Collections.IEnumerator SubscribeNextFrame()
    {
        yield return null;
        if (GameManager.Instance?.Economy != null)
            GameManager.Instance.Economy.OnCoinsChanged.AddListener(_ => { if (isOpen) RefreshShop(); });
        if (GameManager.Instance?.Progression != null)
            GameManager.Instance.Progression.OnLevelUp.AddListener(_ => { if (isOpen) RefreshShop(); });
    }

    public void Setup(GameObject panel, Transform grid, CropDatabase db)
    {
        shopPanel = panel;
        itemGrid = grid;
        cropDatabase = db;
        shopPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            ToggleShop();
    }

    public void ToggleShop()
    {
        if (shopPanel == null) { Debug.LogWarning("[ShopUI] Panel not assigned!"); return; }
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        if (isOpen)
        {
            // Bring to front so buttons are clickable
            shopPanel.transform.SetAsLastSibling();
            RefreshShop();
        }
    }

    public void CloseShop()
    {
        isOpen = false;
        shopPanel?.SetActive(false);
    }

    private void RefreshShop()
    {
        if (itemGrid == null || cropDatabase == null) return;

        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        int playerLevel = GameManager.Instance.Progression.CurrentLevel;
        int coins = GameManager.Instance.Economy.Coins;
        List<CropData> allCrops = cropDatabase.GetAllCrops();

        foreach (var crop in allCrops)
        {
            bool unlocked = crop.UnlockLevel <= playerLevel;
            bool canAfford = coins >= crop.SeedCost;

            // Create row
            GameObject row = new GameObject($"ShopItem_{crop.CropId}");
            row.transform.SetParent(itemGrid, false);

            var rowImg = row.AddComponent<Image>();
            rowImg.color = unlocked ? new Color(0.22f, 0.18f, 0.12f) : new Color(0.15f, 0.13f, 0.1f);

            var rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 65f);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = true;

            // Colour swatch
            GameObject swatch = new GameObject("Swatch");
            swatch.transform.SetParent(row.transform, false);
            var swatchImg = swatch.AddComponent<Image>();
            swatchImg.color = unlocked ? GetCropColour(crop.CropId) : new Color(0.3f, 0.3f, 0.3f);
            var swatchLE = swatch.AddComponent<LayoutElement>();
            swatchLE.preferredWidth = 36f;
            swatchLE.preferredHeight = 36f;
            swatchLE.flexibleWidth = 0f;

            // Info column
            GameObject info = new GameObject("Info");
            info.transform.SetParent(row.transform, false);
            var infoVLG = info.AddComponent<VerticalLayoutGroup>();
            infoVLG.childForceExpandWidth = true;
            infoVLG.spacing = 2f;
            var infoLE = info.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1f;

            // Crop name
            AddText(info.transform, crop.CropName, 18f,
                unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f), FontStyles.Bold);

            // Grow time & cost
            string details = unlocked
                ? $"{crop.GrowTimeSeconds / 60f:0.0} min | Sells for {crop.SellValue} coins"
                : $"Unlocks at Level {crop.UnlockLevel}";
            AddText(info.transform, details, 14f, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal);

            // Buy button
            GameObject btnGO = new GameObject("BuyBtn");
            btnGO.transform.SetParent(row.transform, false);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = unlocked && canAfford ? new Color(0.3f, 0.65f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.interactable = unlocked && canAfford;
            var btnLE = btnGO.AddComponent<LayoutElement>();
            btnLE.preferredWidth = 90f;
            btnLE.flexibleWidth = 0f;

            var btnText = AddText(btnGO.transform, unlocked ? $"{crop.SeedCost} coins" : "[Locked]",
                16f, Color.white, FontStyles.Bold);
            btnText.alignment = TextAlignmentOptions.Center;

            if (unlocked && canAfford)
            {
                CropData cropRef = crop;
                btn.onClick.AddListener(() => SelectCrop(cropRef));
            }

            // Dim locked
            var cg = row.AddComponent<CanvasGroup>();
            cg.alpha = unlocked ? 1f : 0.5f;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemGrid as RectTransform);
    }

    private void SelectCrop(CropData crop)
    {
        var interaction = FindFirstObjectByType<PlayerInteraction>();
        if (interaction != null)
        {
            interaction.SetSelectedCrop(crop);
            HUDManager.Instance?.ShowNotification($"{crop.CropName} selected!");
            HUDManager.Instance?.UpdateToolIndicator($"{crop.CropName}");
        }
        CloseShop();
    }

    private TextMeshProUGUI AddText(Transform parent, string text, float size, Color color, FontStyles style)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Left;
        return tmp;
    }

    private static Color GetCropColour(string id) => id switch
    {
        "carrot"     => new Color(1.0f, 0.55f, 0.1f),
        "sunflower"  => new Color(1.0f, 0.85f, 0.1f),
        "tomato"     => new Color(0.9f, 0.2f,  0.1f),
        "potato"     => new Color(0.7f, 0.55f, 0.2f),
        "strawberry" => new Color(0.9f, 0.15f, 0.25f),
        "corn"       => new Color(1.0f, 0.9f,  0.2f),
        "pumpkin"    => new Color(0.9f, 0.45f, 0.05f),
        "grapes"     => new Color(0.5f, 0.1f,  0.7f),
        "chilli"     => new Color(0.9f, 0.1f,  0.05f),
        "lavender"   => new Color(0.7f, 0.5f,  0.9f),
        _            => Color.green
    };
}
