using UnityEngine;
using UnityEditor;

public class CropModelAssigner : EditorWindow
{
    private const string FOOD_PREFABS    = "Assets/polyperfect/Low Poly Ultimate Pack/_M/Prefabs_M/Food_M";
    private const string FLOWERS_PREFABS = "Assets/polyperfect/Low Poly Ultimate Pack/_M/Prefabs_M/Nature_M/Flowers_M";
    private const string NATURE_PREFABS  = "Assets/polyperfect/Low Poly Ultimate Pack/_M/Prefabs_M/Nature_M";
    private const string CROPS_PATH      = "Assets/_Project/ScriptableObjects/Crops";

    private static readonly System.Collections.Generic.Dictionary<string, (string folder, string prefab, float scale, float rotY)> CropMap =
        new System.Collections.Generic.Dictionary<string, (string, string, float, float)>
    {
        { "carrot",     (FOOD_PREFABS,    "Carrot",               3.0f,  0f   ) },
        { "sunflower",  (FLOWERS_PREFABS, "Sunflower",            0.7f,  -90f ) },
        { "tomato",     (FOOD_PREFABS,    "Tomato",               4.0f,  0f   ) },
        { "potato",     (FOOD_PREFABS,    "Bread_Round",          3.0f,  0f   ) },
        { "strawberry", (FOOD_PREFABS,    "Apple",                3.0f,  0f   ) },
        { "corn",       (FOOD_PREFABS,    "Corn",                 2.5f,  0f   ) },
        { "pumpkin",    (FLOWERS_PREFABS, "Pumkin",               1.0f,  0f   ) },
        { "grapes",     (NATURE_PREFABS,  "Grapes_Purple_Empire", 2.0f,  0f   ) },
        { "chilli",     (FOOD_PREFABS,    "Eggplant",             4.0f,  0f   ) },
        { "lavender",   (FLOWERS_PREFABS, "Carnations",           0.8f,  0f   ) },
    };

    [MenuItem("Tools/CozyFarm/Assign Crop Models")]
    public static void AssignCropModels()
    {
        string[] guids = AssetDatabase.FindAssets("t:CropData", new[] { CROPS_PATH });
        if (guids.Length == 0) { EditorUtility.DisplayDialog("Assign Crop Models", "No CropData assets found!", "OK"); return; }

        int assigned = 0, skipped = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CropData crop = AssetDatabase.LoadAssetAtPath<CropData>(assetPath);
            if (crop == null) continue;

            string cropId = crop.CropId.ToLower().Trim();
            if (!CropMap.ContainsKey(cropId)) { Debug.LogWarning($"[CropModelAssigner] No mapping for '{cropId}'"); skipped++; continue; }

            var (folder, prefabName, scale, rotY) = CropMap[cropId];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/{prefabName}.prefab");
            if (prefab == null) { Debug.LogWarning($"[CropModelAssigner] Not found: {folder}/{prefabName}.prefab"); skipped++; continue; }

            SerializedObject so = new SerializedObject(crop);

            SerializedProperty stageProp = so.FindProperty("growthStagePrefabs");
            if (stageProp == null || !stageProp.isArray) { skipped++; continue; }
            stageProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                stageProp.GetArrayElementAtIndex(i).objectReferenceValue = prefab;

            SerializedProperty scaleProp = so.FindProperty("modelBaseScale");
            if (scaleProp != null) scaleProp.floatValue = scale;

            SerializedProperty rotProp = so.FindProperty("modelRotationOffset");
            if (rotProp != null) rotProp.vector3Value = new Vector3(0f, rotY, 0f);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(crop);
            Debug.Log($"[CropModelAssigner] {crop.CropName} -> {prefabName} scale:{scale} rotY:{rotY}");
            assigned++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done!", $"Assigned: {assigned}\nSkipped: {skipped}", "OK");
    }

    [MenuItem("Tools/CozyFarm/Clear Crop Models")]
    public static void ClearCropModels()
    {
        if (!EditorUtility.DisplayDialog("Clear Crop Models", "Remove all assigned prefabs?", "Yes", "Cancel")) return;
        string[] guids = AssetDatabase.FindAssets("t:CropData", new[] { CROPS_PATH });
        int cleared = 0;
        foreach (string guid in guids)
        {
            CropData crop = AssetDatabase.LoadAssetAtPath<CropData>(AssetDatabase.GUIDToAssetPath(guid));
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
        EditorUtility.DisplayDialog("Done!", $"Cleared {cleared} crops.", "OK");
    }
}