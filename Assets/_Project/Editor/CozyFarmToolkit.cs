using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Central editor window for all CozyFarm tools.
/// Open via: Tools > CozyFarm > Open Toolkit
/// </summary>
public class CozyFarmToolkit : EditorWindow
{
    // ── State ──────────────────────────────────────────────────────────────────
    private float _growthSpeed = 1f;
    private Vector2 _scroll;

    // ── Styles (lazy-init in OnGUI) ────────────────────────────────────────────
    private GUIStyle _sectionHeader;
    private GUIStyle _subLabel;
    private GUIStyle _bigButton;
    private GUIStyle _dangerButton;
    private Texture2D _headerTex;
    private Texture2D _dangerTex;
    private bool _stylesReady;

    // ── Save path (mirrors SaveManager) ───────────────────────────────────────
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    // ── Quick Test Mode save JSON ──────────────────────────────────────────────
    // Level 15 = max (9000 XP), 9999 coins — unlocks every crop and building.
    private const string TestSaveJson =
        "{\n" +
        "    \"coins\": 9999,\n" +
        "    \"xp\": 9000,\n" +
        "    \"level\": 15,\n" +
        "    \"inventoryItems\": [],\n" +
        "    \"tiles\": []\n" +
        "}";

    // ── Menu item ──────────────────────────────────────────────────────────────

    [MenuItem("Tools/CozyFarm/Open Toolkit", priority = 0)]
    public static void OpenWindow()
    {
        var win = GetWindow<CozyFarmToolkit>("CozyFarm Toolkit");
        win.minSize = new Vector2(320, 520);
        win.Show();
    }

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void OnEnable() => RefreshGrowthSpeed();

    private void OnGUI()
    {
        EnsureStyles();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawWindowHeader();
        GUILayout.Space(6);

        DrawSection("SCENE TOOLS", Color.HSVToRGB(0.33f, 0.55f, 0.45f), DrawSceneTools);
        GUILayout.Space(4);
        DrawSection("CROP TOOLS",  Color.HSVToRGB(0.12f, 0.60f, 0.50f), DrawCropTools);
        GUILayout.Space(4);
        DrawSection("SAVE TOOLS",  Color.HSVToRGB(0.58f, 0.50f, 0.45f), DrawSaveTools);
        GUILayout.Space(4);
        DrawSection("BUILD & TEST TOOLS", Color.HSVToRGB(0.02f, 0.55f, 0.48f), DrawBuildTools);
        GUILayout.Space(10);

        EditorGUILayout.EndScrollView();
    }

    // ── Layout helpers ─────────────────────────────────────────────────────────

