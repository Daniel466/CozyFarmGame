using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools > CozyFarm > Assign Poly Universal Pack Crop Models
///
/// Maps Poly Universal Pack Farm/Crops Farm prefabs to the 4 growth-stage slots
/// on each CropData ScriptableObject. Also renames Grapes/Chilli/Lavender
/// (which have no pack models) to Watermelon/Leek/Wheat which do.
///
/// All 10 crops are fully covered after running this tool.
/// </summary>
public static class CropModelAssigner
{
    private const string CropsPath = "Assets/_Project/ScriptableObjects/Crops";
    private const string PackBase  = "Assets/PaidAssets/Poly Universal Pack/- Prefabs/Farm/Crops Farm";

    [MenuItem("Tools/CozyFarm/Assign Poly Universal Pack Crop Models")]
    public static void AssignAll()
    {
        int updated = 0;

        // ── Rename Grapes → Watermelon, Chilli → Leek, Lavender → Wheat ──────
        updated += RenameCrop("Grapes",   "watermelon", "Watermelon");
        updated += RenameCrop("Chilli",   "leek",       "Leek");
        updated += RenameCrop("Lavender", "wheat",      "Wheat");

        // ── Assign models ─────────────────────────────────────────────────────
        updated += AssignCrop("Carrot",     new[]
        {
            PackBase + "/Carrot_Plant_Sapling.prefab",
            PackBase + "/Carrot_Plant_Young.prefab",
            PackBase + "/Carrot_Plant_Ripe_A.prefab",
            PackBase + "/Carrot_Plant_Ripe_B.prefab",
        });

        updated += AssignCrop("Potato",     new[]
        {
            PackBase + "/Potato_Sprouting_A.prefab",
            PackBase + "/Potato_Plant_Young_A.prefab",
            PackBase + "/Potato_Plant_Flowering_A.prefab",
            PackBase + "/Potato_Plant_Ripe_A.prefab",
        });

        updated += AssignCrop("Sunflower",  new[]
        {
            PackBase + "/Sunflower_Plant_Young.prefab",
            PackBase + "/Sunflower_Plant_Mature_A.prefab",
            PackBase + "/Sunflower_Plant_Flowering.prefab",
            PackBase + "/Sunflower_Plant_Ripe_A.prefab",
        });

        updated += AssignCrop("Tomato",     new[]
        {
            PackBase + "/Tomato_Sprout_A.prefab",
            PackBase + "/Tomato_Mature_Single_A.prefab",
            PackBase + "/Tomato_Mature_Group_A.prefab",
            PackBase + "/Tomato_Stakes_B.prefab",
        });

        updated += AssignCrop("Pumpkin",    new[]
        {
            PackBase + "/Pumpkin_Sprout_A.prefab",
            PackBase + "/Pumpkin_Flowering_A.prefab",
            PackBase + "/Pumpkin_D.prefab",
            PackBase + "/Pumpkin_Ripe_A.prefab",
        });

        updated += AssignCrop("Corn",       new[]
        {
            PackBase + "/Corn_A.prefab",
            PackBase + "/Corn_B.prefab",
            PackBase + "/Corn_E.prefab",
            PackBase + "/Corn_Ripe_Sweetcorn_A.prefab",
        });

        // Strawberry: 3 unique stages — seedling repeated for stages 0 and 1
        updated += AssignCrop("Strawberry", new[]
        {
            PackBase + "/Strawberry_Plant_Seedling.prefab",
            PackBase + "/Strawberry_Plant_Seedling.prefab",
            PackBase + "/Strawberry_Plant_Flowering.prefab",
            PackBase + "/Strawberry_Plant_Ripe_A.prefab",
        });

        // Renamed crops — asset files still named Grapes/Chilli/Lavender on disk
        updated += AssignCrop("Grapes",     new[]
        {
            PackBase + "/Watermelon_Plant_Seedling.prefab",
            PackBase + "/Watermelon_Plant_Sprout.prefab",
            PackBase + "/Watermelon_Plant_Mature.prefab",
            PackBase + "/Watermelon_Plant_Ripe.prefab",
        });

        updated += AssignCrop("Chilli",     new[]
        {
            PackBase + "/Leek_Seedling_A.prefab",
            PackBase + "/Leek_Sprout_A.prefab",
            PackBase + "/Leek_Mature_A.prefab",
            PackBase + "/Leek_Harvested_A.prefab",
        });

        updated += AssignCrop("Lavender",   new[]
        {
            PackBase + "/Wheat_Seedling_Square_1x1m_A.prefab",
            PackBase + "/Wheat_Sprout_Square_1x1m_A.prefab",
            PackBase + "/Wheat_Ripening_Square_1x1m_A.prefab",
            PackBase + "/Wheat_Mature_Square_1x1m_A.prefab",
        });

        AssetDatabase.SaveAssets();
        Debug.Log($"[CropModelAssigner] Done — {updated} operations completed.");
        EditorUtility.DisplayDialog("Crop Models Assigned",
            $"All 10 crops updated.\n\n" +
            "Renamed: Grapes -> Watermelon, Chilli -> Leek, Lavender -> Wheat\n\n" +
            "Note: update crop icons in each ScriptableObject Inspector if needed.",
            "OK");
    }

