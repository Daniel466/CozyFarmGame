using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings panel — audio volume sliders, resolution hints.
/// Works both on the main menu and in-game (pause menu).
/// Can be added to any Canvas at runtime.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    private GameObject settingsPanel;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Slider ambienceSlider;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        BuildSettingsPanel();
    }

    private void BuildSettingsPanel()
    {
        // Find parent canvas
        var canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform;

        // Full screen overlay
        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(parent, false);

        var bg = settingsPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);
        var bgRect = settingsPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Settings window
        GameObject window = new GameObject("SettingsWindow");
        window.transform.SetParent(settingsPanel.transform, false);
        var windowImg = window.AddComponent<Image>();
        windowImg.color = new Color(0.12f, 0.1f, 0.07f, 0.97f);
        var windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(500f, 420f);

        var vlg = window.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.spacing = 20f;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        // Title
        AddLabel(window.transform, "Settings", 32f, new Color(1f, 0.9f, 0.6f), FontStyles.Bold);

        // Music volume
        AddLabel(window.transform, "Music Volume", 20f, Color.white, FontStyles.Normal);
        musicSlider = AddSlider(window.transform, 0.4f, (val) =>
            AudioManager.Instance?.SetMusicVolume(val));

        // SFX volume
        AddLabel(window.transform, "Sound Effects", 20f, Color.white, FontStyles.Normal);
        sfxSlider = AddSlider(window.transform, 0.8f, (val) =>
            AudioManager.Instance?.SetSFXVolume(val));

        // Ambience volume
        AddLabel(window.transform, "Ambience", 20f, Color.white, FontStyles.Normal);
        ambienceSlider = AddSlider(window.transform, 0.25f, (val) =>
            AmbienceManager.Instance?.SetVolume(val));

        // Growth speed (debug)
        AddLabel(window.transform, "Crop Growth Speed (1=normal, 60=fast)", 16f,
            new Color(0.6f, 0.6f, 0.6f), FontStyles.Italic);

        // Close button
        AddButton(window.transform, "Close", new Color(0.3f, 0.65f, 0.3f), Hide);

        settingsPanel.SetActive(false);
    }

    public void Show() => settingsPanel?.SetActive(true);
    public void Hide() => settingsPanel?.SetActive(false);
    public void Toggle()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void AddLabel(Transform parent, string text, float size, Color color, FontStyles style)
    {
        GameObject go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = size + 8f;
    }

    private Slider AddSlider(Transform parent, float defaultValue,
        UnityEngine.Events.UnityAction<float> onChange)
    {
        GameObject go = new GameObject("Slider");
        go.transform.SetParent(parent, false);
        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = defaultValue;

        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 30f);

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.25f);
        bgRect.anchorMax = new Vector2(1f, 0.75f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        slider.targetGraphic = bgImg;

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.75f, 0.4f);
        slider.fillRect = fill.GetComponent<RectTransform>();

        slider.onValueChanged.AddListener(onChange);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 30f;

        return slider;
    }

    private void AddButton(Transform parent, string label, Color color,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Button");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 55f;

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
