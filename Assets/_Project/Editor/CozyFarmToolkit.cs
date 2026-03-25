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
    private Vector2 _scroll;

    // ── Styles (lazy-init in OnGUI) ────────────────────────────────────────────
    private GUIStyle _sectionHeader;
    private GUIStyle _subLabel;
    private GUIStyle _bigButton;
    private GUIStyle _dangerButton;
    private bool _stylesReady;

    // ── Save path (mirrors SaveManager) ───────────────────────────────────────
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    // ── Quick Test Mode save JSON ──────────────────────────────────────────────
    private const string TestSaveJson =
        "{\n" +
        "    \"coins\": 9999,\n" +
        "    \"lifetimeEarnings\": 0,\n" +
        "    \"inventoryItems\": [],\n" +
        "    \"tiles\": [],\n" +
        "    \"buildings\": []\n" +
        "}";

    // ── Menu item ──────────────────────────────────────────────────────────────

    [MenuItem("Tools/CozyFarm/Open Toolkit", priority = 0)]
    public static void OpenWindow()
    {
        var win = GetWindow<CozyFarmToolkit>("CozyFarm Toolkit");
        win.minSize = new Vector2(320, 480);
        win.Show();
    }

    // ── Lifecycle ──────────────────────────────────────────────────────────────

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
        DrawSection("TEST TOOLS",  Color.HSVToRGB(0.02f, 0.55f, 0.48f), DrawTestTools);
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
        Rect headerRect = GUILayoutUtility.GetRect(1, 26, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(headerRect, headerColor);
        GUI.Label(headerRect, "  " + title, _sectionHeader);

        EditorGUILayout.BeginVertical("box");
        GUILayout.Space(2);
        drawContent();
        GUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private bool BigButton(string label, string tooltip = "") =>
        GUILayout.Button(new GUIContent(label, tooltip), _bigButton,
            GUILayout.Height(28), GUILayout.ExpandWidth(true));

    private bool DangerButton(string label, string tooltip = "") =>
        GUILayout.Button(new GUIContent(label, tooltip), _dangerButton,
            GUILayout.Height(28), GUILayout.ExpandWidth(true));

    // ── Section: Scene Tools ───────────────────────────────────────────────────

    private void DrawSceneTools()
    {
        if (BigButton("Full Setup",
            "Creates GameManager, FarmGrid, ground plane, and wires all systems"))
            FarmSceneSetup.SetupFarmScene();

        GUILayout.Space(2);

        if (BigButton("Game Systems Only",
            "Adds GameManager + FarmGrid without touching environment"))
            FarmSceneSetup.SetupSystemsOnly();

        GUILayout.Space(2);

        if (BigButton("Fix FarmGrid Values",
            "Sets FarmGrid to 20x20, tileSize 1, and correct position"))
            FarmSceneSetup.FixFarmGridOnly();

        GUILayout.Space(2);

        if (BigButton("Build HUD",
            "Builds the HUD Canvas with Kenney Future font and wires HUDManager"))
            HUDBuilder.BuildHUD();
    }

    // ── Section: Crop Tools ────────────────────────────────────────────────────

    private void DrawCropTools()
    {
        if (BigButton("Generate Crop Assets",
            "Creates CropData ScriptableObjects and updates CropDatabase"))
            CropAssetGenerator.GenerateCropAssets();
    }

    // ── Section: Save Tools ────────────────────────────────────────────────────

    private void DrawSaveTools()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Save file:", _subLabel, GUILayout.Width(64));
        EditorGUILayout.SelectableLabel(SavePath, EditorStyles.miniLabel, GUILayout.Height(16));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);

        bool exists = File.Exists(SavePath);
        var statusStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        statusStyle.normal.textColor = exists ? new Color(0.4f, 0.8f, 0.4f) : new Color(0.7f, 0.7f, 0.7f);
        GUILayout.Label(exists ? "Save file exists" : "No save file found", statusStyle);

        GUILayout.Space(4);

        if (BigButton("Show Save Data", "Prints save file contents to the Console"))
            ShowSaveData();

        GUILayout.Space(2);

        if (BigButton("Open Save File Location", "Opens the save folder in Finder"))
            OpenSaveLocation();

        GUILayout.Space(2);

        if (DangerButton("Clear Save File", "Permanently deletes the save file"))
            ClearSaveFile();
    }

    // ── Section: Test Tools ────────────────────────────────────────────────────

    private void DrawTestTools()
    {
        GUILayout.Label("Quick Test: 9999 coins, fast growth save written to disk.", _subLabel);
        GUILayout.Space(4);

        if (BigButton("Quick Test Mode",
            "Writes a test save with 9999 coins. Hit Play to load it."))
        {
            WriteTestSave(TestSaveJson);
            Debug.Log("[CozyFarmToolkit] Quick Test Mode: save written with 9999 coins.");
            EditorUtility.DisplayDialog("Quick Test Mode",
                "Save file written with 9999 coins.\n\nHit Play to load the test save.", "Let's Go!");
        }

        GUILayout.Space(2);

        if (DangerButton("Reset Test Mode",
            "Deletes the save file so the game starts fresh"))
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            Debug.Log("[CozyFarmToolkit] Reset Test Mode: save file deleted.");
            EditorUtility.DisplayDialog("Reset Test Mode",
                "Save file deleted. Game will start fresh on next Play.", "OK");
        }
    }

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
                $"Save directory does not exist yet:\n{dir}", "OK");
            return;
        }
        EditorUtility.RevealInFinder(dir);
    }

    private static void ClearSaveFile()
    {
        if (!File.Exists(SavePath))
        {
            EditorUtility.DisplayDialog("No Save File", "Nothing to delete.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Clear Save File",
            $"Delete the save file?\n\n{SavePath}\n\nThis cannot be undone.", "Delete", "Cancel"))
            return;
        File.Delete(SavePath);
        Debug.Log("[CozyFarmToolkit] Save file deleted.");
        EditorUtility.DisplayDialog("Done", "Save file deleted.", "OK");
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
            fontSize    = 12,
            fontStyle   = FontStyle.Normal,
            fixedHeight = 0,
        };

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
