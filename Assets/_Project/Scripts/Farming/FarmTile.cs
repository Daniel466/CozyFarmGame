using UnityEngine;

/// <summary>
/// Represents a single tile on the farm grid.
/// Growth is day-based: crops advance one stage per day IF watered.
/// Unwatered crops pause growth but do not die (Phase 1 simplicity).
/// At season end, unripe crops are killed and empty tilled tiles reset.
/// </summary>
[System.Serializable]
public class FarmTile
{
    public Vector2Int Coord         { get; private set; }
    public Vector3    WorldPosition { get; private set; }

    public bool IsTilled  { get; private set; }
    public bool IsWatered { get; private set; } // watered today
    public bool IsPlanted => PlantedCrop != null;

    public CropData PlantedCrop    { get; private set; }
    public int      DaysGrown      { get; private set; }  // days of growth applied
    public bool     IsReadyToHarvest => IsPlanted && DaysGrown >= PlantedCrop.GrowthDays;

    // GrowthProgress: 0-1 float derived from DaysGrown, drives visual stages
    public float GrowthProgress => IsPlanted
        ? Mathf.Clamp01((float)DaysGrown / PlantedCrop.GrowthDays)
        : 0f;

    public FarmTile(Vector2Int coord, Vector3 worldPosition)
    {
        Coord         = coord;
        WorldPosition = worldPosition;
        IsTilled      = false;
        IsWatered     = false;
        DaysGrown     = 0;
    }

    // ── Player actions ───────────────────────────────────────────────────────

    public void Till()
    {
        IsTilled = true;
    }

    public bool Plant(CropData crop)
    {
        if (!IsTilled || IsPlanted) return false;
        PlantedCrop = crop;
        DaysGrown   = 0;
        IsWatered   = false;
        return true;
    }

    public void Water()
    {
        if (IsPlanted && !IsWatered)
            IsWatered = true;
    }

    public CropData Harvest()
    {
        if (!IsReadyToHarvest) return null;
        CropData harvested = PlantedCrop;
        PlantedCrop = null;
        DaysGrown   = 0;
        IsWatered   = false;
        // Tile stays tilled after harvest
        return harvested;
    }

    // ── Day advance ──────────────────────────────────────────────────────────

    /// <summary>
    /// Called by FarmingManager when the day advances.
    /// Watered crops grow one day. Watered flag resets for the new day.
    /// </summary>
    public void AdvanceDay()
    {
        if (IsPlanted && !IsReadyToHarvest && IsWatered)
            DaysGrown++;

        IsWatered = false; // reset for the new day
    }

    /// <summary>
    /// Called at season end. Kills unripe crops. Resets tilled state on empty tiles.
    /// </summary>
    public void OnSeasonEnd()
    {
        if (IsPlanted && !IsReadyToHarvest)
        {
            // Crop didn't make it — dies
            PlantedCrop = null;
            DaysGrown   = 0;
            IsWatered   = false;
            IsTilled    = false; // field reverts to untilled
        }
        else if (!IsPlanted && IsTilled)
        {
            IsTilled = false; // unused tilled tile resets
        }
        // Ready-to-harvest crops survive season end (player can still collect them)
    }

    // ── Visual helpers ───────────────────────────────────────────────────────

    /// <summary>Growth stage 0-3 used by CropGrowthVisual.</summary>
    public int GetGrowthStage()
    {
        if (!IsPlanted) return -1;
        if (GrowthProgress < 0.33f) return 0;
        if (GrowthProgress < 0.66f) return 1;
        if (GrowthProgress < 1f)    return 2;
        return 3;
    }

    /// <summary>Estimated days remaining until harvest.</summary>
    public int GetRemainingDays()
    {
        if (!IsPlanted || IsReadyToHarvest) return 0;
        return PlantedCrop.GrowthDays - DaysGrown;
    }

    // ── Save / Load ──────────────────────────────────────────────────────────

    public FarmTileSaveData ToSaveData() => new FarmTileSaveData
    {
        coordX    = Coord.x,
        coordY    = Coord.y,
        isTilled  = IsTilled,
        isWatered = IsWatered,
        cropId    = PlantedCrop != null ? PlantedCrop.CropId : "",
        daysGrown = DaysGrown,
    };

    public void LoadFromSaveData(FarmTileSaveData data, CropData crop)
    {
        IsTilled    = data.isTilled;
        IsWatered   = data.isWatered;
        PlantedCrop = crop;
        DaysGrown   = data.daysGrown;
    }
}

[System.Serializable]
public class FarmTileSaveData
{
    public int    coordX;
    public int    coordY;
    public bool   isTilled;
    public bool   isWatered;
    public string cropId;
    public int    daysGrown;
}
