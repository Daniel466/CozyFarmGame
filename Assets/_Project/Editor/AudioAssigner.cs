using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Tools > CozyFarm > Assign Audio Clips
/// Finds AudioManager and AmbienceManager in the scene and assigns
/// Universal Sound FX clips to all serialised fields automatically.
/// Searches both the copied location (_Project/Audio/SFX/Universal) and
/// the source library (PaidAssets/Universal Sound FX) as fallback.
/// </summary>
public static class AudioAssigner
{
    [MenuItem("Tools/CozyFarm/Assign Audio Clips")]
    public static void Assign()
    {
        int changes = 0;
        changes += AssignAudioManager();
        changes += AssignAmbienceManager();

        if (changes > 0)
        {
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[AudioAssigner] Assigned {changes} clips. Save the scene to persist.");
            EditorUtility.DisplayDialog("Done", $"Assigned {changes} audio clips.\nSave the scene to persist changes.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Nothing assigned", "No clips were found or all fields already filled.\nRun the Audio Library Curator first to copy clips.", "OK");
        }
    }

    // ── AudioManager ────────────────────────────────────────────────────────

    private static int AssignAudioManager()
    {
        var mgr = Object.FindFirstObjectByType<AudioManager>();
        if (mgr == null) { Debug.LogWarning("[AudioAssigner] AudioManager not found in scene."); return 0; }

        var so = new SerializedObject(mgr);
        int assigned = 0;

        // Single-clip fields: field name → search keywords (tries in order)
        AssignSingle(so, "sellSFX",        ref assigned, "Sell");
        AssignSingle(so, "levelUpSFX",     ref assigned, "Level UP", "8BIT_RETRO_Powerup_Spawn_Quick_Climbing_mono");
        AssignSingle(so, "collectSFX",     ref assigned, "Collect", "8BIT_RETRO_Coin_Collect_Two_Note_Bright_Fast_mono");
        AssignSingle(so, "uiClickSFX",     ref assigned, "UI Click");
        AssignSingle(so, "buildPlaceSFX",  ref assigned, "Build Placed");
        AssignSingle(so, "buildRemoveSFX", ref assigned, "Remove Build");

        AssignArray(so, "dogBarkClips", ref assigned,
            "ANIMAL_Dog_Bark_03_RR01_mono", "ANIMAL_Dog_Bark_03_RR02_mono",
            "ANIMAL_Dog_Bark_03_RR03_mono", "ANIMAL_Dog_Bark_03_RR04_mono");

        // Array fields
        AssignArray(so, "tillSFXClips",    ref assigned,
            "SPADE_Dig_01_mono", "SPADE_Dig_02_mono", "SPADE_Dig_03_mono",
            "SPADE_Dig_04_mono", "SPADE_Dig_05_mono", "SPADE_Dig_06_mono",
            "SPADE_Dig_07_mono", "SPADE_Dig_08_mono");

        AssignArray(so, "plantSFXClips",   ref assigned,
            "PLANTING_Seeds_01_mono", "PLANTING_Seeds_02_mono", "PLANTING_Seeds_03_mono",
            "PLANTING_Seeds_04_mono", "PLANTING_Seeds_05_mono");

        AssignArray(so, "waterSFXClips",   ref assigned,
            "TAP_Kitchen_Water_Turn_On_mono",
            "DRINK_Pour_Liquid_In_Glass_Short_mono",
            "DRINK_Pour_Liquid_In_Glass_Splashy_mono");

        AssignArray(so, "harvestSFXClips", ref assigned,
            "HARVEST_Crops_Scythe_01_RR1_mono", "HARVEST_Crops_Scythe_01_RR2_mono",
            "HARVEST_Crops_Scythe_01_RR3_mono", "HARVEST_Crops_Scythe_01_RR4_mono");

        so.ApplyModifiedProperties();
        return assigned;
    }

    // ── AmbienceManager ─────────────────────────────────────────────────────

    private static int AssignAmbienceManager()
    {
        var mgr = Object.FindFirstObjectByType<AmbienceManager>();
        if (mgr == null) { Debug.LogWarning("[AudioAssigner] AmbienceManager not found in scene."); return 0; }

        var so = new SerializedObject(mgr);
        int assigned = 0;

        AssignArray(so, "ambienceClips", ref assigned,
            "AMBIENCE_Summer_Broadleaf_Forest_Wind_loop_stereo",
            "ANIMAL_Birds_01_loop_mono",
            "AMBIENCE_Forest_Wind_Stream_10sec_loop_stereo",
            "AMBIENCES_Medieval_Village_loop_stereo");

        so.ApplyModifiedProperties();
        return assigned;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// Assigns first found clip to a single SerializedProperty field (skips if already set).
    private static void AssignSingle(SerializedObject so, string propName, ref int count, params string[] names)
    {
        var prop = so.FindProperty(propName);
        if (prop == null || prop.objectReferenceValue != null) return;

        foreach (string name in names)
        {
            var clip = FindClip(name);
            if (clip == null) continue;
            prop.objectReferenceValue = clip;
            count++;
            Debug.Log($"[AudioAssigner] {propName} <- {clip.name}");
            return;
        }
        Debug.LogWarning($"[AudioAssigner] Could not find clip for '{propName}' (tried: {string.Join(", ", names)})");
    }

    /// Assigns an array of clips by name. Skips slots already filled; appends missing entries.
    private static void AssignArray(SerializedObject so, string propName, ref int count, params string[] names)
    {
        var prop = so.FindProperty(propName);
        if (prop == null || !prop.isArray) return;

        // Build list of clips that were found
        var clips = names
            .Select(n => FindClip(n))
            .Where(c => c != null)
            .ToArray();

        if (clips.Length == 0)
        {
            Debug.LogWarning($"[AudioAssigner] No clips found for array '{propName}'");
            return;
        }

        // Only assign if array is currently empty
        if (prop.arraySize > 0) return;

        prop.arraySize = clips.Length;
        for (int i = 0; i < clips.Length; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            Debug.Log($"[AudioAssigner] {propName}[{i}] <- {clips[i].name}");
        }
        count += clips.Length;
    }

    /// Searches copied location first, then falls back to source library.
    private static AudioClip FindClip(string name)
    {
        // Search copied location
        string[] guids = AssetDatabase.FindAssets($"{name} t:AudioClip",
            new[] { "Assets/_Project/Audio/SFX" });

        // Fallback: search source library
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets($"{name} t:AudioClip",
                new[] { "Assets/PaidAssets/Universal Sound FX" });

        // Fallback: search existing project SFX
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets($"{name} t:AudioClip",
                new[] { "Assets/_Project/Audio" });

        if (guids.Length == 0) return null;

        // Pick exact match if multiple results
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == name)
                return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        // Take first result if no exact match
        return AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}
