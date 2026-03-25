using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Saves and loads the full game state to/from a local JSON file.
/// Save path: Application.persistentDataPath/save.json
/// </summary>
public class SaveManager : MonoBehaviour
{
    [SerializeField] private CropDatabase     cropDatabase;
    [SerializeField] private BuildingDatabase buildingDatabase;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public void SaveGame()
    {
        var data = new GameSaveData();

        // Economy
        data.coins           = GameManager.Instance.Economy.Coins;
        data.lifetimeEarnings = GameManager.Instance.Economy.LifetimeEarnings;

        // Tools
        data.tools = GameManager.Instance.ToolManager?.ToSaveData() ?? new ToolSaveData();

        // Inventory
        data.inventoryItems = new List<InventorySaveItem>();
        foreach (var kvp in GameManager.Instance.Inventory.GetAllItems())
            data.inventoryItems.Add(new InventorySaveItem { cropId = kvp.Key, quantity = kvp.Value.quantity });

        // Farm tiles — only planted or tilled tiles
        data.tiles = new List<FarmTileSaveData>();
        foreach (var tile in GameManager.Instance.FarmGrid.GetAllTiles().Values)
            if (tile.IsPlanted || tile.IsTilled)
                data.tiles.Add(tile.ToSaveData());

        // Buildings
        data.buildings = new List<BuildingSaveData>();
        if (BuildingManager.Instance != null)
        {
            foreach (var kvp in BuildingManager.Instance.GetAllBuildings())
            {
                var pb = kvp.Value;
                if (pb.gameObject != null)
                    data.buildings.Add(new BuildingSaveData
                    {
                        buildingId = pb.data.BuildingId,
                        coordX     = kvp.Key.x,
                        coordY     = kvp.Key.y,
                        rotation   = pb.rotation
                    });
            }
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
        Debug.Log($"[Save] Saved — Coins: {data.coins}, Lifetime: {data.lifetimeEarnings}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[Save] No save file — starting fresh.");
            NewGame();
            return;
        }

        var data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath));

        // Economy
        GameManager.Instance.Economy.SetCoins(data.coins);
        GameManager.Instance.Economy.SetLifetimeEarnings(data.lifetimeEarnings);

        // Tools
        if (data.tools != null)
            GameManager.Instance.ToolManager?.LoadFromSaveData(data.tools);

        // Inventory
        if (data.inventoryItems != null)
        {
            foreach (var item in data.inventoryItems)
            {
                CropData crop = cropDatabase?.GetCropById(item.cropId);
                if (crop != null) GameManager.Instance.Inventory.AddItem(crop, item.quantity);
                else Debug.LogWarning($"[Save] Unknown crop '{item.cropId}' skipped.");
            }
        }

        // Farm tiles
        if (data.tiles != null)
        {
            var grid = GameManager.Instance.FarmGrid;
            foreach (var tileData in data.tiles)
            {
                var coord = new Vector2Int(tileData.coordX, tileData.coordY);
                FarmTile tile = grid.GetTile(coord);
                if (tile == null) continue;

                CropData crop = string.IsNullOrEmpty(tileData.cropId)
                    ? null : cropDatabase?.GetCropById(tileData.cropId);

                tile.LoadFromSaveData(tileData, crop);

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
                if (bd == null) { Debug.LogWarning($"[Save] Unknown building '{bData.buildingId}' skipped."); continue; }
                BuildingManager.Instance?.RestoreBuilding(bd, new Vector2Int(bData.coordX, bData.coordY), bData.rotation);
            }
        }

        Debug.Log($"[Save] Loaded — Coins: {data.coins}");
    }

    private void NewGame()
    {
        GameManager.Instance.Economy.SetCoins(500);
        Debug.Log("[Save] New game started — 500 coins.");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        Debug.Log("[Save] Save deleted.");
    }
}

[System.Serializable]
public class GameSaveData
{
    public int    coins;
    public int    lifetimeEarnings;
    public ToolSaveData              tools;
    public List<InventorySaveItem>   inventoryItems;
    public List<FarmTileSaveData>    tiles;
    public List<BuildingSaveData>    buildings;
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
