using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to assign polyperfect prefabs to CropData ScriptableObjects.
/// Tools > CozyFarm > Assign Crop Models
/// </summary>
public class CropModelAssigner : EditorWindow
{
    private const string FOOD_PREFABS = "Assets/polyperfect/Low Poly Ultimate Pack/_M/Prefabs_M/Food_M";
    private const string FARM_PREFABS = "Assets/polyperfect/Low Poly Ultimate Pack/_M/Prefabs_M/Farm_M";
    private const string CROPS_PATH   = "Assets/_Project/ScriptableObjects/Crops";

    [MenuItem("Tools/CozyFarm/Assign Crop Models")]
    public static void AssignCropModels()
    {
        // Map: cropId -> (folder, prefab name)
        // Using same prefab for all 4 stages — CropGrowthVisual scales them up per stage
        var cropMap = new System.Collections.Generic.Dictionary<string, (string folder, string prefab)>
        {
            { "carrot",     (FOOD_PREFABS, "Carrot")    },
            { "tomato",     (FOOD_PREFABS, "Tomato")    },
            { "corn",       (FOOD_PREFABS, "Corn")      },
            { "pumpkin",    (FOOD_PREFABS, "Melon")     },
            { "potato",     (FOOD_PREFABS, "Egg")       },
            { "strawberry", (FOOD_PREFABS, "Apple")     },
            { "grapes",     (FOOD_PREFABS, "Honey")     },
            { "chilli",     (FOOD_PREFABS, "Eggplant")  },
            { "sunflower",  (FARM_PREFABS, "Haystack")  },
            { "lavender",   (FARM_PREFABS, "Hay_Pile")  },
        };

        string[] guids = AssetDatabase.FindAssets("t:CropData", new[] { CROPS_PATH });

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Assign Crop Models",
                $"No CropData assets found in {CROPS_PATH}", "OK");
            return;
        }

        int assigned = 0;
        int skipped  = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CropData crop = AssetDatabase.LoadAssetAtPath<CropData>(assetPath);
            if (crop == null) continue;

            string cropId = crop.CropId.ToLower().Trim();

            if (!cropMap.ContainsKey(cropId))
            {
                Debug.LogWarning($"[CropModelAssigner] No mapping for cropId '{cropId}' — skipping {crop.name}");
                skipped++;
                continue;
            }

            var (folder, prefabName) = cropMap[cropId];
            string prefabPath = $"{folder}/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning($"[CropModelAssigner] Prefab not found at: {prefabPath}");
                skipped++;
                continue;
            }

            // Assign same prefab to all 4 growth stages
            // CropGrowthVisual scales them: 0.3x, 0.5x, 0.8x, 1.2x
            SerializedObject so = new SerializedObject(crop);
            SerializedProperty stageProp = so.FindProperty("growthStagePrefabs");

            if (stageProp == null || !stageProp.isArray)
            {
                Debug.LogWarning($"[CropModelAssigner] growthStagePrefabs not found on {crop.name}");
                skipped++;
                continue;
            }

            stageProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                stageProp.GetArrayElementAtIndex(i).objectReferenceValue = prefab;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(crop);
            Debug.Log($"[CropModelAssigner] Assigned {prefabName} to {crop.CropName}");
            assigned++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Assign Crop Models Complete",
            $"Done!\n\nAssigned: {assigned} crops\nSkipped:  {skipped} crops\n\nCheck Console for details.",
            "OK");
    }

    [MenuItem("Tools/CozyFarm/Clear Crop Models")]
    public static void ClearCropModels()
    {
        if (!EditorUtility.DisplayDialog("Clear Crop Models",
            "Remove all assigned growth stage prefabs from all CropData assets?",
            "Yes clear all", "Cancel")) return;

        string[] guids = AssetDatabase.FindAssets("t:CropData", new[] { CROPS_PATH });
        int cleared = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CropData crop = AssetDatabase.LoadAssetAtPath<CropData>(assetPath);
            if (crop == null) continue;

            SerializedObject so = new SerializedObject(crop);
            SerializedProperty stageProp = so.FindProperty("growthStagePrefabs");
            if (stageProp == null || !stageProp.isArray) continue;

            for (int i = 0; i < stageProp.arraySize; i++)
                stageProp.GetArrayElementAtIndex(i).objectReferenceValue = null;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(crop);
            cleared++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Clear Complete", $"Cleared models from {cleared} crops.", "OK");
    }
}