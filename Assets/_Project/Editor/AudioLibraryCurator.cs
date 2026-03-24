using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools > CozyFarm > Audio Library Curator
/// Browse Universal Sound FX, mark clips to keep, copy them to _Project/Audio/SFX,
/// then delete the rest of the library to save disk space.
/// </summary>
public class AudioLibraryCurator : EditorWindow
{
    private const string SourceRoot   = "Assets/PaidAssets/Universal Sound FX";
    private const string DestRoot     = "Assets/_Project/Audio/SFX";

    // Clips pre-flagged as recommended for this game
    private static readonly HashSet<string> Recommended = new HashSet<string>
    {
        // Farming — planting
        "PLANTING_Seeds_01_mono", "PLANTING_Seeds_02_mono", "PLANTING_Seeds_03_mono",
        "PLANTING_Seeds_04_mono", "PLANTING_Seeds_05_mono",
        // Farming — digging / soil
        "SPADE_Dig_01_mono", "SPADE_Dig_02_mono", "SPADE_Dig_03_mono",
        "SPADE_Dig_04_mono", "SPADE_Dig_05_mono", "SPADE_Dig_06_mono",
        "SPADE_Dig_07_mono", "SPADE_Dig_08_mono",
        // Farming — harvest
        "HARVEST_Crops_Scythe_01_RR1_mono", "HARVEST_Crops_Scythe_01_RR2_mono",
        "HARVEST_Crops_Scythe_01_RR3_mono", "HARVEST_Crops_Scythe_01_RR4_mono",
        // Watering
        "TAP_Kitchen_Water_Turn_On_mono",
        "STREAM_FOUNTAIN_Trickling_loop_mono",
        // Selling / coins
        "CASH_REGISTER_Cha-ching_01_mono", "CASH_REGISTER_Cha-ching_02_mono",
        "CASH_REGISTER_Cha-ching_03_mono", "CASH_REGISTER_Cha-ching_04_mono",
        "CASH_REGISTER_Cha-ching_05_mono",
        "SLOT_MACHINE_Win_Dispense_Coin_01_RR1_mono",
        "SLOT_MACHINE_Win_Dispense_Coin_01_RR2_mono",
        // Collectibles — 8-bit coin
        "8BIT_RETRO_Coin_Collect_Two_Note_Bright_Fast_mono",
        "8BIT_RETRO_Coin_Collect_One_Note_Twinkle_Short_mono",
        "8BIT_RETRO_Coin_Collect_Two_Note_Bright_Twinkle_mono",
        // Level up / powerup
        "8BIT_RETRO_Powerup_Spawn_Quick_Climbing_mono",
        "8BIT_RETRO_Powerup_Spawn_Bright_Repeating_Fading_mono",
        "FIREWORKS_Rocket_Explode_Sparkle_mono",
        // Animals — dog
        "ANIMAL_Dog_Bark_03_RR01_mono", "ANIMAL_Dog_Bark_03_RR02_mono",
        "ANIMAL_Dog_Bark_03_RR03_mono", "ANIMAL_Dog_Bark_03_RR04_mono",
        // Animals — cat
        "ANIMAL_Cat_Meow_01_RR01_mono", "ANIMAL_Cat_Meow_01_RR02_mono",
        "ANIMAL_Cat_Meow_03_RR01_mono", "ANIMAL_Cat_Meow_03_RR02_mono",
        // Animals — sheep
        "ANIMAL_Sheep_Bleat_Baa_Meh_01_mono", "ANIMAL_Sheep_Bleat_Baa_Meh_02_mono",
        "ANIMAL_Sheep_Bleat_Baa_Meh_03_mono", "ANIMAL_Sheep_Bleat_Baa_Meh_04_mono",
        // Animals — ambient farm
        "ANIMAL_Rooster_Crow_01_mono", "ANIMAL_Rooster_Crow_02_mono",
        "ANIMAL_Bird_Singing_Spring_mono", "ANIMAL_Birds_01_loop_mono",
        // Ambience loops
        "AMBIENCE_Summer_Broadleaf_Forest_Wind_loop_stereo",
        "AMBIENCE_Forest_Wind_Stream_10sec_loop_stereo",
        "AMBIENCES_Medieval_Village_loop_stereo",
        "INSECT_Crickets_Segment_01_mono", "INSECT_Crickets_Segment_02_mono",
        "INSECT_Crickets_Segment_03_mono", "INSECT_Crickets_Segment_04_mono",
    };

    // --- State ---
    private List<AudioEntry>        allEntries   = new List<AudioEntry>();
    private Dictionary<string, bool> folderOpen  = new Dictionary<string, bool>();
    private Vector2                 scroll;
    private string                  searchFilter = "";
    private bool                    showOnlyChecked;

    private class AudioEntry
    {
        public string AssetPath;   // e.g. Assets/Universal Sound FX/AGRICULTURE.../foo.wav
        public string FileName;    // no extension
        public string Category;    // top-level folder under SourceRoot
        public bool   Keep;
    }

    [MenuItem("Tools/CozyFarm/Audio Library Curator")]
    public static void Open()
    {
        var w = GetWindow<AudioLibraryCurator>("Audio Curator");
        w.minSize = new Vector2(520, 600);
        w.Refresh();
    }

