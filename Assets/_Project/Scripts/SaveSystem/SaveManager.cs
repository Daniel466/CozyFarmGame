using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles saving and loading the full game state to/from a local JSON file.
/// Save file lives at: Application.persistentDataPath/save.json
/// </summary>
public class SaveManager : MonoBehaviour
{
    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        // Economy
        data.coins = GameManager.Instance.Economy.Coins;

        // Progression
        data.xp = GameManager.Instance.Progression.CurrentXP;
        data.level = GameManager.Instance.Progression.CurrentLevel;

        // Inventory
        data.inventoryItems = new List<InventorySaveItem>();
        foreach (var kvp in GameManager.Instance.Inventory.GetAllItems())
        {
            data.inventoryItems.Add(new InventorySaveItem
            {
                cropId = kvp.Key,
                quantity = kvp.Value.quantity
            });
        }

        // Farm tiles
        data.tiles = new List<FarmTileSaveData>();
        foreach (var tile in GameManager.Instance.FarmGrid.GetAllTiles().Values)
        {
            if (tile.IsTilled || tile.IsPlanted)
                data.tiles.Add(tile.ToSaveData());
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[Save] Game saved to {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[Save] No save file found — starting fresh.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        // Economy
        GameManager.Instance.Economy.SetCoins(data.coins);

        // Progression
        GameManager.Instance.Progression.SetState(data.xp, data.level);

        // Inventory — TODO: resolve CropData from cropId via CropDatabase
        // (CropDatabase will be added next)

        // Farm tiles — TODO: resolve CropData from cropId via CropDatabase

        Debug.Log($"[Save] Game loaded from {SavePath}");
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
    public int coins;
    public int xp;
    public int level;
    public List<InventorySaveItem> inventoryItems;
    public List<FarmTileSaveData> tiles;
}

[System.Serializable]
public class InventorySaveItem
{
    public string cropId;
    public int quantity;
}
