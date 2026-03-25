using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player farming actions: tilling, planting, and harvesting.
/// Growth is real-time — FarmTile tracks elapsed seconds since planting.
/// No energy, no seasons, no watering.
/// </summary>
public class FarmingManager : MonoBehaviour
{
    public static FarmingManager Instance { get; private set; }

    [Header("Visual")]
    [SerializeField] private GameObject cropVisualPrefab;
    [SerializeField] private GameObject harvestReadyFXPrefab;
    [SerializeField] private GameObject harvestFXPrefab;
    [SerializeField] private GameObject plantFXPrefab;

    private FarmGrid grid;
    private Dictionary<Vector2Int, FarmTile>   tileCache   = new();
    private Dictionary<Vector2Int, GameObject> cropVisuals = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance == null) { Debug.LogError("[FarmingManager] GameManager missing!"); enabled = false; return; }
        grid      = GameManager.Instance.FarmGrid;
        tileCache = grid.GetAllTiles();
    }

    // ── Farming actions ──────────────────────────────────────────────────────

    public bool TillTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || tile.IsTilled) return false;

        tile.Till();
        AudioManager.Instance?.PlayTill();
        return true;
    }

    public bool PlantCrop(Vector2Int coord, CropData crop)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || tile.IsPlanted) return false;

        // Auto-till if needed
        if (!tile.IsTilled) tile.Till();

        if (!GameManager.Instance.Economy.SpendCoins(crop.SeedCost)) return false;

        if (!tile.Plant(crop))
        {
            GameManager.Instance.Economy.AddCoins(crop.SeedCost); // refund
            return false;
        }

        Vector3 worldPos = grid.GridToWorld(coord);

        if (cropVisualPrefab != null)
        {
            GameObject visual = Instantiate(cropVisualPrefab, worldPos, Quaternion.identity);
            visual.GetComponent<CropGrowthVisual>()?.Initialise(tile, harvestReadyFXPrefab);
            cropVisuals[coord] = visual;
        }

        AudioManager.Instance?.PlayPlant();

        if (plantFXPrefab != null)
        {
            var fx = Instantiate(plantFXPrefab, worldPos + Vector3.up * 0.1f, Quaternion.identity);
            Destroy(fx, 3f);
        }

        return true;
    }

    public CropData HarvestTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsReadyToHarvest) return null;

        CropData harvested = tile.Harvest();
        if (harvested == null) return null;

        if (cropVisuals.TryGetValue(coord, out GameObject visual) && visual != null)
        {
            cropVisuals.Remove(coord);
            var cgv = visual.GetComponent<CropGrowthVisual>();
            if (cgv != null) cgv.PopOutAndDestroy(() => Destroy(visual));
            else             Destroy(visual);
        }

        GameManager.Instance.Inventory.AddItem(harvested, 1);
        AudioManager.Instance?.PlayHarvest();

        Vector3 worldPos = grid.GridToWorld(coord);
        if (harvestFXPrefab != null)
        {
            var fx = Instantiate(harvestFXPrefab, worldPos + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(fx, 3f);
        }

        return harvested;
    }

    /// <summary>Restores a planted tile's visual state after loading a save.</summary>
    public void RestoreFromSave(Vector2Int coord, FarmTile tile)
    {
        if (cropVisualPrefab == null) return;
        Vector3 worldPos = grid.GridToWorld(coord);
        GameObject visual = Instantiate(cropVisualPrefab, worldPos, Quaternion.identity);
        visual.GetComponent<CropGrowthVisual>()?.Initialise(tile, harvestReadyFXPrefab);
        cropVisuals[coord] = visual;
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    /// <summary>Number of tiles currently planted with this crop (including ready-to-harvest).</summary>
    public int GetPlantedCount(string cropId)
    {
        int count = 0;
        foreach (var tile in tileCache.Values)
            if (tile.IsPlanted && tile.PlantedCrop?.CropId == cropId) count++;
        return count;
    }

    /// <summary>
    /// Nearest remaining seconds until harvest for this crop.
    /// Returns 0 if any tile is ready, -1 if no tiles planted.
    /// </summary>
    public float GetNearestRemainingSeconds(string cropId)
    {
        float nearest = float.MaxValue;
        bool  found   = false;
        foreach (var tile in tileCache.Values)
        {
            if (!tile.IsPlanted || tile.PlantedCrop?.CropId != cropId) continue;
            found = true;
            if (tile.IsReadyToHarvest) return 0f;
            float rem = tile.GetRemainingSeconds();
            if (rem < nearest) nearest = rem;
        }
        return found ? nearest : -1f;
    }

    /// <summary>Removes a planted crop without adding it to inventory (player used the Remove tool).</summary>
    public bool RemoveCrop(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsPlanted) return false;

        tile.Harvest(); // clears planted state
        RemoveCropVisual(coord);
        AudioManager.Instance?.PlayRemove();
        return true;
    }

    // ── Visual refresh (called by RealTimeManager every second) ──────────────

    /// <summary>
    /// Asks the CropGrowthVisual on this tile to re-evaluate its growth stage.
    /// CropGrowthVisual only rebuilds when the stage changes, so this is cheap.
    /// </summary>
    public void RefreshVisual(Vector2Int coord, FarmTile tile)
    {
        if (!cropVisuals.TryGetValue(coord, out GameObject visual) || visual == null) return;
        visual.GetComponent<CropGrowthVisual>()?.Refresh(tile);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void RemoveCropVisual(Vector2Int coord)
    {
        if (cropVisuals.TryGetValue(coord, out GameObject v) && v != null) Destroy(v);
        cropVisuals.Remove(coord);
    }
}
