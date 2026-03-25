using System;
using UnityEngine;

/// <summary>
/// Represents a single tile on the farm grid.
/// Growth is real-time: crop progress is derived from elapsed wall-clock seconds
/// since planting, so offline catch-up is automatic on load.
/// Crops do not die when ready — they wait until harvested.
/// </summary>
[System.Serializable]
public class FarmTile
{
    public Vector2Int Coord         { get; private set; }
    public Vector3    WorldPosition { get; private set; }

    public bool     IsTilled  { get; private set; }
    public bool     IsPlanted => PlantedCrop != null;
    public CropData PlantedCrop { get; private set; }

    // UTC ticks at the moment the crop was planted (DateTime.UtcNow.Ticks).
    // 0 means no crop is planted.
    private long plantedAtUtcTicks;

    public bool IsReadyToHarvest =>
        IsPlanted && GetElapsedSeconds() >= PlantedCrop.GrowthTimeSeconds;

    public FarmTile(Vector2Int coord, Vector3 worldPosition)
    {
        Coord         = coord;
        WorldPosition = worldPosition;
    }

    // ── Player actions ────────────────────────────────────────────────────────

    public void Till()
    {
        IsTilled = true;
    }

    public bool Plant(CropData crop)
    {
        if (!IsTilled || IsPlanted) return false;
        PlantedCrop       = crop;
        plantedAtUtcTicks = DateTime.UtcNow.Ticks;
        return true;
    }

    /// <summary>
    /// Harvests a ready crop, returning its CropData. Returns null if not ready.
    /// </summary>
    public CropData Harvest()
    {
        if (!IsReadyToHarvest) return null;
        CropData harvested = PlantedCrop;
        ClearCrop();
        return harvested;
    }

    /// <summary>
    /// Force-clears the planted crop regardless of growth state.
    /// Used by the Remove tool. Tile stays tilled.
    /// </summary>
    public void ClearCrop()
    {
        PlantedCrop       = null;
        plantedAtUtcTicks = 0;
    }

    // ── Growth queries ────────────────────────────────────────────────────────

    private double GetElapsedSeconds()
    {
        if (!IsPlanted || plantedAtUtcTicks == 0) return 0;
        return (DateTime.UtcNow - new DateTime(plantedAtUtcTicks, DateTimeKind.Utc)).TotalSeconds;
    }

    /// <summary>0-1 growth fraction. 1 = ready to harvest.</summary>
    public float GetGrowthProgress()
    {
        if (!IsPlanted) return 0f;
        return Mathf.Clamp01((float)(GetElapsedSeconds() / PlantedCrop.GrowthTimeSeconds));
    }

    /// <summary>Growth stage 0-3 used by CropGrowthVisual. -1 if no crop.</summary>
    public int GetGrowthStage()
    {
        if (!IsPlanted) return -1;
        float p = GetGrowthProgress();
        if (p < 0.25f) return 0;
        if (p < 0.5f)  return 1;
        if (p < 1f)    return 2;
        return 3;
    }

    /// <summary>Seconds remaining until harvest. 0 if ready. -1 if no crop.</summary>
    public float GetRemainingSeconds()
    {
        if (!IsPlanted) return -1f;
        if (IsReadyToHarvest) return 0f;
        return Mathf.Max(0f, PlantedCrop.GrowthTimeSeconds - (float)GetElapsedSeconds());
    }

    // ── Save / Load ───────────────────────────────────────────────────────────

    public FarmTileSaveData ToSaveData() => new FarmTileSaveData
    {
        coordX            = Coord.x,
        coordY            = Coord.y,
        isTilled          = IsTilled,
        cropId            = PlantedCrop != null ? PlantedCrop.CropId : "",
        plantedAtUtcTicks = plantedAtUtcTicks,
    };

    public void LoadFromSaveData(FarmTileSaveData data, CropData crop)
    {
        IsTilled          = data.isTilled;
        PlantedCrop       = crop; // null if no crop was planted
        plantedAtUtcTicks = data.plantedAtUtcTicks;
    }
}

[System.Serializable]
public class FarmTileSaveData
{
    public int    coordX;
    public int    coordY;
    public bool   isTilled;
    public string cropId;
    public long   plantedAtUtcTicks;
}
