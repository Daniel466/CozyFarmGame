using UnityEngine;

/// <summary>
/// Central 1-second tick loop for all real-time systems.
/// Drives: crop growth visual refresh, autosave timing.
/// Future: companion task triggers, worker state checks.
///
/// Add this component to the GameManager GameObject.
/// </summary>
public class RealTimeManager : MonoBehaviour
{
    public static RealTimeManager Instance { get; private set; }

    [Header("Autosave")]
    [SerializeField] private float autosaveInterval = 90f;

    [Header("Crop Visual Refresh")]
    [Tooltip("How often (seconds) to poll crop growth stages and update visuals.")]
    [SerializeField] private float cropRefreshInterval = 1f;

    private float autosaveTimer;
    private float cropRefreshTimer;

    // Tick fired every second — subscribe for real-time updates
    public event System.Action OnTick;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1-second tick
        // (using accumulator rather than InvokeRepeating so it survives time scale changes)
        cropRefreshTimer += dt;
        if (cropRefreshTimer >= cropRefreshInterval)
        {
            cropRefreshTimer -= cropRefreshInterval;
            OnTick?.Invoke();
            RefreshCropVisuals();
        }

        // Autosave
        autosaveTimer += dt;
        if (autosaveTimer >= autosaveInterval)
        {
            autosaveTimer = 0f;
            GameManager.Instance?.SaveManager?.SaveGame();
            Debug.Log("[RealTimeManager] Autosave triggered.");
        }
    }

    /// <summary>
    /// Polls every planted tile and asks CropGrowthVisual to update its stage.
    /// CropGrowthVisual only rebuilds the mesh when the stage actually changes,
    /// so this is cheap to call every second.
    /// </summary>
    private void RefreshCropVisuals()
    {
        var grid = GameManager.Instance?.FarmGrid;
        if (grid == null) return;

        foreach (var kvp in grid.GetAllTiles())
        {
            FarmTile tile = kvp.Value;
            if (!tile.IsPlanted) continue;

            // CropGrowthVisual sits on the visual GameObject spawned by FarmingManager.
            // We find it via FarmingManager's crop visual registry.
            FarmingManager.Instance?.RefreshVisual(kvp.Key, tile);
        }
    }

    /// <summary>Call on key game events (sell, hire, place building) to reset autosave timer.</summary>
    public void ResetAutosaveTimer() => autosaveTimer = 0f;
}
