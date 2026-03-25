using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main HUD — coins, tool indicator, selected crop panel,
/// context hint, tile info panel, notification.
/// All fields wired in Inspector by Bezi or HUDBuilder.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Coins")]
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Tool Indicator")]
    [SerializeField] private TextMeshProUGUI toolText;

    [Header("Context Hint")]
    [SerializeField] private TextMeshProUGUI contextHintText;

    [Header("Selected Crop Panel")]
    [SerializeField] private GameObject      selectedCropPanel;
    [SerializeField] private Image           selectedCropSwatch;
    [SerializeField] private TextMeshProUGUI selectedCropNameText;
    [SerializeField] private TextMeshProUGUI selectedCropStatsText;

    [Header("Tile Info Panel")]
    [SerializeField] private GameObject      tileInfoPanel;
    [SerializeField] private TextMeshProUGUI tileInfoCropName;
    [SerializeField] private TextMeshProUGUI tileInfoStageText;
    [SerializeField] private TextMeshProUGUI tileInfoTimeText;
    [SerializeField] private RectTransform   tileInfoProgressFill;
    [SerializeField] private TextMeshProUGUI tileInfoActionHint;

    [Header("Notification")]
    [SerializeField] private GameObject      notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("Controls Overlay")]
    [SerializeField] private GameObject controlsPanel;

    // ── Crop colours ──────────────────────────────────────────────────────────

    private static readonly Dictionary<string, Color> CropColors = new()
    {
        { "wheat",  new Color(0.85f, 0.72f, 0.15f) },
        { "carrot", new Color(1.0f,  0.55f, 0.1f)  },
        { "corn",   new Color(1.0f,  0.9f,  0.2f)  },
    };

    // ── State ─────────────────────────────────────────────────────────────────

    private CropData _selectedCrop;

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (selectedCropPanel)  selectedCropPanel.SetActive(false);
        if (tileInfoPanel)      tileInfoPanel.SetActive(false);
        if (notificationPanel)  notificationPanel.SetActive(false);
        if (controlsPanel)      controlsPanel.SetActive(true);

        SetContextHint("1:Hoe  2:Seed  3:Harvest  4:Remove  5:Build");

        StartCoroutine(SubscribeNextFrame());
    }

    private IEnumerator SubscribeNextFrame()
    {
        yield return null;
        if (GameManager.Instance?.Economy == null) yield break;
        GameManager.Instance.Economy.OnCoinsChanged.AddListener(UpdateCoins);
        UpdateCoins(GameManager.Instance.Economy.Coins);
    }

    private void OnDisable()
    {
        if (GameManager.Instance?.Economy != null)
            GameManager.Instance.Economy.OnCoinsChanged.RemoveListener(UpdateCoins);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && controlsPanel != null)
            controlsPanel.SetActive(!controlsPanel.activeSelf);
    }

    // ── Coins ─────────────────────────────────────────────────────────────────

    private void UpdateCoins(int coins)
    {
        if (coinsText) coinsText.text = $"{coins} coins";
    }

    // ── Tool indicator ────────────────────────────────────────────────────────

    public void UpdateToolIndicator(string toolName)
    {
        if (toolText) toolText.text = toolName;
    }

    // ── Context hint ──────────────────────────────────────────────────────────

    public void SetContextHint(string hint)
    {
        if (contextHintText) contextHintText.text = hint;
    }

    // ── Selected crop panel ───────────────────────────────────────────────────

    public void ShowSelectedCrop(CropData crop)
    {
        _selectedCrop = crop;
        if (selectedCropPanel == null) return;

        if (crop == null) { selectedCropPanel.SetActive(false); return; }

        selectedCropPanel.SetActive(true);
        if (selectedCropNameText) selectedCropNameText.text = crop.CropName;
        if (selectedCropSwatch)
            selectedCropSwatch.color = CropColors.TryGetValue(crop.CropId, out Color c)
                ? c : new Color(0.5f, 0.8f, 0.5f);

        StopCoroutine("TickCropStats");
        StartCoroutine("TickCropStats");
    }

    private IEnumerator TickCropStats()
    {
        while (_selectedCrop != null)
        {
            UpdateSelectedCropStats();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateSelectedCropStats()
    {
        if (_selectedCrop == null || selectedCropStatsText == null) return;
        var fm = FarmingManager.Instance;
        if (fm == null) return;

        int   count = fm.GetPlantedCount(_selectedCrop.CropId);
        float secs  = fm.GetNearestRemainingSeconds(_selectedCrop.CropId);

        if (count == 0) { selectedCropStatsText.text = "None planted"; return; }

        string timer = secs <= 0f ? ", Ready!" : $", {FormatTime(secs)}";
        selectedCropStatsText.text = (count == 1 ? "1 planted" : $"{count} planted") + timer;
    }

    // ── Tile info panel ───────────────────────────────────────────────────────

    public void ShowTileInfo(FarmTile tile)
    {
        if (tileInfoPanel == null || tile == null) { HideTileInfo(); return; }

        tileInfoPanel.SetActive(true);
        bool planted = tile.IsPlanted;

        if (tileInfoCropName)
            tileInfoCropName.text = planted ? tile.PlantedCrop.CropName : "";

        if (tileInfoStageText)
        {
            if (!planted)                  tileInfoStageText.text = "Empty";
            else if (tile.IsReadyToHarvest) tileInfoStageText.text = "Ready to Harvest!";
            else
            {
                string[] labels = { "Planted", "Sprouting", "Growing" };
                tileInfoStageText.text = labels[Mathf.Clamp(tile.GetGrowthStage(), 0, 2)];
            }
        }

        if (tileInfoTimeText)
        {
            float secs = tile.GetRemainingSeconds();
            tileInfoTimeText.text = planted && !tile.IsReadyToHarvest && secs > 0f
                ? $"{FormatTime(secs)} remaining" : "";
        }

        if (tileInfoProgressFill != null)
        {
            float prog = !planted ? 0f : (tile.IsReadyToHarvest ? 1f : tile.GetGrowthProgress());
            tileInfoProgressFill.anchorMax = new Vector2(prog, 1f);
        }

        if (tileInfoActionHint)
            tileInfoActionHint.text = tile.IsReadyToHarvest ? "Click to harvest"
                : planted ? "" : "Click to plant";
    }

    public void HideTileInfo()
    {
        if (tileInfoPanel) tileInfoPanel.SetActive(false);
    }

    // ── Notification ──────────────────────────────────────────────────────────

    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationPanel == null) return;
        notificationPanel.SetActive(true);
        if (notificationText) notificationText.text = message;
        CancelInvoke(nameof(HideNotification));
        Invoke(nameof(HideNotification), duration);
    }

    private void HideNotification()
    {
        if (notificationPanel) notificationPanel.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string FormatTime(float seconds)
    {
        int m = (int)seconds / 60;
        int s = (int)seconds % 60;
        return m > 0 ? $"{m}m {s:D2}s" : $"{s}s";
    }
}
