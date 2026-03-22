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

        // Build particle systems procedurally if not assigned in Inspector
        if (waterParticles   == null) waterParticles   = CreateWaterParticles();
        if (harvestParticles == null) harvestParticles = CreateHarvestParticles();
    }

    private ParticleSystem CreateWaterParticles()
    {
        var go = new GameObject("WaterParticleTemplate");
        go.SetActive(false); // keep inactive — we Instantiate copies at runtime
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration         = 0.6f;
        main.loop             = false;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(2.5f, 5f);
        main.startSize        = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
        main.startColor       = new ParticleSystem.MinMaxGradient(
                                    new Color(0.4f, 0.75f, 1.0f, 0.9f),
                                    new Color(0.7f, 0.92f, 1.0f, 0.7f));
        main.gravityModifier  = 1.2f;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.stopAction       = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.Cone;
        shape.angle        = 35f;
        shape.radius       = 0.3f;
        shape.rotation     = new Vector3(-90f, 0f, 0f); // spray upward

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.5f, 0.8f, 1f), 0f),
                    new GradientColorKey(new Color(0.7f, 0.95f, 1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.5f, 0.8f), new Keyframe(1f, 0f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                          ?? Shader.Find("Particles/Standard Unlit")
                          ?? Shader.Find("Standard");
        var mat = new Material(particleShader);
        mat.SetColor("_BaseColor", new Color(0.4f, 0.75f, 1.0f, 0.9f));
        renderer.material = mat;

        return ps;
    }

    private ParticleSystem CreateHarvestParticles()
    {
        var go = new GameObject("HarvestParticleTemplate");
        go.SetActive(false);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration         = 0.5f;
        main.loop             = false;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        main.startSize        = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startColor       = new ParticleSystem.MinMaxGradient(
                                    new Color(1.0f, 0.88f, 0.2f, 1f),
                                    new Color(0.6f, 1.0f, 0.3f, 1f));
        main.startRotation    = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.gravityModifier  = 0.3f;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.stopAction       = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.25f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0f),
                    new GradientColorKey(new Color(0.8f, 1f, 0.4f), 0.5f),
                    new GradientColorKey(new Color(1f, 0.8f, 0.2f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.3f), new Keyframe(0.2f, 1f), new Keyframe(1f, 0f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-120f * Mathf.Deg2Rad, 120f * Mathf.Deg2Rad);

        var particleShader2 = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                           ?? Shader.Find("Particles/Standard Unlit")
                           ?? Shader.Find("Standard");
        var mat = new Material(particleShader2);
        mat.SetColor("_BaseColor", new Color(1f, 0.88f, 0.2f, 1f));
        go.GetComponent<ParticleSystemRenderer>().material = mat;

        return ps;
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
        marker.transform.localScale = new Vector3(2.0f, 2.0f, 1f);

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
                visual.GetComponent<CropGrowthVisual>()?.Initialise(tile);
                cropVisuals[coord] = visual;
            }

            // Award XP
            GameManager.Instance.Progression.AddXP(crop.PlantXP);
            AudioManager.Instance?.PlayPlant();
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
            visual.GetComponent<CropGrowthVisual>()?.Initialise(tile);
            cropVisuals[coord] = visual;
        }

    }

    public bool WaterTile(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null || !tile.IsPlanted || tile.IsWatered) return false;

        // Cache crop reference BEFORE watering (tile state changes after Water())
        int xp = tile.PlantedCrop?.WaterXP ?? 1;

        tile.Water();
        AudioManager.Instance?.PlayWater();

        // Always use grid.GridToWorld so Grid Origin is respected
        Vector3 worldPos = grid.GridToWorld(coord);

        // Subtle blue marker — alpha 0.2, scale 2.0
        SpawnTileMarker(coord, worldPos, new Color(0.15f, 0.35f, 0.9f, 0.2f));

        // Crop bounce
        if (cropVisuals.TryGetValue(coord, out GameObject visual) && visual != null)
            visual.GetComponent<CropGrowthVisual>()?.PlayWaterBounce();

        // Particles — clone plays once then self-destructs
        if (waterParticles != null)
        {
            var ps = Instantiate(waterParticles, worldPos + Vector3.up * 0.5f, Quaternion.identity);
            ps.gameObject.SetActive(true);
            ps.Play();
            Destroy(ps.gameObject, 2f); // fallback: force destroy after 2s regardless
        }

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

            AudioManager.Instance?.PlayHarvest();

            // Remove watered marker now that tile is reset
            if (tileMarkers.TryGetValue(coord, out GameObject marker) && marker != null)
            {
                Destroy(marker);
                tileMarkers.Remove(coord);
            }
            Vector3 worldPos = grid.GridToWorld(coord);

            // Particles
            if (harvestParticles != null)
            {
                var ps = Instantiate(harvestParticles, worldPos + Vector3.up * 0.5f, Quaternion.identity);
                ps.gameObject.SetActive(true);
                ps.Play();
            }
        }

        return harvested;
    }
}