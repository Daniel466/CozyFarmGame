using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to assign polyperfect 3D models to CropData ScriptableObjects.
/// Tools > CozyFarm > Assign Crop Models
/// </summary>
public class CropModelAssigner : EditorWindow
{
    private const string FOOD_PATH = "Assets/polyperfect/Low Poly Ultimate Pack/_M/Meshes_M/Food_M";
    private const string FARM_PATH = "Assets/polyperfect/Low Poly Ultimate Pack/_M/Meshes_M/Farm_M";
    private const string NATURE_PATH = "Assets/polyperfect/Low Poly Ultimate Pack/_M/Meshes_M/Nature_M";
    private const string CROPS_PATH = "Assets/_Project/ScriptableObjects/Crops";

    [MenuItem("Tools/CozyFarm/Assign Crop Models")]
    public static void AssignCropModels()
    {
        int assigned = 0;
        int skipped = 0;

        // Map: cropId -> array of mesh names per growth stage [planted, sprout, growing, ready]
        // We use the same mesh scaled up per stage since polyperfect has single harvest meshes
        var cropToMeshes = new System.Collections.Generic.Dictionary<string, (string path, string[] meshNames)>
        {
            { "carrot",     (FOOD_PATH,   new[] { "SM_Carrot",   "SM_Carrot",   "SM_Carrot",   "SM_Carrot"   }) },
            { "tomato",     (FOOD_PATH,   new[] { "SM_Tomato",   "SM_Tomato",   "SM_Tomato",   "SM_Tomato"   }) },
            { "corn",       (FOOD_PATH,   new[] { "SM_Corn",     "SM_Corn",     "SM_Corn",     "SM_Corn"     }) },
            { "pumpkin",    (FOOD_PATH,   new[] { "SM_Melon",    "SM_Melon",    "SM_Melon",    "SM_Melon"    }) },
            { "potato",     (FOOD_PATH,   new[] { "SM_Egg",      "SM_Egg",      "SM_Egg",      "SM_Egg"      }) },
            { "strawberry", (FOOD_PATH,   new[] { "SM_Apple",    "SM_Apple",    "SM_Apple",    "SM_Apple"    }) },
            { "grapes",     (FOOD_PATH,   new[] { "SM_Honey",    "SM_Honey",    "SM_Honey",    "SM_Honey"    }) },
            { "chilli",     (FOOD_PATH,   new[] { "SM_Eggplant", "SM_Eggplant", "SM_Eggplant", "SM_Eggplant" }) },
            { "sunflower",  (FARM_PATH,   new[] { "SM_Haystack", "SM_Haystack", "SM_Haystack", "SM_Haystack" }) },
            { "lavender",   (FARM_PATH,   new[] { "SM_Hay_Pile", "SM_Hay_Pile", "SM_Hay_Pile", "SM_Hay_Pile" }) },
        };

        // Find all CropData assets
        string[] guids = AssetDatabase.FindAssets("t:CropData", new[] { CROPS_PATH });

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Assign Crop Models",
                $"No CropData assets found in {CROPS_PATH}\nMake sure your crop ScriptableObjects are in that folder.",
                "OK");
            return;
        }

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CropData crop = AssetDatabase.LoadAssetAtPath<CropData>(assetPath);

            if (crop == null) continue;

            string cropId = crop.CropId.ToLower().Trim();

            if (!cropToMeshes.ContainsKey(cropId))
            {
                Debug.LogWarning($"[CropModelAssigner] No mesh mapping for cropId '{cropId}' — skipping {crop.name}");
                skipped++;
                continue;
            }

            var (basePath, meshNames) = cropToMeshes[cropId];
            var prefabs = new GameObject[4];
            bool anyFound = false;

            for (int i = 0; i < 4; i++)
            {
                string meshPath = $"{basePath}/{meshNames[i]}.fbx";
                GameObject mesh = AssetDatabase.LoadAssetAtPath<GameObject>(meshPath);

                if (mesh != null)
                {
                    prefabs[i] = mesh;
                    anyFound = true;
                }
                else
                {
                    Debug.LogWarning($"[CropModelAssigner] Could not find mesh at: {meshPath}");
                }
            }

            if (anyFound)
            {
                // Use SerializedObject to modify the ScriptableObject
                SerializedObject so = new SerializedObject(crop);
                SerializedProperty stageProp = so.FindProperty("growthStagePrefabs");

                if (stageProp != null && stageProp.isArray)
                {
                    stageProp.arraySize = 4;
                    for (int i = 0; i < 4; i++)
                    {
                        stageProp.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
                    }
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(crop);
                    Debug.Log($"[CropModelAssigner] Assigned models to {crop.CropName} ({cropId})");
                    assigned++;
                }
                else
                {
                    Debug.LogWarning($"[CropModelAssigner] Could not find growthStagePrefabs property on {crop.name} — check the field name in CropData.cs");
                    skipped++;
                }
            }
            else
            {
                skipped++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Assign Crop Models Complete",
            $"Done!\n\nAssigned: {assigned} crops\nSkipped: {skipped} crops\n\nCheck the Console for details.",
            "OK");
    }

    [MenuItem("Tools/CozyFarm/Clear Crop Models")]
    public static void ClearCropModels()
    {
        if (!EditorUtility.DisplayDialog("Clear Crop Models",
            "This will remove all assigned growth stage prefabs from all CropData assets. Are you sure?",
            "Yes, clear all", "Cancel"))
            return;

        string[] guids = AssetDatabase.FindAssets("t:CropData", new[] { CROPS_PATH });
        int cleared = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CropData crop = AssetDatabase.LoadAssetAtPath<CropData>(assetPath);
            if (crop == null) continue;

            SerializedObject so = new SerializedObject(crop);
            SerializedProperty stageProp = so.FindProperty("growthStagePrefabs");

            if (stageProp != null && stageProp.isArray)
            {
                for (int i = 0; i < stageProp.arraySize; i++)
                    stageProp.GetArrayElementAtIndex(i).objectReferenceValue = null;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(crop);
                cleared++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Clear Complete", $"Cleared models from {cleared} crops.", "OK");
    }
}
