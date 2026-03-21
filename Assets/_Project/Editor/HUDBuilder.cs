using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool that builds the entire HUD Canvas as a permanent scene object.
/// Run via: Tools > CozyFarm > Build HUD in Scene
/// This replaces the runtime HUDBootstrapper approach.
/// </summary>
public class HUDBuilder : Editor
{
    private static TMP_FontAsset font;
    private static Canvas canvas;
    private static GameObject hudCanvas;

    [MenuItem("Tools/CozyFarm/Build HUD in Scene")]
    public static void BuildHUD()
    {
        // Load font
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            AssetDatabase.FindAssets("Kenney Future SDF t:TMP_FontAsset")
            .Length > 0
            ? AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("Kenney Future SDF t:TMP_FontAsset")[0])
            : "");

        if (font == null)
        {
            // Try LiberationSans as fallback
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        }

        if (font == null)
        {
            EditorUtility.DisplayDialog("Font Missing",
                "Could not find Kenney Future SDF or LiberationSans SDF font asset.\n\n" +
                "Please generate a TMP font asset first via:\n" +
                "Window > TextMeshPro > Font Asset Creator", "OK");
            return;
        }

        // Remove existing HUD Canvas
        var existing = GameObject.Find("HUD Canvas");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Replace HUD?",
                "An existing HUD Canvas was found. Replace it?", "Yes", "Cancel"))
                return;
            DestroyImmediate(existing);
        }

        // Remove HUDBootstrapper if present
        var bootstrapper = Object.FindFirstObjectByType<HUDBootstrapper>();
        if (bootstrapper != null)
            DestroyImmediate(bootstrapper.gameObject);

        // Build canvas
        hudCanvas = new GameObject("HUD Canvas");
        canvas = hudCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = hudCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        hudCanvas.AddComponent<GraphicRaycaster>();

        // Add HUDManager
        var hudManager = hudCanvas.AddComponent<HUDManager>();

        // Build all HUD elements
        var coinsText    = CreateText("CoinsText", hudCanvas.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, -40), new Vector2(280, 50),
            "500 coins", 28, Color.white, TextAlignmentOptions.Left);

        var levelText    = CreateText("LevelText", hudCanvas.transform,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-120, -40), new Vector2(180, 50),
            "Lv. 1", 28, Color.white, TextAlignmentOptions.Right);

        var xpSlider     = CreateXPSlider(hudCanvas.transform);

        var xpText       = CreateText("XPText", hudCanvas.transform,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-120, -105), new Vector2(220, 30),
            "0 XP to next level", 16, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.Right);

        // Controls hint
        CreateText("ControlsHint", hudCanvas.transform,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(700, 40),
            "LClick: Till/Plant/Harvest  |  RClick: Water  |  B: Shop  |  Tab: Inventory  |  G: Build",
            16, new Color(1, 1, 1, 0.6f), TextAlignmentOptions.Center);

        // Tool indicator background
        CreatePanel("ToolBG", hudCanvas.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(340, 44),
            new Color(0, 0, 0, 0.55f));

        var toolText = CreateText("ToolText", hudCanvas.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(340, 44),
            "No crop selected - press B", 18, Color.white, TextAlignmentOptions.Center);

        // Notification panel
        var notifPanel = CreatePanel("NotificationPanel", hudCanvas.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(500, 50),
            new Color(0.1f, 0.1f, 0.1f, 0.85f));
        notifPanel.SetActive(false);

        var notifText = CreateText("NotificationText", notifPanel.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "", 20, Color.white, TextAlignmentOptions.Center);

        // Level up panel
        var levelUpPanel = CreatePanel("LevelUpPanel", hudCanvas.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 120),
            new Color(1, 0.85f, 0.1f, 0.95f));
        levelUpPanel.SetActive(false);

        var levelUpText = CreateText("LevelUpText", levelUpPanel.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "LEVEL UP!", 42, new Color(0.1f, 0.05f, 0), TextAlignmentOptions.Center);

        // Wire up HUDManager via SerializedObject
        var so = new SerializedObject(hudManager);
        so.FindProperty("coinsText").objectReferenceValue = coinsText;
        so.FindProperty("levelText").objectReferenceValue = levelText;
        so.FindProperty("xpSlider").objectReferenceValue = xpSlider;
        so.FindProperty("xpText").objectReferenceValue = xpText;
        so.FindProperty("levelUpPanel").objectReferenceValue = levelUpPanel;
        so.FindProperty("levelUpText").objectReferenceValue = levelUpText;
        so.FindProperty("notificationPanel").objectReferenceValue = notifPanel;
        so.FindProperty("notificationText").objectReferenceValue = notifText;
        so.FindProperty("toolText").objectReferenceValue = toolText;
        so.ApplyModifiedProperties();

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("HUD Built!",
            $"HUD Canvas created successfully in scene using font: {font.name}\n\n" +
            "All HUDManager references have been wired up automatically.", "Great!");

        Selection.activeGameObject = hudCanvas;
        Debug.Log("[HUDBuilder] HUD Canvas built successfully in scene!");
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size,
        string text, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.font = font;
        tmp.fontStyle = FontStyles.Bold;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return tmp;
    }

    private static Slider CreateXPSlider(Transform parent)
    {
        var go = new GameObject("XPSlider");
        go.transform.SetParent(parent, false);
        var slider = go.AddComponent<Slider>();
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-120, -80);
        rect.sizeDelta = new Vector2(220, 16);

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        slider.targetGraphic = bgImg;

        // Fill area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.8f, 0.4f);
        slider.fillRect = fill.GetComponent<RectTransform>();

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;
        return slider;
    }

    private static GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
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
}
