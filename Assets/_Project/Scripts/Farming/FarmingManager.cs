using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player farming actions: tilling, planting, watering, and harvesting.
/// Growth is day-based — crops advance when the day changes, not in real time.
/// All actions check energy via EnergyManager and tool via ToolManager.
/// </summary>
public class FarmingManager : MonoBehaviour
{
    public static FarmingManager Instance { get; private set; }

    [Header("Visual")]
    [SerializeField] private GameObject cropVisualPrefab;
    [SerializeField] private GameObject harvestReadyFXPrefab;
    [SerializeField] private GameObject harvestFXPrefab;
    [SerializeField] private GameObject waterFXPrefab;
    [SerializeField] private GameObject plantFXPrefab;

    [Header("Debug")]
    [SerializeField] private bool skipEnergyCheck = false; // enable in Inspector for testing

    private FarmGrid grid;
    private Dictionary<Vector2Int, FarmTile>   tileCache    = new();
    private Dictionary<Vector2Int, GameObject> cropVisuals  = new();
    private Dictionary<Vector2Int, GameObject> tileMarkers  = new();

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

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged     += OnDayChanged;
            GameTimeManager.Instance.OnSeasonChanged  += OnSeasonChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged    -= OnDayChanged;
            GameTimeManager.Instance.OnSeasonChanged -= OnSeasonChanged;
        }
    }

    // ── Day events ───────────────────────────────────────────────────────────

    private void OnDayChanged(int day, Season season, int year)
    {
        foreach (var tile in tileCache.Values)
            tile.AdvanceDay();
    }

    private void OnSeasonChanged(Season oldSeason, Season newSeason)
    {
        int killed = 0;
        foreach (var kvp in tileCache)
        {
            bool wasPlanted = kvp.Value.IsPlanted && !kvp.Value.IsReadyToHarvest;
            kvp.Value.OnSeasonEnd();

            if (wasPlanted && !kvp.Value.IsPlanted)
            {
                killed++;
                RemoveCropVisual(kvp.Key);
                RemoveTileMarker(kvp.Key);
            }
        }

        if (killed > 0)
            HUDManager.Instance?.ShowNotification(
                $"{killed} crop{(killed > 1 ? "s" : "")} died at season end.", 4f);
    }

    // ── Farming actions ──────────────────────────────────────────────────────

    public bool TillTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || tile.IsTilled) return false;

        if (!SpendEnergy(ToolManager.EnergyCostHoe)) return false;

        tile.Till();
        AudioManager.Instance?.PlayTill();
        return true;
    }

    public bool PlantCrop(Vector2Int coord, CropData crop)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || tile.IsPlanted) return false;

        // Auto-till if needed so player doesn't need a separate till step
        if (!tile.IsTilled) tile.Till();

        // Season check
        if (!crop.CanGrowIn(GameTimeManager.Instance.CurrentSeason))
        {
            HUDManager.Instance?.ShowNotification(
                $"{crop.CropName} does not grow in {GameTimeManager.Instance.CurrentSeason.DisplayName()}.", 3f);
            return false;
        }

        if (!GameManager.Instance.Economy.SpendCoins(crop.SeedCost)) return false;
        if (!SpendEnergy(ToolManager.EnergyCostPlant)) { GameManager.Instance.Economy.AddCoins(crop.SeedCost); return false; }

        if (!tile.Plant(crop)) return false;

        Vector3 worldPos = grid.GridToWorld(coord);

        if (cropVisualPrefab != null)
        {
            GameObject visual = Instantiate(cropVisualPrefab, worldPos, Quaternion.identity);
            visual.GetComponent<CropGrowthVisual>()?.Initialise(tile, harvestReadyFXPrefab);
            cropVisuals[coord] = visual;
        }

        GameManager.Instance.Progression.AddXP(crop.PlantXP);
        AudioManager.Instance?.PlayPlant();

        if (plantFXPrefab != null)
        {
            var fx = Instantiate(plantFXPrefab, worldPos + Vector3.up * 0.1f, Quaternion.identity);
            Destroy(fx, 3f);
        }

        return true;
    }

    public bool WaterTile(Vector2Int coord, bool playEffects = true)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsPlanted || tile.IsWatered) return false;

        // Watering can capacity check (skip if playEffects=false, e.g. well auto-water)
        if (playEffects)
        {
            if (ToolManager.Instance != null && !ToolManager.Instance.TryUseWateringCan()) return false;
            if (!SpendEnergy(ToolManager.EnergyCostWater)) return false;
        }

        tile.Water();

        Vector3 worldPos = grid.GridToWorld(coord);
        SpawnTileMarker(coord, worldPos, new Color(0.15f, 0.35f, 0.9f, 0.12f));

        if (playEffects)
        {
            AudioManager.Instance?.PlayWater();

            if (cropVisuals.TryGetValue(coord, out GameObject visual) && visual != null)
                visual.GetComponent<CropGrowthVisual>()?.PlayWaterBounce();

            if (waterFXPrefab != null)
            {
                var fx = Instantiate(waterFXPrefab, worldPos + Vector3.up * 0.1f, Quaternion.identity);
                Destroy(fx, 3f);
            }

            GameManager.Instance.Progression.AddXP(tile.PlantedCrop?.WaterXP ?? 1);
        }

        return true;
    }

    public CropData HarvestTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsReadyToHarvest) return null;

        if (!SpendEnergy(ToolManager.EnergyCostHarvest)) return null;

        CropData crop = tile.PlantedCrop;
        int xp        = crop.HarvestXP;

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
        GameManager.Instance.Progression.AddXP(xp);
        AudioManager.Instance?.PlayHarvest();

        RemoveTileMarker(coord);

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
        Vector3 worldPos = grid.GridToWorld(coord);

        if (cropVisualPrefab != null)
        {
            GameObject visual = Instantiate(cropVisualPrefab, worldPos, Quaternion.identity);
            visual.GetComponent<CropGrowthVisual>()?.Initialise(tile, harvestReadyFXPrefab);
            cropVisuals[coord] = visual;
        }

        if (tile.IsWatered)
            SpawnTileMarker(coord, worldPos, new Color(0.15f, 0.35f, 0.9f, 0.12f));
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public int GetPlantedCount(string cropId)
    {
        int count = 0;
        foreach (var tile in tileCache.Values)
            if (tile.IsPlanted && tile.PlantedCrop?.CropId == cropId) count++;
        return count;
    }

    public int GetRemainingDays(string cropId)
    {
        int nearest = int.MaxValue;
        bool found  = false;
        foreach (var tile in tileCache.Values)
        {
            if (!tile.IsPlanted || tile.PlantedCrop?.CropId != cropId) continue;
            found = true;
            if (tile.IsReadyToHarvest) return 0;
            int rem = tile.GetRemainingDays();
            if (rem < nearest) nearest = rem;
        }
        return found ? nearest : -1;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private bool SpendEnergy(int amount)
    {
        if (skipEnergyCheck) return true;
        return EnergyManager.Instance == null || EnergyManager.Instance.TrySpend(amount);
    }

    private void RemoveCropVisual(Vector2Int coord)
    {
        if (cropVisuals.TryGetValue(coord, out GameObject v) && v != null) Destroy(v);
        cropVisuals.Remove(coord);
    }

    private void RemoveTileMarker(Vector2Int coord)
    {
        if (tileMarkers.TryGetValue(coord, out GameObject m) && m != null) Destroy(m);
        tileMarkers.Remove(coord);
    }

    private void SpawnTileMarker(Vector2Int coord, Vector3 worldPos, Color color)
    {
        RemoveTileMarker(coord);

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
        marker.name = "TileMarker";
        marker.transform.position  = worldPos + Vector3.up * 0.02f;
        marker.transform.rotation  = Quaternion.Euler(90f, 0f, 0f);
        marker.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        Destroy(marker.GetComponent<Collider>());

        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard");
        var mat    = new Material(shader);
        mat.SetFloat("_Surface",  1f);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite",   0f);
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        marker.GetComponent<Renderer>().material = mat;

        tileMarkers[coord] = marker;
    }
}