    [MenuItem("Tools/CozyFarm/Clear Crop Models")]
    public static void ClearCropModels()
    {
        string[] assetNames = { "Carrot", "Potato", "Sunflower", "Tomato", "Pumpkin",
                                "Corn", "Strawberry", "Grapes", "Chilli", "Lavender" };
        int cleared = 0;
        foreach (string name in assetNames)
        {
            string path = $"{CropsPath}/{name}.asset";
            var crop = AssetDatabase.LoadAssetAtPath<CropData>(path);
            if (crop == null) continue;
            var so = new SerializedObject(crop);
            var prefabsProp = so.FindProperty("growthStagePrefabs");
            for (int i = 0; i < prefabsProp.arraySize; i++)
                prefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = null;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(crop);
            cleared++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[CropModelAssigner] Cleared models from {cleared} crops.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Updates cropId and cropName fields on a CropData asset.</summary>
    private static int RenameCrop(string assetName, string newId, string newDisplayName)
    {
        string assetPath = $"{CropsPath}/{assetName}.asset";
        var crop = AssetDatabase.LoadAssetAtPath<CropData>(assetPath);
        if (crop == null)
        {
            Debug.LogWarning($"[CropModelAssigner] Could not find {assetPath} for rename");
            return 0;
        }

        var so = new SerializedObject(crop);
        so.FindProperty("cropId").stringValue   = newId;
        so.FindProperty("cropName").stringValue = newDisplayName;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(crop);
        Debug.Log($"[CropModelAssigner] Renamed {assetName} -> {newDisplayName} (id: {newId})");
        return 1;
    }

    private static int AssignCrop(string assetName, string[] prefabPaths)
    {
        string assetPath = $"{CropsPath}/{assetName}.asset";
        var crop = AssetDatabase.LoadAssetAtPath<CropData>(assetPath);
        if (crop == null)
        {
            Debug.LogWarning($"[CropModelAssigner] Could not find {assetPath}");
            return 0;
        }

        var so = new SerializedObject(crop);
        var prefabsProp = so.FindProperty("growthStagePrefabs");
        prefabsProp.arraySize = 4;

        bool anyMissing = false;
        for (int i = 0; i < prefabPaths.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
            if (prefab == null)
            {
                Debug.LogWarning($"[CropModelAssigner] Missing prefab: {prefabPaths[i]}");
                anyMissing = true;
            }
            prefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(crop);

        string status = anyMissing ? "(some prefabs missing)" : "OK";
        Debug.Log($"[CropModelAssigner] {assetName} -> {status}");
        return 1;
    }
}
