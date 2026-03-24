using UnityEngine;
using UnityEditor;

public class CropModelAssigner : EditorWindow
{
    private const string SYNTY_PLANTS = "Assets/Synty/PolygonFarm/Prefabs/Plants";
    private const string CROPS_PATH   = "Assets/_Project/ScriptableObjects/Crops";

    // Per crop: stage0 (S), stage1 (M), stage2 (L), stage3 (Group/harvest), scale, rotY
    private static readonly System.Collections.Generic.Dictionary<string, (string s0, string s1, string s2, string s3, float scale, float rotY)> CropMap =
        new System.Collections.Generic.Dictionary<string, (string, string, string, string, float, float)>
    {
        { "carrot",     ("SM_Prop_Carrot_01",           "SM_Prop_Carrot_01",           "SM_Prop_Carrot_01_L",          "SM_Prop_Carrot_01_Group",      2.8f,  0f  ) },
        { "sunflower",  ("SM_Prop_Sunflower_01",        "SM_Prop_Sunflower_01",        "SM_Prop_Sunflower_01",         "SM_Prop_Sunflower_01",         1.8f, -90f ) },
        { "tomato",     ("SM_Prop_Tomato_01",           "SM_Prop_Tomato_01",           "SM_Prop_Tomato_01_L",          "SM_Prop_Tomato_01_L",          3.0f,  0f  ) },
        { "potato",     ("SM_Prop_Plant_Potato_01_S",   "SM_Prop_Plant_Potato_01_M",   "SM_Prop_Potato_01",            "SM_Prop_Potato_01_Group",      2.6f,  0f  ) },
        { "strawberry", ("SM_Prop_Strawberry_01_S",     "SM_Prop_Strawberry_01_S",     "SM_Prop_Strawberry_01_L",      "SM_Prop_Strawberry_01_Group",  2.8f,  0f  ) },
        { "corn",       ("SM_Prop_Plant_Corn_01_S",     "SM_Prop_Plant_Corn_01_S",     "SM_Prop_Plant_Corn_01_L",      "SM_Prop_Plant_Corn_01_L",      1.0f,  0f  ) },
        { "pumpkin",    ("SM_Prop_Pumpkin_01_M",        "SM_Prop_Pumpkin_01_M",        "SM_Prop_Pumpkin_01_L",         "SM_Prop_Pumpkin_01_L",         1.6f,  0f  ) },
        { "grapes",     ("SM_Prop_Plant_Bush_02_S",     "SM_Prop_Plant_Bush_02_S",     "SM_Prop_Plant_Bush_02_M",      "SM_Prop_Plant_Bush_02_M",      1.8f,  0f  ) },
        { "chilli",     ("SM_Prop_Chilli_01",           "SM_Prop_Chilli_01_M",         "SM_Prop_Chilli_01_L",          "SM_Prop_Chilli_01_Group",      3.0f,  0f  ) },
        { "lavender",   ("SM_Prop_Plant_Bush_03_S",     "SM_Prop_Plant_Bush_03_S",     "SM_Prop_Plant_Bush_03_M",      "SM_Prop_Plant_Bush_03_M",      1.8f,  0f  ) },
    };

    [MenuItem("Tools/CozyFarm/Assign Crop Models (Synty)")]
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
            if (!CropMap.TryGetValue(cropId, out var entry))
            {
                Debug.LogWarning($"[CropModelAssigner] No mapping for '{cropId}'");
                skipped++; continue;
            }

            string[] prefabNames = { entry.s0, entry.s1, entry.s2, entry.s3 };
            GameObject[] prefabs = new GameObject[4];
            bool allFound = true;

            for (int i = 0; i < 4; i++)
            {
                string path = $"{SYNTY_PLANTS}/{prefabNames[i]}.prefab";
                prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabs[i] == null)
                {
                    Debug.LogWarning($"[CropModelAssigner] Not found: {path}");
                    allFound = false;
                }
            }

            if (!allFound) { skipped++; continue; }

            SerializedObject so = new SerializedObject(crop);

            var stageProp = so.FindProperty("growthStagePrefabs");
            stageProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                stageProp.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];

            so.FindProperty("modelBaseScale").floatValue        = entry.scale;
            so.FindProperty("modelRotationOffset").vector3Value = new Vector3(0f, entry.rotY, 0f);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(crop);
            Debug.Log($"[CropModelAssigner] {crop.CropName} -> {entry.s0} / {entry.s1} / {entry.s2} / {entry.s3}");
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
            var stageProp = so.FindProperty("growthStagePrefabs");
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
