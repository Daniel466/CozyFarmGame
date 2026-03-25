using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Manages the main HUD display: coins, XP bar, level, and tool indicator.
/// Attach to a Canvas GameObject in your scene.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Coins")]
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Level & XP")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI xpText;

    [Header("Level Up Panel")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private float levelUpDisplayTime = 3f;

    [Header("Controls Overlay")]
    [SerializeField] private GameObject controlsPanel;

    [Header("Selected Crop Panel")]
    [SerializeField] private GameObject selectedCropPanel;
    [SerializeField] private Image selectedCropSwatch;
    [SerializeField] private TextMeshProUGUI selectedCropNameText;
    [SerializeField] private TextMeshProUGUI selectedCropStatsText;

    [Header("Tile Info Panel")]
    [SerializeField] private GameObject tileInfoPanel;
    [SerializeField] private Image tileInfoSwatch;
    [SerializeField] private TextMeshProUGUI tileInfoCropName;
    [SerializeField] private TextMeshProUGUI tileInfoStageText;
    [SerializeField] private TextMeshProUGUI tileInfoTimeText;
    [SerializeField] private TextMeshProUGUI tileInfoWaterText;
    [SerializeField] private RectTransform tileInfoProgressFill;
    [SerializeField] private TextMeshProUGUI tileInfoActionHint;

    private RectTransform tileInfoPanelRect;
    private bool tileInfoVisible;
    private const float TileInfoOnScreenX  = -155f;
    private const float TileInfoOffScreenX =  160f;

    [Header("Context Hint")]
    [SerializeField] private TextMeshProUGUI contextHintText;

    [Header("Tool Indicator")]
    [SerializeField] private TextMeshProUGUI toolText;

    [Header("Notification")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;

    private CropData currentSelectedCrop;

    private static readonly Dictionary<string, Color> CropColors = new Dictionary<string, Color>
    {
        { "carrot",     new Color(1.0f, 0.55f, 0.1f)  },
        { "sunflower",  new Color(1.0f, 0.85f, 0.1f)  },
        { "tomato",     new Color(0.9f, 0.2f,  0.1f)  },
        { "potato",     new Color(0.7f, 0.55f, 0.2f)  },
        { "strawberry", new Color(0.9f, 0.15f, 0.25f) },
        { "corn",       new Color(1.0f, 0.9f,  0.2f)  },
        { "pumpkin",    new Color(0.9f, 0.45f, 0.05f) },
        { "watermelon", new Color(0.25f, 0.75f, 0.2f)  },
        { "leek",       new Color(0.4f,  0.75f, 0.3f)  },
        { "wheat",      new Color(0.85f, 0.72f, 0.15f) },
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Hide panels first (safe before GameManager is ready)
        if (levelUpPanel) levelUpPanel.SetActive(false);
        if (notificationPanel) notificationPanel.SetActive(false);
        if (tileInfoPanel)
        {
            tileInfoPanel.SetActive(false);
            tileInfoPanelRect = tileInfoPanel.GetComponent<RectTransform>();
        }
        if (controlsPanel) controlsPanel.SetActive(true); // visible by default
        SetContextHint("B: Shop  |  Tab: Inventory  |  G: Build");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && controlsPanel != null)
            controlsPanel.SetActive(!controlsPanel.activeSelf);
    }

    private void OnEnable()
    {
        // Delay subscription by one frame to ensure all systems are initialised
        StartCoroutine(SubscribeNextFrame());
    }

    private System.Collections.IEnumerator SubscribeNextFrame()
    {
        yield return null; // Wait one frame

        if (GameManager.Instance == null) yield break;

        // Subscribe to events
        GameManager.Instance.Economy.OnCoinsChanged.AddListener(UpdateCoins);
        GameManager.Instance.Progression.OnXPChanged.AddListener(UpdateXP);
        GameManager.Instance.Progression.OnLevelUp.AddListener(ShowLevelUp);

        // Initial update
        UpdateCoins(GameManager.Instance.Economy.Coins);
        UpdateXP(GameManager.Instance.Progression.CurrentXP,
                 GameManager.Instance.Progression.CurrentLevel);
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.Economy.OnCoinsChanged.RemoveListener(UpdateCoins);
        GameManager.Instance.Progression.OnXPChanged.RemoveListener(UpdateXP);
        GameManager.Instance.Progression.OnLevelUp.RemoveListener(ShowLevelUp);
    }

    /// <summary>
    /// Called by HUDBootstrapper to wire up all UI references at runtime.
    /// </summary>
    public void Setup(
        TextMeshProUGUI coins,
        TextMeshProUGUI level,
        Slider xp,
        TextMeshProUGUI xpLabel,
        GameObject levelUp,
        TextMeshProUGUI levelUpLabel,
        GameObject notification,
        TextMeshProUGUI notificationLabel,
        TextMeshProUGUI tool)
    {
        coinsText = coins;
        levelText = level;
        xpSlider = xp;
        xpText = xpLabel;
        levelUpPanel = levelUp;
        levelUpText = levelUpLabel;
        notificationPanel = notification;
        notificationText = notificationLabel;
        toolText = tool;
    }

    private void UpdateCoins(int coins)
    {
        if (coinsText) coinsText.text = $"{coins:N0} coins";
    }

    private void UpdateXP(int xp, int level)
    {
        if (levelText) levelText.text = $"Lv. {level}";

        float progress = GameManager.Instance.Progression.LevelProgress();
        if (xpSlider) xpSlider.value = progress;

        int xpToNext = GameManager.Instance.Progression.XPForNextLevel();
        if (xpText)
        {
            if (level >= GameManager.Instance.Progression.MaxLevel)
                xpText.text = "MAX";
            else
                xpText.text = $"{xpToNext} XP to next level";
        }
    }

    private void ShowLevelUp(int newLevel)
    {
        if (levelUpPanel == null) return;
        levelUpPanel.SetActive(true);
        if (levelUpText) levelUpText.text = $"Level Up!\nLevel {newLevel}";
        Invoke(nameof(HideLevelUp), levelUpDisplayTime);
    }

    private void HideLevelUp()
    {
        if (levelUpPanel) levelUpPanel.SetActive(false);
    }

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

    public void UpdateToolIndicator(string toolName)
    {
        if (toolText) toolText.text = toolName;
    }

    public void SetContextHint(string hint)
    {
        if (contextHintText) contextHintText.text = hint;
    }

    public void ShowSelectedCrop(CropData crop)
    {
        currentSelectedCrop = crop;
        if (selectedCropPanel == null) return;

        if (crop == null)
        {
            selectedCropPanel.SetActive(false);
            StopCoroutine("TickSelectedCropStats");
            return;
        }

        selectedCropPanel.SetActive(true);

        if (selectedCropNameText)
            selectedCropNameText.text = crop.CropName;

        if (selectedCropSwatch)
            selectedCropSwatch.color = CropColors.TryGetValue(crop.CropId, out Color c)
                ? c : new Color(0.5f, 0.8f, 0.5f);

        UpdateSelectedCropStats();
        StopCoroutine("TickSelectedCropStats");
        StartCoroutine("TickSelectedCropStats");
    }

    private System.Collections.IEnumerator TickSelectedCropStats()
    {
        while (currentSelectedCrop != null)
        {
            UpdateSelectedCropStats();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateSelectedCropStats()
    {
        if (currentSelectedCrop == null || selectedCropStatsText == null) return;
        var fm = FarmingManager.Instance;
        if (fm == null) { selectedCropStatsText.text = ""; return; }

        int   count = fm.GetPlantedCount(currentSelectedCrop.CropId);
        float secs  = fm.GetNearestRemainingSeconds(currentSelectedCrop.CropId);

        if (count == 0)
        {
            selectedCropStatsText.text = "None planted";
            return;
        }

        string countStr = count == 1 ? "1 planted" : $"{count} planted";
        string timerStr = secs <= 0f ? ", Ready!" : $", {FormatTime(secs)}";
        selectedCropStatsText.text = countStr + timerStr;
    }

    private static string FormatTime(float seconds)
    {
        int m = (int)seconds / 60;
        int s = (int)seconds % 60;
        return m > 0 ? $"{m}m {s:D2}s" : $"{s}s";
    }

    public void ShowTileInfo(FarmTile tile)
    {
        if (tileInfoPanel == null || tile == null) { HideTileInfo(); return; }

        // Slide in from right if not already visible
        if (!tileInfoVisible)
        {
            tileInfoVisible = true;
            tileInfoPanel.SetActive(true);
            if (tileInfoPanelRect != null)
            {
                tileInfoPanelRect.DOKill();
                tileInfoPanelRect.anchoredPosition = new Vector2(TileInfoOffScreenX, tileInfoPanelRect.anchoredPosition.y);
                tileInfoPanelRect.DOAnchorPosX(TileInfoOnScreenX, 0.2f).SetEase(Ease.OutCubic);
            }
        }

        bool planted = tile.IsPlanted;

        // Swatch — only visible when a crop is planted
        if (tileInfoSwatch)
        {
            tileInfoSwatch.gameObject.SetActive(planted);
            if (planted)
                tileInfoSwatch.color = CropColors.TryGetValue(tile.PlantedCrop.CropId, out Color c)
                    ? c : new Color(0.5f, 0.8f, 0.5f);
        }

        if (tileInfoCropName)
            tileInfoCropName.text = planted ? tile.PlantedCrop.CropName : "";

        if (tileInfoStageText)
        {
            if (!planted)
                tileInfoStageText.text = "";
            else if (tile.IsReadyToHarvest)
                tileInfoStageText.text = "Ready to Harvest!";
            else
            {
                string[] labels = { "Planted", "Sprouting", "Growing" };
                int stage = Mathf.Clamp(tile.GetGrowthStage(), 0, 2);
                tileInfoStageText.text = labels[stage];
            }
        }

        if (tileInfoTimeText)
        {
            if (!planted || tile.IsReadyToHarvest)
                tileInfoTimeText.text = "";
            else
            {
                float secs = tile.GetRemainingSeconds();
                tileInfoTimeText.text = secs > 0f ? $"{FormatTime(secs)} remaining" : "";
            }
        }

        if (tileInfoWaterText)
            tileInfoWaterText.text = "";

        // Progress bar
        if (tileInfoProgressFill != null)
        {
            float prog = !planted ? 0f : (tile.IsReadyToHarvest ? 1f : tile.GetGrowthProgress());
            tileInfoProgressFill.anchorMax = new Vector2(prog, 1f);
            var fillImg = tileInfoProgressFill.GetComponent<Image>();
            if (fillImg) fillImg.color = tile.IsReadyToHarvest
                ? new Color(1f, 0.75f, 0.1f)
                : new Color(0.35f, 0.75f, 0.35f);
        }

        // Action hint
        if (tileInfoActionHint)
        {
            if (tile.IsReadyToHarvest)
                tileInfoActionHint.text = "Left click - harvest";
            else if (planted)
                tileInfoActionHint.text = "";
            else
                tileInfoActionHint.text = "Left click - plant";
        }
    }

    public void HideTileInfo()
    {
        if (!tileInfoVisible) return;
        tileInfoVisible = false;
        if (tileInfoPanelRect == null) { if (tileInfoPanel) tileInfoPanel.SetActive(false); return; }
        tileInfoPanelRect.DOKill();
        tileInfoPanelRect.DOAnchorPosX(TileInfoOffScreenX, 0.15f).SetEase(Ease.InCubic)
            .OnComplete(() => { if (tileInfoPanel) tileInfoPanel.SetActive(false); });
    }
}
