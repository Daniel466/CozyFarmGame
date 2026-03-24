using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to auto-generate all BuildingData and BuildingDatabase assets.
/// Run via: Tools > CozyFarm > Generate Building Assets
/// </summary>
public class BuildingAssetGenerator
{
    [MenuItem("Tools/CozyFarm/Generate Building Assets")]
    public static void GenerateBuildingAssets()
    {
        string path = "Assets/_Project/ScriptableObjects/Buildings";

        // Define all buildings
        var buildings = new[]
        {
            new BuildingDef("barn",         "Barn",          BuildingType.Functional,  1,    0, 2, 2, new Color(0.72f, 0.25f, 0.18f), 10),
            new BuildingDef("watering_well","Watering Well", BuildingType.Functional,  2,  200, 1, 1, new Color(0.40f, 0.60f, 0.90f), 10),
            new BuildingDef("greenhouse",   "Greenhouse",    BuildingType.Functional,  6,  800, 3, 2, new Color(0.70f, 0.95f, 0.70f), 10),
            new BuildingDef("silo",         "Silo",          BuildingType.Functional,  8,  600, 1, 2, new Color(0.80f, 0.75f, 0.55f), 10),
            new BuildingDef("market_stall", "Market Stall",  BuildingType.Functional, 10, 1000, 2, 2, new Color(0.90f, 0.65f, 0.20f), 10, 120f, 0.1f),
            new BuildingDef("scarecrow",    "Scarecrow",     BuildingType.Decoration,  1,   50, 1, 1, new Color(0.80f, 0.60f, 0.20f),  3),
            new BuildingDef("wooden_fence", "Wooden Fence",  BuildingType.Decoration,  1,   20, 1, 1, new Color(0.55f, 0.35f, 0.15f),  3),
            new BuildingDef("flower_bed",   "Flower Bed",    BuildingType.Decoration,  1,   30, 1, 1, new Color(0.95f, 0.40f, 0.60f),  3),
            new BuildingDef("stone_path",   "Stone Path",    BuildingType.Decoration,  3,   15, 1, 1, new Color(0.60f, 0.60f, 0.60f),  3),
            new BuildingDef("lantern",      "Lantern",       BuildingType.Decoration,  9,   80, 1, 1, new Color(1.00f, 0.90f, 0.40f),  3),
            new BuildingDef("windmill",     "Windmill",      BuildingType.Decoration, 10,  300, 2, 2, new Color(0.85f, 0.80f, 0.70f), 10),
        };

        var createdAssets = new System.Collections.Generic.List<BuildingData>();

        foreach (var def in buildings)
        {
            string assetPath = $"{path}/{def.name.Replace(" ", "")}.asset";

            // Skip if already exists
            var existing = AssetDatabase.LoadAssetAtPath<BuildingData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[BuildingAssetGenerator] Skipped {def.name} — already exists.");
                createdAssets.Add(existing);
                continue;
            }

            var asset = ScriptableObject.CreateInstance<BuildingData>();

            // Use SerializedObject to set private fields
            var so = new SerializedObject(asset);
            so.FindProperty("buildingId").stringValue = def.id;
            so.FindProperty("buildingName").stringValue = def.name;
            so.FindProperty("buildingType").enumValueIndex = (int)def.type;
            so.FindProperty("unlockLevel").intValue = def.unlockLevel;
            so.FindProperty("cost").intValue = def.cost;
            so.FindProperty("size").vector2IntValue = new Vector2Int(def.sizeX, def.sizeY);
            so.FindProperty("placeholderColor").colorValue = def.color;
            so.FindProperty("placeXP").intValue = def.placeXP;
            so.FindProperty("autoSellInterval").floatValue = def.autoSellInterval;
            so.FindProperty("autoSellBonus").floatValue = def.autoSellBonus;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(asset, assetPath);
            createdAssets.Add(asset);
            Debug.Log($"[BuildingAssetGenerator] Created {def.name}");
        }

        // Create or update BuildingDatabase
        string dbPath = $"{path}/BuildingDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<BuildingDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        var dbSO = new SerializedObject(db);
        var buildingsList = dbSO.FindProperty("buildings");
        buildingsList.ClearArray();
        for (int i = 0; i < createdAssets.Count; i++)
        {
            buildingsList.InsertArrayElementAtIndex(i);
            buildingsList.GetArrayElementAtIndex(i).objectReferenceValue = createdAssets[i];
        }
        dbSO.ApplyModifiedProperties();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("✅ Done!",
            $"Generated {createdAssets.Count} building assets and updated BuildingDatabase!", "OK");

        Debug.Log($"[BuildingAssetGenerator] Done! {createdAssets.Count} assets created/updated.");
    }

    private struct BuildingDef
    {
        public string id, name;
        public BuildingType type;
        public int unlockLevel, cost, sizeX, sizeY, placeXP;
        public Color color;
        public float autoSellInterval, autoSellBonus;

        public BuildingDef(string id, string name, BuildingType type,
            int unlockLevel, int cost, int sizeX, int sizeY, Color color, int placeXP,
            float autoSellInterval = 0f, float autoSellBonus = 0.1f)
        {
            this.id = id; this.name = name; this.type = type;
            this.unlockLevel = unlockLevel; this.cost = cost;
            this.sizeX = sizeX; this.sizeY = sizeY;
            this.color = color; this.placeXP = placeXP;
            this.autoSellInterval = autoSellInterval;
            this.autoSellBonus = autoSellBonus;
        }
    }
}
