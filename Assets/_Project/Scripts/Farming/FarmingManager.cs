using UnityEngine;

/// <summary>
/// Handles player farming actions: tilling, planting, watering, and harvesting.
/// Also ticks crop growth for all planted tiles each frame.
/// </summary>
public class FarmingManager : MonoBehaviour
{
    public static FarmingManager Instance { get; private set; }

    [Header("Visual")]
    [SerializeField] private GameObject cropVisualPrefab; // Prefab with CropGrowthVisual component
    [SerializeField] private ParticleSystem harvestParticles;
    [SerializeField] private ParticleSystem waterParticles;

    private FarmGrid grid;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        grid = GameManager.Instance.FarmGrid;
    }

    private void Update()
    {
        // Tick growth for all planted tiles
        foreach (var tile in grid.GetAllTiles().Values)
        {
            tile.UpdateGrowth(Time.deltaTime);
        }
    }

    public bool TillTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || tile.IsTilled) return false;

        tile.Till();
        return true;
    }

    public bool PlantCrop(Vector2Int coord, CropData crop)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsTilled || tile.IsPlanted) return false;

        // Check unlock level
        if (GameManager.Instance.Progression.CurrentLevel < crop.UnlockLevel)
        {
            Debug.Log($"{crop.CropName} requires level {crop.UnlockLevel}.");
            return false;
        }

        // Deduct seed cost
        if (!GameManager.Instance.Economy.SpendCoins(crop.SeedCost)) return false;

        bool planted = tile.Plant(crop);
        if (planted)
        {
            // Spawn visual
            if (cropVisualPrefab != null)
            {
                GameObject visual = Instantiate(cropVisualPrefab, tile.WorldPosition, Quaternion.identity);
                visual.GetComponent<CropGrowthVisual>()?.Initialise(tile);
            }

            // Award XP
            GameManager.Instance.Progression.AddXP(crop.PlantXP);
        }
        return planted;
    }

    public bool WaterTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsPlanted || tile.IsWatered) return false;

        tile.Water();

        // Particles
        if (waterParticles != null)
            Instantiate(waterParticles, tile.WorldPosition + Vector3.up * 0.5f, Quaternion.identity);

        // Award XP
        GameManager.Instance.Progression.AddXP(GameManager.Instance.FarmGrid.GetTile(coord).PlantedCrop.WaterXP);

        return true;
    }

    public CropData HarvestTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsReadyToHarvest) return null;

        CropData crop = tile.PlantedCrop;
        int xp = crop.HarvestXP;

        CropData harvested = tile.Harvest();
        if (harvested != null)
        {
            // Add to inventory
            GameManager.Instance.Inventory.AddItem(harvested, 1);

            // Award XP
            GameManager.Instance.Progression.AddXP(xp);

            // Particles
            if (harvestParticles != null)
                Instantiate(harvestParticles, tile.WorldPosition + Vector3.up * 0.5f, Quaternion.identity);
        }

        return harvested;
    }
}