    private void Refresh()
    {
        allEntries.Clear();
        folderOpen.Clear();

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { SourceRoot });
        foreach (string guid in guids)
        {
            string path  = AssetDatabase.GUIDToAssetPath(guid);
            string rel   = path.Substring(SourceRoot.Length + 1); // strip "Assets/Universal Sound FX/"
            string cat   = rel.Contains("/") ? rel.Split('/')[0] : "Root";
            string name  = Path.GetFileNameWithoutExtension(path);
            bool   keep  = Recommended.Contains(name);

            allEntries.Add(new AudioEntry { AssetPath = path, FileName = name, Category = cat, Keep = keep });
            if (!folderOpen.ContainsKey(cat)) folderOpen[cat] = keep; // auto-open categories with recommended clips
        }

        allEntries = allEntries.OrderBy(e => e.Category).ThenBy(e => e.FileName).ToList();
    }

    private void OnGUI()
    {
        // Toolbar
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label($"Universal Sound FX  ({allEntries.Count(e => e.Keep)} / {allEntries.Count} kept)", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60))) Refresh();
        EditorGUILayout.EndHorizontal();

        // Search + filter row
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchFilter    = EditorGUILayout.TextField(searchFilter).ToLower();
        showOnlyChecked = GUILayout.Toggle(showOnlyChecked, "Checked only", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Select/deselect all visible
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check All Visible",   GUILayout.Height(22))) SetVisible(true);
        if (GUILayout.Button("Uncheck All Visible", GUILayout.Height(22))) SetVisible(false);
        if (GUILayout.Button("Check Recommended",   GUILayout.Height(22))) CheckRecommended();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Clip list grouped by category
        scroll = EditorGUILayout.BeginScrollView(scroll);

        string currentCat = null;
        foreach (var e in allEntries)
        {
            if (!MatchesFilter(e)) continue;

            // Category header
            if (e.Category != currentCat)
            {
                currentCat = e.Category;
                if (!folderOpen.ContainsKey(currentCat)) folderOpen[currentCat] = false;
                int catKept  = allEntries.Count(x => x.Category == currentCat && x.Keep);
                int catTotal = allEntries.Count(x => x.Category == currentCat);
                string label = $"  {currentCat}  ({catKept}/{catTotal})";
                folderOpen[currentCat] = EditorGUILayout.Foldout(folderOpen[currentCat], label, true, EditorStyles.foldoutHeader);
            }

            if (!folderOpen.ContainsKey(currentCat) || !folderOpen[currentCat]) continue;

            // Entry row
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            bool newKeep = EditorGUILayout.ToggleLeft(e.FileName, e.Keep);
            if (newKeep != e.Keep) e.Keep = newKeep;

            // Play button
            if (GUILayout.Button("▶", GUILayout.Width(24), GUILayout.Height(16)))
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(e.AssetPath);
                if (clip != null) PlayClip(clip);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);
        DrawActionButtons();
    }

    private void DrawActionButtons()
    {
        int keepCount   = allEntries.Count(e => e.Keep);
        int deleteCount = allEntries.Count - keepCount;

        EditorGUILayout.HelpBox(
            $"{keepCount} clips will be COPIED to {DestRoot}\n{deleteCount} clips will be DELETED",
            MessageType.Info);

        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button($"Copy {keepCount} Selected Clips to _Project/Audio/SFX", GUILayout.Height(32)))
            CopySelected();

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button($"Delete Entire 'Universal Sound FX' Folder  ({allEntries.Count} files)", GUILayout.Height(32)))
        {
            if (EditorUtility.DisplayDialog("Delete Universal Sound FX?",
                "This will permanently delete the entire Universal Sound FX folder from your project.\n\nMake sure you have already copied the clips you want to keep.",
                "Delete", "Cancel"))
                DeleteSourceFolder();
        }
        GUI.backgroundColor = Color.white;
    }

    // ---- Actions ----

    private void CopySelected()
    {
        int copied = 0;
        foreach (var e in allEntries)
        {
            if (!e.Keep) continue;

            string dest = Path.Combine(DestRoot, "Universal", e.Category,
                          Path.GetFileName(e.AssetPath)).Replace("\\", "/");
            string destDir = Path.GetDirectoryName(dest);

            if (!Directory.Exists(Path.Combine(Application.dataPath, "../", destDir)))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "../", destDir));

            AssetDatabase.CopyAsset(e.AssetPath, dest);
            copied++;
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", $"Copied {copied} clips to {DestRoot}/Universal/", "OK");
    }

    private void DeleteSourceFolder()
    {
        bool ok = AssetDatabase.DeleteAsset(SourceRoot);
        AssetDatabase.Refresh();
        if (ok)
        {
            allEntries.Clear();
            EditorUtility.DisplayDialog("Deleted", "Universal Sound FX folder removed.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Could not delete folder. Try deleting manually.", "OK");
        }
    }

    // ---- Helpers ----

    private bool MatchesFilter(AudioEntry e)
    {
        if (showOnlyChecked && !e.Keep) return false;
        if (!string.IsNullOrEmpty(searchFilter) && !e.FileName.ToLower().Contains(searchFilter)) return false;
        return true;
    }

    private void SetVisible(bool value)
    {
        foreach (var e in allEntries)
            if (MatchesFilter(e)) e.Keep = value;
    }

    private void CheckRecommended()
    {
        foreach (var e in allEntries)
            e.Keep = Recommended.Contains(e.FileName);
    }

    private static void PlayClip(AudioClip clip)
    {
        var method = typeof(AudioImporter).Assembly
            .GetType("UnityEditor.AudioUtil")
            ?.GetMethod("PlayPreviewClip",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        method?.Invoke(null, new object[] { clip, 0, false });
    }
}
