using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles saving and loading the full game state to/from a local JSON file.
/// Save file lives at: Application.persistentDataPath/save.json
/// </summary>
public class SaveManager : MonoBehaviour
{
    [SerializeField] private CropDatabase cropDatabase;
    [SerializeField] private BuildingDatabase buildingDatabase;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public void SaveGame()
    {
        var data = new GameSaveData();

        // Economy + Progression
        data.coins = GameManager.Instance.Economy.Coins;
        data.xp    = GameManager.Instance.Progression.CurrentXP;
        data.level = GameManager.Instance.Progression.CurrentLevel;

        // Timestamp — used by offline growth system on next load
        data.saveTimestamp = DateTime.UtcNow.ToString("O");

        // Inventory
        data.inventoryItems = new List<InventorySaveItem>();
        foreach (var kvp in GameManager.Instance.Inventory.GetAllItems())
        {
            data.inventoryItems.Add(new InventorySaveItem
            {
                cropId   = kvp.Key,
                quantity = kvp.Value.quantity
            });
        }

        // Farm tiles — only save planted tiles
        data.tiles = new List<FarmTileSaveData>();
        foreach (var tile in GameManager.Instance.FarmGrid.GetAllTiles().Values)
        {
            if (tile.IsPlanted)
                data.tiles.Add(tile.ToSaveData());
        }

        // Buildings — origin entries only (multi-tile aliases have gameObject == null)
        data.buildings = new List<BuildingSaveData>();
        if (BuildingManager.Instance != null)
        {
            foreach (var kvp in BuildingManager.Instance.GetAllBuildings())
            {
                var pb = kvp.Value;
                if (pb.gameObject != null)
                {
                    data.buildings.Add(new BuildingSaveData
                    {
                        buildingId = pb.data.BuildingId,
                        coordX     = kvp.Key.x,
                        coordY     = kvp.Key.y,
                        rotation   = pb.rotation
                    });
                }
            }
        }

        // Dog happiness
        data.dogHappiness = DogManager.Instance?.ActiveDog?.Happiness ?? 0.5f;

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
        Debug.Log($"[Save] Saved — coins:{data.coins} level:{data.level} " +
                  $"tiles:{data.tiles.Count} inventory:{data.inventoryItems.Count} buildings:{data.buildings.Count}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[Save] No save file found — starting fresh.");
            return;
        }

        var data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath));

        // Economy + Progression
        GameManager.Instance.Economy.SetCoins(data.coins);
        GameManager.Instance.Progression.SetState(data.xp, data.level);

        // Inventory
        if (data.inventoryItems != null)
        {
            foreach (var item in data.inventoryItems)
            {
                CropData crop = cropDatabase?.GetCropById(item.cropId);
                if (crop != null)
                    GameManager.Instance.Inventory.AddItem(crop, item.quantity);
                else
                    Debug.LogWarning($"[Save] Unknown crop id '{item.cropId}' — inventory item skipped.");
            }
        }

        // Calculate how long the game has been closed
        float offlineSeconds = 0f;
        if (!string.IsNullOrEmpty(data.saveTimestamp) &&
            DateTime.TryParseExact(data.saveTimestamp, "O", null,
                DateTimeStyles.RoundtripKind, out DateTime savedTime))
        {
            offlineSeconds = (float)(DateTime.UtcNow - savedTime).TotalSeconds;
            offlineSeconds = Mathf.Clamp(offlineSeconds, 0f, 7f * 24f * 3600f); // cap at 7 days
            Debug.Log($"[Save] Offline for {offlineSeconds:F0}s — applying growth.");
        }

        // Farm tiles — restore state, apply offline growth, then spawn visuals
        int readyCount = 0;
        int grewCount  = 0;
        if (data.tiles != null)
        {
            var grid      = GameManager.Instance.FarmGrid;
            float speedMult = FarmingManager.Instance?.GrowthSpeedMultiplier ?? 1f;

            foreach (var tileData in data.tiles)
            {
                var coord = new Vector2Int(tileData.coordX, tileData.coordY);
                FarmTile tile = grid.GetTile(coord);
                if (tile == null) continue;

                CropData crop = string.IsNullOrEmpty(tileData.cropId)
                    ? null
                    : cropDatabase?.GetCropById(tileData.cropId);

                tile.LoadFromSaveData(tileData, crop);

                if (tile.IsPlanted && offlineSeconds > 0f)
                {
                    float before = tile.GrowthProgress;
                    tile.ApplyOfflineGrowth(offlineSeconds, speedMult);
                    if (tile.GrowthProgress > before) grewCount++;
                    if (tile.IsReadyToHarvest)        readyCount++;
                }

                if (tile.IsPlanted)
                    FarmingManager.Instance?.RestoreFromSave(coord, tile);
            }
        }

        // Buildings
        if (data.buildings != null && buildingDatabase != null)
        {
            foreach (var bData in data.buildings)
            {
                BuildingData bd = buildingDatabase.GetById(bData.buildingId);
                if (bd == null)
                {
                    Debug.LogWarning($"[Save] Unknown building id '{bData.buildingId}' — skipped.");
                    continue;
                }
                BuildingManager.Instance?.RestoreBuilding(bd, new Vector2Int(bData.coordX, bData.coordY), bData.rotation);
            }
        }

        // Restore dog happiness after buildings (doghouse spawns dog during RestoreBuilding)
        if (DogManager.Instance?.ActiveDog != null)
            DogManager.Instance.ActiveDog.SetHappiness(data.dogHappiness);

        Debug.Log($"[Save] Loaded — coins:{data.coins} level:{data.level} " +
                  $"tiles:{data.tiles?.Count} inventory:{data.inventoryItems?.Count} buildings:{data.buildings?.Count}");

        // Notify the player about offline growth (defer one frame so HUDManager is ready)
        if (offlineSeconds > 60f)
            StartCoroutine(ShowOfflineNotification(readyCount, grewCount));
    }

    private IEnumerator ShowOfflineNotification(int ready, int grew)
    {
        yield return new WaitForSeconds(0.5f);
        if (ready > 0)
            HUDManager.Instance?.ShowNotification(
                ready == 1 ? "1 crop ready to harvest!" : $"{ready} crops ready to harvest!", 4f);
        else if (grew > 0)
            HUDManager.Instance?.ShowNotification("Your crops grew while you were away!", 3f);
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
        Debug.Log("[Save] Save file deleted.");
    }
}

[System.Serializable]
public class GameSaveData
{
    public int    coins;
    public int    xp;
    public int    level;
    public string saveTimestamp;
    public float  dogHappiness = 0.5f;
    public List<InventorySaveItem> inventoryItems;
    public List<FarmTileSaveData>  tiles;
    public List<BuildingSaveData>  buildings;
}

[System.Serializable]
public class InventorySaveItem
{
    public string cropId;
    public int    quantity;
}

[System.Serializable]
public class BuildingSaveData
{
    public string buildingId;
    public int    coordX;
    public int    coordY;
    public int    rotation;
}
