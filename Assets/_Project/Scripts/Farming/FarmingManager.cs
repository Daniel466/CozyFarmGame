using System.Collections.Generic;
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

    [Header("Debug")]
    [SerializeField] private float growthSpeedMultiplier = 1f; // Set to 60 in Inspector for fast testing

    private FarmGrid grid;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private Dictionary<Vector2Int, FarmTile> tileCache;
    private Dictionary<Vector3, GameObject> tileMarkers = new Dictionary<Vector3, GameObject>();
    private Dictionary<Vector2Int, GameObject> cropVisuals = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[FarmingManager] GameManager not found!");
            enabled = false;
            return;
        }
        grid = GameManager.Instance.FarmGrid;
        if (grid == null)
        {
            Debug.LogError("[FarmingManager] FarmGrid not found!");
            enabled = false;
            return;
        }
        // Cache tile dictionary to avoid repeated GetAllTiles() calls in Update
        tileCache = grid.GetAllTiles();
    }

    private void Update()
    {
        if (tileCache == null) return;
        // Tick growth for all planted tiles
        foreach (var tile in tileCache.Values)
        {
            if (tile.IsPlanted) tile.UpdateGrowth(Time.deltaTime * growthSpeedMultiplier);
        }
    }

    public bool TillTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || tile.IsTilled) return false;

        tile.Till();
        SpawnTileMarker(tile.WorldPosition, new Color(0.28f, 0.17f, 0.09f)); // Dark brown = tilled
        return true;
    }

    /// <summary>
    /// Spawns a flat quad on the ground to visually mark a tilled or watered tile.
    /// </summary>
    private void SpawnTileMarker(Vector3 worldPos, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
        marker.name = "TileMarker";
        marker.transform.position = worldPos + Vector3.up * 0.01f; // Slightly above ground
        marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Lie flat
        marker.transform.localScale = new Vector3(0.9f, 0.9f, 1f); // Slightly smaller than tile

        // Remove collider so it doesn't block raycasts
        Destroy(marker.GetComponent<Collider>());

        // URP material
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        mat.SetFloat("_Smoothness", 0f);
        marker.GetComponent<Renderer>().material = mat;

        // Store reference on the tile for later updates (watering)
        tileMarkers[worldPos] = marker;
    }

    public void UpdateTileMarkerColor(Vector3 worldPos, Color color)
    {
        if (tileMarkers.TryGetValue(worldPos, out GameObject marker) && marker != null)
        {
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Create a new material instance to avoid shared material modification
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetColor("_BaseColor", color);
                mat.color = color;
                mat.SetFloat("_Smoothness", 0f);
                renderer.material = mat;
            }
        }
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
            // Spawn visual and track it
            if (cropVisualPrefab != null)
            {
                GameObject visual = Instantiate(cropVisualPrefab, tile.WorldPosition, Quaternion.identity);
                visual.GetComponent<CropGrowthVisual>()?.Initialise(tile);
                cropVisuals[coord] = visual;
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

        // Cache crop reference BEFORE watering (tile state changes after Water())
        int xp = tile.PlantedCrop?.WaterXP ?? 1;

        tile.Water();

        // Update tile marker to blue = watered
        UpdateTileMarkerColor(tile.WorldPosition, new Color(0.15f, 0.35f, 0.9f));

        // Particles
        if (waterParticles != null)
            Instantiate(waterParticles, tile.WorldPosition + Vector3.up * 0.5f, Quaternion.identity);

        // Award XP using cached value
        GameManager.Instance.Progression.AddXP(xp);

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
            // Destroy crop visual
            if (cropVisuals.TryGetValue(coord, out GameObject visual) && visual != null)
            {
                Destroy(visual);
                cropVisuals.Remove(coord);
            }

            // Add to inventory
            GameManager.Instance.Inventory.AddItem(harvested, 1);

            // Award XP
            GameManager.Instance.Progression.AddXP(xp);

            // Reset tile marker back to tilled brown
            UpdateTileMarkerColor(tile.WorldPosition, new Color(0.28f, 0.17f, 0.09f));

            // Particles
            if (harvestParticles != null)
                Instantiate(harvestParticles, tile.WorldPosition + Vector3.up * 0.5f, Quaternion.identity);
        }

        return harvested;
    }
}
