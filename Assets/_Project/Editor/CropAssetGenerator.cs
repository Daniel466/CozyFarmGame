using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to auto-generate all CropData and CropDatabase assets.
/// Run via: Tools > CozyFarm > Generate Crop Assets
/// </summary>
public class CropAssetGenerator
{
    [MenuItem("Tools/CozyFarm/Generate Crop Assets")]
    public static void GenerateCropAssets()
    {
        string path = "Assets/_Project/ScriptableObjects/Crops";

        var crops = new[]
        {
            new CropDef("carrot",     "Carrot",     1,   5,  10,  300),
            new CropDef("sunflower",  "Sunflower",  1,   8,  15,  480),
            new CropDef("tomato",     "Tomato",     1,  12,  25,  720),
            new CropDef("potato",     "Potato",     1,  10,  20,  600),
            new CropDef("strawberry", "Strawberry", 3,  20,  35,  900),
            new CropDef("corn",       "Corn",       5,  30,  50, 1200),
            new CropDef("pumpkin",    "Pumpkin",    7,  45,  80, 1800),
            new CropDef("grapes",     "Grapes",     9,  38,  65, 1500),
            new CropDef("chilli",     "Chilli",    11,  32,  55, 1080),
            new CropDef("lavender",   "Lavender",  14,  60, 100, 2100),
        };

        var createdAssets = new System.Collections.Generic.List<CropData>();

        foreach (var def in crops)
        {
            string assetPath = $"{path}/{def.id}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<CropData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[CropAssetGenerator] Skipped {def.name} — already exists.");
                createdAssets.Add(existing);
                continue;
            }

            var asset = ScriptableObject.CreateInstance<CropData>();
            var so = new SerializedObject(asset);
            so.FindProperty("cropId").stringValue = def.id;
            so.FindProperty("cropName").stringValue = def.name;
            so.FindProperty("unlockLevel").intValue = def.unlockLevel;
            so.FindProperty("seedCost").intValue = def.seedCost;
            so.FindProperty("sellValue").intValue = def.sellValue;
            so.FindProperty("growTimeSeconds").floatValue = def.growTime;
            so.FindProperty("harvestXP").intValue = 5;
            so.FindProperty("plantXP").intValue = 2;
            so.FindProperty("waterXP").intValue = 1;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(asset, assetPath);
            createdAssets.Add(asset);
            Debug.Log($"[CropAssetGenerator] Created {def.name}");
        }

        // Create or update CropDatabase
        string dbPath = "Assets/_Project/ScriptableObjects/CropDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<CropDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<CropDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        var dbSO = new SerializedObject(db);
        var cropsList = dbSO.FindProperty("crops");
        cropsList.ClearArray();
        for (int i = 0; i < createdAssets.Count; i++)
        {
            cropsList.InsertArrayElementAtIndex(i);
            cropsList.GetArrayElementAtIndex(i).objectReferenceValue = createdAssets[i];
        }
        dbSO.ApplyModifiedProperties();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("✅ Done!",
            $"Generated {createdAssets.Count} crop assets and updated CropDatabase!", "OK");

        Debug.Log($"[CropAssetGenerator] Done! {createdAssets.Count} assets created/updated.");
    }

    private struct CropDef
    {
        public string id, name;
        public int unlockLevel, seedCost, sellValue;
        public float growTime;

        public CropDef(string id, string name, int unlockLevel,
            int seedCost, int sellValue, float growTime)
        {
            this.id = id; this.name = name;
            this.unlockLevel = unlockLevel;
            this.seedCost = seedCost; this.sellValue = sellValue;
            this.growTime = growTime;
        }
    }
}