    private void DrawWindowHeader()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space(4);
        GUILayout.Label("CozyFarm Toolkit", new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
        });
        GUILayout.Label("All editor tools in one place", new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 11,
        });
        GUILayout.Space(2);
        Rect r = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f, 0.4f));
        EditorGUILayout.EndVertical();
    }

    private void DrawSection(string title, Color headerColor, System.Action drawContent)
    {
        // Header bar
        Rect headerRect = GUILayoutUtility.GetRect(1, 26, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(headerRect, headerColor);
        GUI.Label(headerRect, "  " + title, _sectionHeader);

        // Content box
        EditorGUILayout.BeginVertical("box");
        GUILayout.Space(2);
        drawContent();
        GUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private bool BigButton(string label, string tooltip = "")
    {
        return GUILayout.Button(new GUIContent(label, tooltip), _bigButton,
            GUILayout.Height(28), GUILayout.ExpandWidth(true));
    }

    private bool DangerButton(string label, string tooltip = "")
    {
        return GUILayout.Button(new GUIContent(label, tooltip), _dangerButton,
            GUILayout.Height(28), GUILayout.ExpandWidth(true));
    }

    // ── Section: Scene Tools ───────────────────────────────────────────────────

    private void DrawSceneTools()
    {
        if (BigButton("Fix Flower Bed Layers",
            "Sets all flower bed objects to Ignore Raycast so clicks pass through to farm tiles"))
            FarmSceneSetup.FixFlowerBedLayers();

        GUILayout.Space(2);

        if (BigButton("Clean Demo Scene",
            "Removes pre-placed polyperfect crop props (wheat, corn, carrot etc.) from flower beds"))
            CleanDemoScene.CleanScene();

        GUILayout.Space(2);

        if (BigButton("Setup Farm Scene",
            "Creates and wires all required GameObjects (GameManager, Player, Camera, HUD, Audio)"))
            FarmSceneSetup.SetupFarmScene();

        GUILayout.Space(2);

        if (BigButton("Build HUD in Scene",
            "Builds the HUD Canvas with Kenney Future font and wires all HUDManager references"))
            HUDBuilder.BuildHUD();
    }

    // ── Section: Crop Tools ────────────────────────────────────────────────────

    private void DrawCropTools()
    {
        if (BigButton("Assign Crop Models",
            "Assigns polyperfect prefabs to all 10 CropData ScriptableObjects"))
            CropModelAssigner.AssignCropModels();

        GUILayout.Space(2);

        if (BigButton("Generate Crop Assets",
            "Creates all 10 CropData ScriptableObjects and updates CropDatabase"))
            CropAssetGenerator.GenerateCropAssets();

        GUILayout.Space(2);

        if (DangerButton("Clear Crop Models",
            "Removes all prefab assignments from every CropData asset"))
            CropModelAssigner.ClearCropModels();
    }

    // ── Section: Save Tools ────────────────────────────────────────────────────

    private void DrawSaveTools()
    {
        // Show save file path
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Save file:", _subLabel, GUILayout.Width(64));
        EditorGUILayout.SelectableLabel(SavePath, EditorStyles.miniLabel,
            GUILayout.Height(16));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);

        // Existence status
        bool exists = File.Exists(SavePath);
        var statusStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        statusStyle.normal.textColor = exists
            ? new Color(0.4f, 0.8f, 0.4f)
            : new Color(0.7f, 0.7f, 0.7f);
        GUILayout.Label(exists ? "Save file exists" : "No save file found", statusStyle);

        GUILayout.Space(4);

        if (BigButton("Show Save Data",
            "Prints the current save file contents to the Console"))
            ShowSaveData();

        GUILayout.Space(2);

        if (BigButton("Open Save File Location",
            "Opens the save file folder in Finder"))
            OpenSaveLocation();

        GUILayout.Space(2);

        if (DangerButton("Clear Save File",
            "Permanently deletes the save file — game will start fresh next Play"))
            ClearSaveFile();
    }

    // ── Section: Build & Test Tools ────────────────────────────────────────────

    private void DrawBuildTools()
    {
        // Growth speed control
        GUILayout.Label("Growth Speed Multiplier", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _growthSpeed = EditorGUILayout.FloatField(_growthSpeed, GUILayout.Width(70));
        _growthSpeed = Mathf.Max(0.1f, _growthSpeed);

        string currentLabel = GetCurrentGrowthSpeed(out float current);
        GUILayout.Label($"Current: {currentLabel}", _subLabel);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(2);

        if (BigButton("Apply Growth Speed",
            "Sets FarmingManager.growthSpeedMultiplier in the scene"))
            ApplyGrowthSpeed(_growthSpeed);

        GUILayout.Space(6);
        Rect sep = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(sep, new Color(0.4f, 0.4f, 0.4f, 0.3f));
        GUILayout.Space(6);

        // Preset buttons
        GUILayout.Label("Test Mode Presets", EditorStyles.boldLabel);
        GUILayout.Label(
            "Quick Test: growth 60x + 9999 coins + level 15 (saved to file, apply on next Play)",
            _subLabel);
        GUILayout.Space(2);

        if (BigButton("Quick Test Mode",
            "Sets growth to 60x and writes a test save with 9999 coins and level 15"))
        {
            ApplyGrowthSpeed(60f);
            _growthSpeed = 60f;
            WriteTestSave(TestSaveJson);
            Debug.Log("[CozyFarmToolkit] Quick Test Mode: growth=60x, save written with 9999 coins + level 15.");
            EditorUtility.DisplayDialog("Quick Test Mode",
                "Growth speed set to 60x.\n\n" +
                "Save file written with:\n  • 9999 coins\n  • Level 15 (all crops unlocked)\n\n" +
                "Hit Play to load the test save.", "Let's Go!");
        }

        GUILayout.Space(2);

        if (DangerButton("Reset Test Mode",
            "Sets growth back to 1x and deletes the save file so the game starts fresh"))
        {
            ApplyGrowthSpeed(1f);
            _growthSpeed = 1f;
            if (File.Exists(SavePath)) File.Delete(SavePath);
            Debug.Log("[CozyFarmToolkit] Reset Test Mode: growth=1x, save file deleted.");
            EditorUtility.DisplayDialog("Reset Test Mode",
                "Growth speed reset to 1x.\nSave file deleted — game will start fresh on next Play.", "OK");
        }
    }

    // ── Growth speed helpers ───────────────────────────────────────────────────

    private void RefreshGrowthSpeed()
    {
        var fm = FindFarmingManager();
        if (fm == null) return;
        var so = new SerializedObject(fm);
        var prop = so.FindProperty("growthSpeedMultiplier");
        if (prop != null) _growthSpeed = prop.floatValue;
    }

    private string GetCurrentGrowthSpeed(out float value)
    {
        value = -1f;
        var fm = FindFarmingManager();
        if (fm == null) return "(FarmingManager not in scene)";
        var so = new SerializedObject(fm);
        var prop = so.FindProperty("growthSpeedMultiplier");
        if (prop == null) return "(field not found)";
        value = prop.floatValue;
        return $"{value}x";
    }

    private void ApplyGrowthSpeed(float speed)
    {
        var fm = FindFarmingManager();
        if (fm == null)
        {
            EditorUtility.DisplayDialog("FarmingManager Not Found",
                "No FarmingManager found in the current scene.\n\nRun Setup Farm Scene first.", "OK");
            return;
        }
        var so = new SerializedObject(fm);
        var prop = so.FindProperty("growthSpeedMultiplier");
        if (prop == null) { Debug.LogError("[CozyFarmToolkit] growthSpeedMultiplier field not found."); return; }
        prop.floatValue = speed;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(fm);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[CozyFarmToolkit] growthSpeedMultiplier set to {speed}x");
        Repaint();
    }

    private static FarmingManager FindFarmingManager() =>
        Object.FindFirstObjectByType<FarmingManager>();

    // ── Save tool helpers ──────────────────────────────────────────────────────

    private static void ShowSaveData()
    {
        if (!File.Exists(SavePath))
        {
            EditorUtility.DisplayDialog("No Save File", "No save file found at:\n" + SavePath, "OK");
            return;
        }
        string json = File.ReadAllText(SavePath);
        Debug.Log($"[CozyFarmToolkit] Save file contents:\n{json}");
        EditorUtility.DisplayDialog("Save Data", "Save file contents printed to the Console.", "OK");
    }

    private static void OpenSaveLocation()
    {
        string dir = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(dir))
        {
            EditorUtility.DisplayDialog("Folder Not Found",
                $"Save directory does not exist yet:\n{dir}\n\nIt will be created when the game first saves.", "OK");
            return;
        }
        EditorUtility.RevealInFinder(dir);
    }

    private static void ClearSaveFile()
    {
        if (!File.Exists(SavePath))
        {
            EditorUtility.DisplayDialog("No Save File", "No save file found — nothing to delete.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Clear Save File",
            $"Delete the save file?\n\n{SavePath}\n\nThis cannot be undone.", "Delete", "Cancel"))
            return;
        File.Delete(SavePath);
        Debug.Log("[CozyFarmToolkit] Save file deleted.");
        EditorUtility.DisplayDialog("Done", "Save file deleted. Game will start fresh on next Play.", "OK");
    }

    private static void WriteTestSave(string json)
    {
        string dir = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(SavePath, json);
    }

    // ── Style init ─────────────────────────────────────────────────────────────

    private void EnsureStyles()
    {
        if (_stylesReady) return;

        _sectionHeader = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 11,
            alignment = TextAnchor.MiddleLeft,
            normal    = { textColor = Color.white },
        };

        _subLabel = new GUIStyle(EditorStyles.miniLabel)
        {
            wordWrap = true,
            normal   = { textColor = new Color(0.7f, 0.7f, 0.7f) },
        };

        _bigButton = new GUIStyle(GUI.skin.button)
        {
            fontSize   = 12,
            fontStyle  = FontStyle.Normal,
            fixedHeight = 0,
        };

        // Danger button — slightly reddish tint via normal color (can't easily tint buttons
        // without custom textures in built-in skin, so we rely on label color)
        _dangerButton = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 12,
            fontStyle = FontStyle.Normal,
            normal    = { textColor = new Color(1f, 0.55f, 0.45f) },
            hover     = { textColor = new Color(1f, 0.4f, 0.3f) },
        };

        _stylesReady = true;
    }
}
