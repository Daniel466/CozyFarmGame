using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// In-game pause menu. Press Escape to toggle.
/// Shows: Resume, Settings, Save, Main Menu, Quit.
/// Attach to any persistent GameObject in the Farm scene.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameObject pausePanel;
    private Canvas canvas;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Find or create canvas
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("PauseCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // On top of everything
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        BuildPauseMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel?.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        AudioManager.Instance?.PlayUIClick();
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel?.SetActive(false);
        Time.timeScale = 1f;
    }

    private void BuildPauseMenu()
    {
        // Full screen dim
        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvas.transform, false);

        var bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        var bgRect = pausePanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Pause window (centred)
        GameObject window = new GameObject("PauseWindow");
        window.transform.SetParent(pausePanel.transform, false);
        var windowImg = window.AddComponent<Image>();
        windowImg.color = new Color(0.12f, 0.1f, 0.07f, 0.97f);
        var windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(380f, 420f);

        var vlg = window.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.spacing = 15f;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        // Title
        AddLabel(window.transform, "⏸️ Paused", 36f, new Color(1f, 0.9f, 0.6f), FontStyles.Bold);

        // Buttons
        AddButton(window.transform, "▶️  Resume", new Color(0.3f, 0.65f, 0.3f), Resume);
        AddButton(window.transform, "⚙️  Settings", new Color(0.35f, 0.45f, 0.55f), () =>
        {
            SettingsUI.Instance?.Show();
        });
        AddButton(window.transform, "💾  Save Game", new Color(0.4f, 0.35f, 0.55f), () =>
        {
            GameManager.Instance?.SaveManager?.SaveGame();
            HUDManager.Instance?.ShowNotification("Game saved! 💾");
        });
        AddButton(window.transform, "🏠  Main Menu", new Color(0.45f, 0.35f, 0.25f), () =>
        {
            GameManager.Instance?.SaveManager?.SaveGame();
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        });
        AddButton(window.transform, "🚪  Quit", new Color(0.55f, 0.25f, 0.25f), () =>
        {
            GameManager.Instance?.SaveManager?.SaveGame();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        // Add settings UI to same canvas
        var settingsGO = new GameObject("SettingsUI");
        settingsGO.transform.SetParent(canvas.transform, false);
        settingsGO.AddComponent<SettingsUI>();

        pausePanel.SetActive(false);
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
        le.preferredHeight = size + 10f;
    }

    private void AddButton(Transform parent, string label, Color color,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject($"Btn_{label}");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var colors = btn.colors;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 60f;

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
