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
    [SerializeField] private GameObject cropVisualPrefab;     // Prefab with CropGrowthVisual component
    [SerializeField] private GameObject harvestReadyFXPrefab; // FX_Sparkle_Orbit_01 — loops above ready crop
    [SerializeField] private GameObject harvestFXPrefab;      // FX_Confetti_01 — burst on harvest
    [SerializeField] private GameObject waterFXPrefab;        // FX_Impact_Water_Ripple_01 — burst on water
    [SerializeField] private GameObject plantFXPrefab;        // FX_Dust_Small_01 — burst on plant

    [Header("Debug")]
    [SerializeField] private float growthSpeedMultiplier = 1f; // Set to 60 in Inspector for fast testing
    public float GrowthSpeedMultiplier
    {
        get => growthSpeedMultiplier;
        set => growthSpeedMultiplier = value;
    }

    // Additive bonus applied on top of growthSpeedMultiplier — owned exclusively by DogController.
    // Kept separate so the dog never corrupts the Inspector-set base value.
    private float dogGrowthBonus;

    /// <summary>
    /// Additive growth speed bonus granted by the dog's happiness (set by DogController).
    /// Combined with growthSpeedMultiplier at tick time.
    /// </summary>
    public float DogGrowthBonus
    {
        get => dogGrowthBonus;
        set => dogGrowthBonus = Mathf.Max(0f, value);
    }

    /// <summary>Effective multiplier used for crop growth ticks.</summary>
    public float EffectiveGrowthMultiplier => growthSpeedMultiplier + dogGrowthBonus;

    private FarmGrid grid;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private Dictionary<Vector2Int, FarmTile> tileCache;

    // Use Vector2Int coord as key instead of Vector3 worldPos — worldPos was unreliable
    // because tile.WorldPosition was baked at grid init before Grid Origin was set
    private Dictionary<Vector2Int, GameObject> tileMarkers = new Dictionary<Vector2Int, GameObject>();
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
            if (tile.IsPlanted) tile.UpdateGrowth(Time.deltaTime * EffectiveGrowthMultiplier);
        }
    }

    public bool TillTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || tile.IsTilled) return false;

        tile.Till();
        // No marker spawned — the flower bed IS the tilled visual
        AudioManager.Instance?.PlayTill();
        return true;
    }

    /// <summary>
    /// Spawns a flat quad on the ground to visually mark a tilled or watered tile.
    /// Keyed by coord so it survives Grid Origin changes.
    /// </summary>
    private void SpawnTileMarker(Vector2Int coord, Vector3 worldPos, Color color)
    {
        // Remove old marker if one exists for this coord
        if (tileMarkers.TryGetValue(coord, out GameObject old) && old != null)
            Destroy(old);

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
        marker.name = "TileMarker";
        marker.transform.position = worldPos + Vector3.up * 0.02f;
        marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Lie flat
        marker.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        // Remove collider so it doesn't block raycasts
        Destroy(marker.GetComponent<Collider>());

        // URP transparent material
        var unlitShader = Shader.Find("Universal Render Pipeline/Unlit")
                       ?? Shader.Find("Unlit/Color")
                       ?? Shader.Find("Standard");
        var mat = new Material(unlitShader);
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        marker.GetComponent<Renderer>().material = mat;

        tileMarkers[coord] = marker;
    }

    public void UpdateTileMarkerColor(Vector2Int coord, Color color)
    {
        if (tileMarkers.TryGetValue(coord, out GameObject marker) && marker != null)
        {
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
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
            // Always use grid.GridToWorld so Grid Origin is respected
            Vector3 worldPos = grid.GridToWorld(coord);

            // Spawn visual and track it
            if (cropVisualPrefab != null)
            {
                GameObject visual = Instantiate(cropVisualPrefab, worldPos, Quaternion.identity);
                visual.GetComponent<CropGrowthVisual>()?.Initialise(tile, harvestReadyFXPrefab);
                cropVisuals[coord] = visual;
            }

            // Award XP
            GameManager.Instance.Progression.AddXP(crop.PlantXP);
            AudioManager.Instance?.PlayPlant();

            if (plantFXPrefab != null)
            {
                var fx = Instantiate(plantFXPrefab, worldPos + Vector3.up * 0.1f, Quaternion.identity);
                Destroy(fx, 3f);
            }
        }
        return planted;
    }

    /// <summary>
    /// Restores a planted tile's visual state after loading a save.
    /// Does not spend coins, award XP, or play audio.
    /// </summary>
    public void RestoreFromSave(Vector2Int coord, FarmTile tile)
    {
        Vector3 worldPos = grid.GridToWorld(coord);

        if (cropVisualPrefab != null)
        {
            GameObject visual = Instantiate(cropVisualPrefab, worldPos, Quaternion.identity);
            visual.GetComponent<CropGrowthVisual>()?.Initialise(tile, harvestReadyFXPrefab);
            cropVisuals[coord] = visual;
        }

    }

    /// <summary>
    /// Waters a planted tile. playEffects=false is used by the Watering Well building
    /// to avoid audio/particle spam when watering multiple tiles at once.
    /// </summary>
    public bool WaterTile(Vector2Int coord, bool playEffects = true)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsPlanted || tile.IsWatered) return false;

        tile.Water();

        // Always use grid.GridToWorld so Grid Origin is respected
        Vector3 worldPos = grid.GridToWorld(coord);

        // Destroy any existing marker before spawning a new one
        if (tileMarkers.TryGetValue(coord, out GameObject existing))
        {
            Destroy(existing);
            tileMarkers.Remove(coord);
        }

        SpawnTileMarker(coord, worldPos, new Color(0.15f, 0.35f, 0.9f, 0.12f));

        if (playEffects)
        {
            AudioManager.Instance?.PlayWater();

            // Crop bounce
            if (cropVisuals.TryGetValue(coord, out GameObject visual) && visual != null)
                visual.GetComponent<CropGrowthVisual>()?.PlayWaterBounce();

            if (waterFXPrefab != null)
            {
                var fx = Instantiate(waterFXPrefab, worldPos + Vector3.up * 0.1f, Quaternion.identity);
                Destroy(fx, 3f);
            }

            // Award XP using cached value
            GameManager.Instance.Progression.AddXP(tile.PlantedCrop?.WaterXP ?? 1);
        }

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
            // Pop-out animation then destroy — deferred so the tween plays out
            if (cropVisuals.TryGetValue(coord, out GameObject visual) && visual != null)
            {
                cropVisuals.Remove(coord);
                var cgv = visual.GetComponent<CropGrowthVisual>();
                if (cgv != null)
                    cgv.PopOutAndDestroy(() => Destroy(visual));
                else
                    Destroy(visual);
            }

            // Inventory, XP, audio, particles all fire immediately
            GameManager.Instance.Inventory.AddItem(harvested, 1);
            GameManager.Instance.Progression.AddXP(xp);
            AudioManager.Instance?.PlayHarvest();

            if (tileMarkers.TryGetValue(coord, out GameObject marker) && marker != null)
            {
                Destroy(marker);
                tileMarkers.Remove(coord);
            }
            Vector3 worldPos = grid.GridToWorld(coord);

            if (harvestFXPrefab != null)
            {
                var fx = Instantiate(harvestFXPrefab, worldPos + Vector3.up * 0.5f, Quaternion.identity);
                Destroy(fx, 3f);
            }
        }

        return harvested;
    }

    // ── Selected-crop panel queries ─────────────────────────────────────────────

    /// <summary>Count of tiles with this crop planted (including ready-to-harvest).</summary>
    public int GetPlantedCount(string cropId)
    {
        if (tileCache == null) return 0;
        int count = 0;
        foreach (var tile in tileCache.Values)
            if (tile.IsPlanted && tile.PlantedCrop?.CropId == cropId)
                count++;
        return count;
    }

    /// <summary>
    /// Remaining seconds on the nearest-to-done tile of this crop type.
    /// Returns 0 if any tile is ready, -1 if none planted.
    /// </summary>
    public float GetNearestRemainingSeconds(string cropId)
    {
        if (tileCache == null) return -1f;
        float nearest = float.MaxValue;
        bool found = false;
        foreach (var tile in tileCache.Values)
        {
            if (!tile.IsPlanted || tile.PlantedCrop?.CropId != cropId) continue;
            found = true;
            if (tile.IsReadyToHarvest) return 0f;
            float rem = tile.GetRemainingSeconds(EffectiveGrowthMultiplier);
            if (rem < nearest) nearest = rem;
        }
        return found ? nearest : -1f;
    }
}