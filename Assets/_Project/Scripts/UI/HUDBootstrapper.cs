using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Automatically creates the full HUD Canvas at runtime.
/// Attach to any GameObject in the scene — no manual Canvas setup needed!
/// </summary>
public class HUDBootstrapper : MonoBehaviour
{
    private Canvas canvas;
    private HUDManager hudManager;

    // UI references
    private TextMeshProUGUI coinsText;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI xpText;
    private Slider xpSlider;
    private GameObject levelUpPanel;
    private TextMeshProUGUI levelUpText;
    private GameObject notificationPanel;
    private TextMeshProUGUI notificationText;
    private TextMeshProUGUI toolText;

    private void Awake()
    {
        BuildCanvas();
        BuildHUD();
        WireUpHUDManager();
    }

    private void BuildCanvas()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("HUD Canvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Add HUDManager
        hudManager = canvasGO.AddComponent<HUDManager>();
    }

    private void BuildHUD()
    {
        // --- COINS (top left) ---
        coinsText = CreateText("CoinsText", canvas.transform,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(160f, -40f), new Vector2(280f, 50f),
            "🪙 100", 28, Color.white, TextAlignmentOptions.Left);

        // --- LEVEL (top right) ---
        levelText = CreateText("LevelText", canvas.transform,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-120f, -40f), new Vector2(180f, 50f),
            "Lv. 1", 28, Color.white, TextAlignmentOptions.Right);

        // --- XP SLIDER (top right, below level) ---
        GameObject sliderGO = new GameObject("XPSlider");
        sliderGO.transform.SetParent(canvas.transform, false);
        xpSlider = sliderGO.AddComponent<Slider>();
        xpSlider.minValue = 0f;
        xpSlider.maxValue = 1f;
        xpSlider.value = 0f;

        var sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(1f, 1f);
        sliderRect.anchorMax = new Vector2(1f, 1f);
        sliderRect.anchoredPosition = new Vector2(-120f, -80f);
        sliderRect.sizeDelta = new Vector2(220f, 16f);

        // Slider background
        GameObject bg = CreatePanel("Background", sliderGO.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.2f, 0.2f, 0.2f, 0.8f));
        xpSlider.targetGraphic = bg.GetComponent<Image>();

        // Slider fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = CreatePanel("Fill", fillArea.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.4f, 0.8f, 0.4f, 1f));
        xpSlider.fillRect = fill.GetComponent<RectTransform>();

        // XP text below slider
        xpText = CreateText("XPText", canvas.transform,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-120f, -105f), new Vector2(220f, 30f),
            "0 XP to next level", 16, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.Right);

        // --- TOOL INDICATOR (bottom centre) ---
        CreatePanel("ToolBG", canvas.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 50f), new Vector2(280f, 44f),
            new Color(0f, 0f, 0f, 0.55f));

        toolText = CreateText("ToolText", canvas.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 50f), new Vector2(280f, 44f),
            "🌱 No crop selected (press B)", 18, Color.white, TextAlignmentOptions.Center);

        // --- NOTIFICATION PANEL (bottom centre, above tool) ---
        notificationPanel = CreatePanel("NotificationPanel", canvas.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 110f), new Vector2(500f, 50f),
            new Color(0.1f, 0.1f, 0.1f, 0.85f));

        notificationText = CreateText("NotificationText", notificationPanel.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "", 20, Color.white, TextAlignmentOptions.Center);
        notificationPanel.SetActive(false);

        // --- LEVEL UP PANEL (centre screen) ---
        levelUpPanel = CreatePanel("LevelUpPanel", canvas.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400f, 120f),
            new Color(1f, 0.85f, 0.1f, 0.95f));

        levelUpText = CreateText("LevelUpText", levelUpPanel.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "⭐ Level Up!", 42, new Color(0.1f, 0.05f, 0f), TextAlignmentOptions.Center);
        levelUpPanel.SetActive(false);

        // --- CONTROLS HINT (top centre, small) ---
        CreateText("ControlsHint", canvas.transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -30f), new Vector2(600f, 40f),
            "LClick: Till/Plant/Harvest  |  RClick: Water  |  B: Shop  |  Tab: Inventory",
            16, new Color(1f, 1f, 1f, 0.6f), TextAlignmentOptions.Center);
    }

    private void WireUpHUDManager()
    {
        // Use reflection-free approach — set public fields via a setup method
        hudManager.Setup(coinsText, levelText, xpSlider, xpText,
                         levelUpPanel, levelUpText, notificationPanel, notificationText, toolText);
    }

    // --- Helpers ---

    private TextMeshProUGUI CreateText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        string text, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = FontStyles.Bold;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        return tmp;
    }

    private GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        return go;
    }
}
