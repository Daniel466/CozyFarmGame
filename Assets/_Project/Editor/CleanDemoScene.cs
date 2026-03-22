using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Removes pre-placed polyperfect crop/plant props from the demo farm scene.
/// Keeps flower beds, terrain, fences, animals, trees and buildings intact.
/// Run via: Tools > CozyFarm > Clean Demo Scene
/// </summary>
public static class CleanDemoScene
{
    // ── Crop props ─────────────────────────────────────────────────────────────
    // Unity renames duplicates as "carrot (1)", "carrot (2)" etc., so we match by
    // StartsWith rather than exact equality.
    private static readonly string[] CropPropPrefixes =
    {
        "wheat-plant",    // ~162 instances — planted in rows across all beds
        "carrot",         // ~61  instances — carrot crop rows
        "corn-plant",     // ~60  instances — corn crop rows
        "plant-salad",    // ~60  instances — lettuce/salad crop rows
        "cotton",         // ~34  instances — cotton crop rows
        "pumkin-leaves",  // ~23  instances — pumpkin crop rows
    };

    // ── Vehicle props ───────────────────────────────────────────────────────────
    // Matched by Contains() — any object whose name includes these substrings.
    // Keeps: bike-old, motorbike-old, lawn-mower-ride (farm tools / non-road vehicles).
    private static readonly string[] VehicleKeywords =
    {
        "truck",
        "tractor",
        "car",
        "vehicle",
    };

    // ── Matching helpers ────────────────────────────────────────────────────────

    private static bool IsCropProp(string goName)
    {
        foreach (var prefix in CropPropPrefixes)
        {
            // Matches "carrot", "carrot (1)", "carrot (38)", etc.
            if (goName.Equals(prefix, System.StringComparison.OrdinalIgnoreCase))
                return true;
            if (goName.StartsWith(prefix + " (", System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static bool IsVehicleProp(string goName)
    {
        foreach (var keyword in VehicleKeywords)
            if (goName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        return false;
    }

    private static bool IsTargetProp(string goName) =>
        IsCropProp(goName) || IsVehicleProp(goName);

    private static string GetBaseName(string goName)
    {
        foreach (var prefix in CropPropPrefixes)
            if (goName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                return prefix;
        // For vehicles, return the name as-is (usually only 1 instance each)
        return goName;
    }

    [MenuItem("Tools/CozyFarm/Clean Demo Scene")]
    public static void CleanScene()
    {
        // Collect all matching objects first
        var toDelete = new List<GameObject>();
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var go in allObjects)
        {
            if (IsTargetProp(go.name))
                toDelete.Add(go);
        }

        if (toDelete.Count == 0)
        {
            EditorUtility.DisplayDialog("Clean Demo Scene",
                "No pre-placed crop props found.\n\nThe scene may already be clean, or the objects may have different names.",
                "OK");
            return;
        }

        // Build a summary grouped by base name (strip " (N)" suffix) for the confirmation dialog
        var summary = new Dictionary<string, int>();
        foreach (var go in toDelete)
        {
            string key = GetBaseName(go.name);
            summary[key] = summary.TryGetValue(key, out int c) ? c + 1 : 1;
        }

        var lines = new System.Text.StringBuilder();
        lines.AppendLine($"Found {toDelete.Count} crop prop(s) to remove:\n");
        foreach (var kvp in summary)
            lines.AppendLine($"  • {kvp.Value}x  {kvp.Key}");
        lines.AppendLine("\nKeeping: flower beds, fences, animals, hay, barrels, trees, buildings, decorative flowers.\n");
        lines.AppendLine("This action supports Undo (Ctrl+Z).");

        bool confirmed = EditorUtility.DisplayDialog(
            "Clean Demo Scene",
            lines.ToString(),
            "Remove Crop Props",
            "Cancel");

        if (!confirmed) return;

        // Register all objects with Undo before destroying
        Undo.SetCurrentGroupName("Clean Demo Scene — Remove Crop Props");
        int group = Undo.GetCurrentGroup();

        foreach (var go in toDelete)
        {
            if (go == null) continue; // guard against duplicates already destroyed
            Undo.DestroyObjectImmediate(go);
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Clean Demo Scene",
            $"Done! Removed {toDelete.Count} crop prop(s).\n\nSave the scene to persist, or press Ctrl+Z to undo.",
            "OK");

        Debug.Log($"[CleanDemoScene] Removed {toDelete.Count} crop props from scene.");
    }

    /// <summary>
    /// Logs all unique GameObject names in the scene — useful for auditing what's present.
    /// Run via: Tools > CozyFarm > List Scene Object Names
    /// </summary>
    [MenuItem("Tools/CozyFarm/List Scene Object Names")]
    public static void ListSceneObjectNames()
    {
        var counts = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            string name = go.name;
            counts[name] = counts.TryGetValue(name, out int c) ? c + 1 : 1;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[CleanDemoScene] {counts.Count} unique GameObject names in scene:\n");
        foreach (var kvp in new SortedDictionary<string, int>(counts))
            sb.AppendLine($"  {kvp.Value,4}x  {kvp.Key}");

        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog("Scene Object Names",
            $"Found {counts.Count} unique names.\nFull list printed to the Console.", "OK");
    }
}
