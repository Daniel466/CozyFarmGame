using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Builds and manages the Main Menu at runtime.
/// Attach to an empty GameObject in your MainMenu scene.
/// Scenes needed: "MainMenu" (index 0), "Farm" (index 1)
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string farmSceneName = "Farm";

    private Canvas canvas;

    private void Awake()
    {
        EnsureEventSystem();
        BuildCanvas();
        BuildMainMenu();
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }
    }

    private void BuildCanvas()
    {
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
    }

    private void BuildMainMenu()
    {
        // Background panel (warm gradient feel)
        GameObject bg = CreatePanel("Background", canvas.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.18f, 0.28f, 0.15f)); // Deep cozy green

        // Title
        CreateText("GameTitle", canvas.transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -160f), new Vector2(800f, 120f),
            "Cozy Farm", 72, new Color(1f, 0.95f, 0.7f),
            TextAlignmentOptions.Center, FontStyles.Bold);

        // Subtitle
        CreateText("Subtitle", canvas.transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -270f), new Vector2(600f, 50f),
            "A cozy farming adventure", 28, new Color(0.8f, 0.9f, 0.7f),
            TextAlignmentOptions.Center, FontStyles.Italic);

        // Buttons container
        GameObject btnContainer = new GameObject("Buttons");
        btnContainer.transform.SetParent(canvas.transform, false);
        var containerRect = btnContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0f, -50f);
        containerRect.sizeDelta = new Vector2(320f, 280f);

        var vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20f;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        // Play button
        CreateMenuButton(btnContainer.transform, "Play", new Color(0.3f, 0.65f, 0.3f), () =>
        {
            SceneManager.LoadScene(farmSceneName);
        });

        // Settings button
        CreateMenuButton(btnContainer.transform, "Settings", new Color(0.35f, 0.45f, 0.55f), () =>
        {
            SettingsUI.Instance?.Show();
        });

        // Quit button
        CreateMenuButton(btnContainer.transform, "Quit", new Color(0.55f, 0.25f, 0.25f), () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        // Version text
        CreateText("Version", canvas.transform,
            new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-20f, 20f), new Vector2(200f, 30f),
            "v0.1 Alpha", 16, new Color(0.5f, 0.5f, 0.5f),
            TextAlignmentOptions.Right, FontStyles.Normal);

        // Add Settings UI
        var settingsGO = new GameObject("SettingsUI");
        settingsGO.transform.SetParent(canvas.transform, false);
        settingsGO.AddComponent<SettingsUI>();
    }

    private void CreateMenuButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject($"Btn_{label}");
        btnGO.transform.SetParent(parent, false);

        var img = btnGO.AddComponent<Image>();
        img.color = color;

        var rect = btnGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320f, 70f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;

        // Hover effect
        var colors = btn.colors;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        // Label
        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(btnGO.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 26f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var le = btnGO.AddComponent<LayoutElement>();
        le.preferredHeight = 70f;
    }

    private GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return go;
    }

    private TextMeshProUGUI CreateText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size,
        string text, float fontSize, Color color, TextAlignmentOptions alignment, FontStyles style)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = style;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return tmp;
    }
}
