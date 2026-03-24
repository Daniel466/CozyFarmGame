using UnityEngine;
using UnityEditor;

/// <summary>
/// Assigns Synty POLYGON Farm prefabs to all BuildingData assets.
/// Run via: Tools > CozyFarm > Assign Building Models (Synty)
/// </summary>
public class BuildingModelAssigner
{
    private const string SYNTY_BUILDINGS  = "Assets/Synty/PolygonFarm/Prefabs/Buildings";
    private const string SYNTY_PROPS      = "Assets/Synty/PolygonFarm/Prefabs/Props";
    private const string SYNTY_CHARACTERS = "Assets/Synty/PolygonFarm/Prefabs/Characters";
    private const string BUILDINGS_PATH   = "Assets/_Project/ScriptableObjects/Buildings";

    // buildingId -> (folder, prefabName)
    private static readonly System.Collections.Generic.Dictionary<string, (string folder, string prefab)> BuildingMap =
        new System.Collections.Generic.Dictionary<string, (string, string)>
    {
        { "barn",          (SYNTY_BUILDINGS,  "SM_Bld_Barn_01")          },
        { "watering_well", (SYNTY_PROPS,      "SM_Prop_Well_01")         },
        { "greenhouse",    (SYNTY_BUILDINGS,  "SM_Bld_Greenhouse_01")    },
        { "silo",          (SYNTY_BUILDINGS,  "SM_Bld_Silo_01")          },
        { "market_stall",  (SYNTY_BUILDINGS,  "SM_Bld_ProduceStand_01")  },
        { "scarecrow",     (SYNTY_CHARACTERS, "SM_Chr_Scarecrow_01")     },
        { "wooden_fence",  (SYNTY_PROPS,      "SM_Prop_Fence_Wood_01")   },
        { "windmill",      (SYNTY_PROPS,      "SM_Prop_Windmill_01")     },
    };

    [MenuItem("Tools/CozyFarm/Assign Building Models (Synty)")]
    public static void AssignBuildingModels()
    {
        string[] guids = AssetDatabase.FindAssets("t:BuildingData", new[] { BUILDINGS_PATH });
        if (guids.Length == 0) { EditorUtility.DisplayDialog("Assign Building Models", "No BuildingData assets found!", "OK"); return; }

        int assigned = 0, skipped = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            BuildingData building = AssetDatabase.LoadAssetAtPath<BuildingData>(assetPath);
            if (building == null) continue;

            string id = building.BuildingId.ToLower().Trim();
            if (!BuildingMap.TryGetValue(id, out var entry))
            {
                Debug.Log($"[BuildingModelAssigner] No Synty mapping for '{id}' — leaving as placeholder.");
                skipped++; continue;
            }

            string prefabPath = $"{entry.folder}/{entry.prefab}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[BuildingModelAssigner] Not found: {prefabPath}");
                skipped++; continue;
            }

            var so = new SerializedObject(building);
            so.FindProperty("prefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(building);

            Debug.Log($"[BuildingModelAssigner] {building.BuildingName} -> {entry.prefab}");
            assigned++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done!", $"Assigned: {assigned}\nSkipped: {skipped} (no mapping or kept as placeholder)", "OK");
    }
}
