using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools > CozyFarm > Setup Crop Seasons and Growth Days
///
/// Sets GrowingSeason and GrowthDays on all 10 CropData assets
/// as defined in the GDD Phase 1 crop table.
/// </summary>
public static class CropSeasonSetup
{
    private const string CropsPath = "Assets/_Project/ScriptableObjects/Crops";

    [MenuItem("Tools/CozyFarm/Setup Crop Seasons and Growth Days")]
    public static void SetupAll()
    {
        // (assetName, season flags, growthDays, seedCost, sellValue)
        var crops = new (string name, GrowingSeason season, int days, int seed, int sell)[]
        {
            ("Carrot",     GrowingSeason.Spring,               4,  20,  50),
            ("Potato",     GrowingSeason.Spring,               6,  30,  80),
            ("Strawberry", GrowingSeason.Spring,               8,  50, 120),
            ("Tomato",     GrowingSeason.Summer,               5,  35,  90),
            ("Corn",       GrowingSeason.Summer,               7,  45, 110),
            ("Watermelon", GrowingSeason.Summer,              10,  80, 200),
            ("Pumpkin",    GrowingSeason.Fall,                 8,  60, 150),
            ("Sunflower",  GrowingSeason.Fall,                 5,  30,  75),
            ("Wheat",      GrowingSeason.Fall,                 4,  15,  40),
            // Grapes.asset was renamed to Watermelon in CropModelAssigner
            // Chilli.asset  was renamed to Leek
            // Lavender.asset was renamed to Wheat
            // Asset file names on disk are still the old names:
            ("Grapes",     GrowingSeason.Summer,              10,  80, 200), // = Watermelon asset
            ("Chilli",     GrowingSeason.Fall | GrowingSeason.Winter, 6, 25, 65), // = Leek asset
            ("Lavender",   GrowingSeason.Fall,                 4,  15,  40), // = Wheat asset
        };

        int updated = 0;
        foreach (var (name, season, days, seed, sell) in crops)
        {
            string path = $"{CropsPath}/{name}.asset";
            var crop = AssetDatabase.LoadAssetAtPath<CropData>(path);
            if (crop == null)
            {
                Debug.LogWarning($"[CropSeasonSetup] Not found: {path}");
                continue;
            }

            var so = new SerializedObject(crop);
            so.FindProperty("growingSeason").intValue = (int)season;
            so.FindProperty("growthDays").intValue    = days;
            so.FindProperty("seedCost").intValue      = seed;
            so.FindProperty("sellValue").intValue     = sell;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(crop);
            updated++;
            Debug.Log($"[CropSeasonSetup] {name}: {season}, {days} days, seed={seed}, sell={sell}");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Crop Seasons Set",
            $"{updated} crop assets updated.\n\n" +
            "Spring:  Carrot (4d), Potato (6d), Strawberry (8d)\n" +
            "Summer:  Tomato (5d), Corn (7d), Watermelon (10d)\n" +
            "Fall:    Pumpkin (8d), Sunflower (5d), Wheat (4d)\n" +
            "Fall/Winter: Leek (6d)",
            "OK");
    }
}
