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
    private static Canvas panelsCanvas;

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

        // Selected crop indicator — bottom left
        var selectedCropPanel = CreatePanel("SelectedCropPanel", hudCanvas.transform,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(110, 68), new Vector2(200, 52),
            new Color(0.05f, 0.05f, 0.05f, 0.72f));
        selectedCropPanel.SetActive(false);

        var selectedCropText = CreateText("SelectedCropText", selectedCropPanel.transform,
            Vector2.zero, Vector2.one, new Vector2(8, 0), new Vector2(-8, 0),
            "", 17, Color.white, TextAlignmentOptions.Left);

        // Context hint pill — bottom centre, changes based on hovered tile
        var contextHintBG = CreatePanel("ContextHintBG", hudCanvas.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(700, 36),
            new Color(0.05f, 0.05f, 0.05f, 0.72f));

        var contextHintText = CreateText("ContextHintText", contextHintBG.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "B: Shop  |  Tab: Inventory  |  G: Build",
            15, new Color(1, 1, 1, 0.85f), TextAlignmentOptions.Center);

        // Tool indicator background
        CreatePanel("ToolBG", hudCanvas.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 68), new Vector2(340, 44),
            new Color(0, 0, 0, 0.55f));

        var toolText = CreateText("ToolText", hudCanvas.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 68), new Vector2(340, 44),
            "No crop selected - press B", 18, Color.white, TextAlignmentOptions.Center);

        // Notification panel
        var notifPanel = CreatePanel("NotificationPanel", hudCanvas.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 126), new Vector2(500, 50),
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
        so.FindProperty("contextHintText").objectReferenceValue = contextHintText;
        so.FindProperty("selectedCropPanel").objectReferenceValue = selectedCropPanel;
        so.FindProperty("selectedCropText").objectReferenceValue = selectedCropText;
        so.ApplyModifiedProperties();

        // Build panels on a separate canvas
        BuildPanels();

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("HUD Built!",
            $"HUD Canvas + Panels Canvas built successfully.\nFont: {font.name}\n\n" +
            "All references wired automatically. HUDBootstrapper can be deleted.", "Great!");

        Selection.activeGameObject = hudCanvas;
        Debug.Log("[HUDBuilder] HUD Canvas + Panels Canvas built successfully in scene!");
    }

    // ─── Panels Canvas ─────────────────────────────────────────────────────────

    [MenuItem("Tools/CozyFarm/Build Panels in Scene")]
    public static void BuildPanelsOnly()
    {
        // Load font so helpers work even when called standalone
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            AssetDatabase.FindAssets("Kenney Future SDF t:TMP_FontAsset").Length > 0
                ? AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("Kenney Future SDF t:TMP_FontAsset")[0])
                : "");
        if (font == null)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        BuildPanels();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Panels Built!", "Shop, Inventory and BuildMode panels added to PanelsCanvas.", "OK");
    }

    private static void BuildPanels()
    {
        // Load databases
        var cropDB = AssetDatabase.LoadAssetAtPath<CropDatabase>(
            "Assets/_Project/ScriptableObjects/Crops/CropDatabase.asset");
        var buildingDB = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(
            "Assets/_Project/ScriptableObjects/Buildings/BuildingDatabase.asset");

        // Remove existing PanelsCanvas
        var existingPC = Object.FindFirstObjectByType<Canvas>();
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.gameObject.name == "PanelsCanvas")
            {
                if (!EditorUtility.DisplayDialog("Replace Panels?",
                    "A PanelsCanvas already exists. Replace it?", "Yes", "Cancel"))
                    return;
                Object.DestroyImmediate(c.gameObject);
                break;
            }
        }

        // Create PanelsCanvas (sort order 50 — above HUD Canvas at 10)
        var pcGO = new GameObject("PanelsCanvas");
        panelsCanvas = pcGO.AddComponent<Canvas>();
        panelsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        panelsCanvas.sortingOrder = 50;

        var scaler = pcGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        pcGO.AddComponent<GraphicRaycaster>();

        // Ensure EventSystem exists
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        BuildShopPanel(pcGO, cropDB);
        BuildInventoryPanel(pcGO);
        BuildBuildModePanel(pcGO, buildingDB);

        Debug.Log("[HUDBuilder] PanelsCanvas built with Shop, Inventory and BuildMode panels.");
    }

    private static void BuildShopPanel(GameObject canvasGO, CropDatabase cropDB)
    {
        // Full-screen dim
        var shopPanel = CreatePanelGO("ShopPanel", canvasGO.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0f, 0f, 0f, 0.75f));
        shopPanel.SetActive(false);

        // Window (left side)
        var shopWindow = CreatePanelGO("ShopWindow", shopPanel.transform,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(250f, 0f), new Vector2(500f, 0f),
            new Color(0.12f, 0.1f, 0.07f, 0.97f));

        CreateText("ShopTitle", shopWindow.transform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -30f), new Vector2(0f, 50f),
            "Seed Shop", 26, new Color(1f, 0.9f, 0.6f), TextAlignmentOptions.Center);

        CreateText("ShopCoins", shopWindow.transform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -65f), new Vector2(0f, 30f),
            "Your coins: 0", 16, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center);

        var content = BuildScrollView(shopWindow.transform,
            new Vector2(5f, 40f), new Vector2(-5f, -90f), useGrid: false);

        CreateText("CloseHint", shopWindow.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 15f), new Vector2(0f, 25f),
            "Press B to close", 16, new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.Center);

        if (cropDB == null)
            Debug.LogWarning("[HUDBuilder] CropDatabase not found at expected path — ShopUI will be empty at runtime.");

        // Add ShopUI component and wire via SerializedObject
        var shopUI = canvasGO.AddComponent<ShopUI>();
        var so = new SerializedObject(shopUI);
        so.FindProperty("shopPanel").objectReferenceValue = shopPanel;
        so.FindProperty("itemGrid").objectReferenceValue = content;
        so.FindProperty("cropDatabase").objectReferenceValue = cropDB;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(shopUI);
    }

    private static void BuildInventoryPanel(GameObject canvasGO)
    {
        // Full-screen dim
        var invPanel = CreatePanelGO("InventoryPanel", canvasGO.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0f, 0f, 0f, 0.75f));
        invPanel.SetActive(false);

        // Window (centred)
        var invWindow = CreatePanelGO("InventoryWindow", invPanel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(700f, 500f),
            new Color(0.15f, 0.12f, 0.08f, 0.97f));

        CreateText("InventoryTitle", invWindow.transform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -30f), new Vector2(0f, 50f),
            "Inventory", 32, new Color(1f, 0.9f, 0.6f), TextAlignmentOptions.Center);

        var slotsText = CreateText("SlotsText", invWindow.transform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -70f), new Vector2(0f, 30f),
            "Slots: 0/20", 18, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Center);

        var content = BuildScrollView(invWindow.transform,
            new Vector2(20f, 0f), new Vector2(-20f, 0f), useGrid: false,
            anchorMin: new Vector2(0f, 0.15f), anchorMax: new Vector2(1f, 0.85f));

        // Sell All button
        var sellBtnGO = new GameObject("SellAllButton");
        sellBtnGO.transform.SetParent(invWindow.transform, false);
        var sellBtnRect = sellBtnGO.AddComponent<RectTransform>();
        sellBtnRect.anchorMin = new Vector2(0.5f, 0f);
        sellBtnRect.anchorMax = new Vector2(0.5f, 0f);
        sellBtnRect.anchoredPosition = new Vector2(0f, 40f);
        sellBtnRect.sizeDelta = new Vector2(250f, 55f);
        var sellBtnImg = sellBtnGO.AddComponent<Image>();
        sellBtnImg.color = new Color(0.3f, 0.7f, 0.3f);
        var sellBtn = sellBtnGO.AddComponent<Button>();
        sellBtn.targetGraphic = sellBtnImg;

        var sellBtnText = CreateText("SellBtnText", sellBtnGO.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            "Sell All", 24, Color.white, TextAlignmentOptions.Center);

        CreateText("CloseHint", invWindow.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 15f), new Vector2(0f, 25f),
            "Press TAB to close", 16, new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.Center);

        // Add InventoryUI component and wire via SerializedObject
        var invUI = canvasGO.AddComponent<InventoryUI>();
        var so = new SerializedObject(invUI);
        so.FindProperty("inventoryPanel").objectReferenceValue = invPanel;
        so.FindProperty("itemGrid").objectReferenceValue = content;
        so.FindProperty("slotsText").objectReferenceValue = slotsText;
        so.FindProperty("sellAllButton").objectReferenceValue = sellBtn;
        so.FindProperty("sellAllText").objectReferenceValue = sellBtnText;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(invUI);
    }

    private static void BuildBuildModePanel(GameObject canvasGO, BuildingDatabase buildingDB)
    {
        // Full-screen dim
        var buildPanel = CreatePanelGO("BuildModePanel", canvasGO.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0f, 0f, 0f, 0.75f));
        buildPanel.SetActive(false);

        // Side panel (right side)
        var sidePanel = CreatePanelGO("BuildSidePanel", buildPanel.transform,
            new Vector2(1f, 0f), new Vector2(1f, 1f),
            new Vector2(-110f, 0f), new Vector2(220f, 0f),
            new Color(0.12f, 0.1f, 0.07f, 0.97f));

        CreateText("BuildTitle", sidePanel.transform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -30f), new Vector2(0f, 50f),
            "Build Mode", 26, new Color(1f, 0.9f, 0.6f), TextAlignmentOptions.Center);

        CreateText("BuildHint", sidePanel.transform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -65f), new Vector2(0f, 30f),
            "R: Rotate  |  Del: Remove  |  Esc: Cancel", 13,
            new Color(0.6f, 0.6f, 0.6f), TextAlignmentOptions.Center);

        var content = BuildScrollView(sidePanel.transform,
            new Vector2(5f, 5f), new Vector2(-5f, -90f), useGrid: true);

        // Add BuildModeUI component and wire via SerializedObject
        var buildUI = canvasGO.AddComponent<BuildModeUI>();
        var so = new SerializedObject(buildUI);
        so.FindProperty("buildPanel").objectReferenceValue = buildPanel;
        so.FindProperty("itemGrid").objectReferenceValue = content;
        so.FindProperty("database").objectReferenceValue = buildingDB;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(buildUI);

        if (buildingDB == null)
            Debug.LogWarning("[HUDBuilder] BuildingDatabase not found — BuildModeUI will be empty at runtime.");
    }

    /// <summary>
    /// Creates a ScrollView with a Content child. Returns the Content transform.
    /// </summary>
    private static Transform BuildScrollView(Transform parent,
        Vector2 offsetMin, Vector2 offsetMax, bool useGrid,
        Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        var svGO = new GameObject("ScrollView");
        svGO.transform.SetParent(parent, false);
        var scrollRect = svGO.AddComponent<ScrollRect>();
        var svRect = svGO.GetComponent<RectTransform>();
        svRect.anchorMin = anchorMin ?? Vector2.zero;
        svRect.anchorMax = anchorMax ?? Vector2.one;
        svRect.offsetMin = offsetMin;
        svRect.offsetMax = offsetMax;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Viewport with RectMask2D for proper clipping
        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(svGO.transform, false);
        var viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportRect.pivot = new Vector2(0f, 1f);
        viewportGO.AddComponent<RectMask2D>();
        scrollRect.viewport = viewportRect;

        // Content sized by children
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        var contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        if (useGrid)
        {
            var glg = contentGO.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(90f, 110f);
            glg.spacing = new Vector2(5f, 5f);
            glg.padding = new RectOffset(5, 5, 5, 5);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;
        }
        else
        {
            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5f;
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
        }

        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;

        return contentGO.transform;
    }

    /// <summary>Helper that creates a panel without using the static font field.</summary>
    private static GameObject CreatePanelGO(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name);
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

    // ─── Text / Panel helpers ───────────────────────────────────────────────────

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
        rect.sizeDelta = new Vector2(220, 12);

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

        // Fill area — inset slightly so fill stays within background
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0);
        fillAreaRect.anchorMax = new Vector2(1, 1);
        fillAreaRect.offsetMin = new Vector2(2, 2);
        fillAreaRect.offsetMax = new Vector2(-2, -2);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.8f, 0.4f);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        slider.fillRect = fillRect;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
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
