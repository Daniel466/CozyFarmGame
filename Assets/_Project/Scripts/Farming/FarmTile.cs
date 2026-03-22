using UnityEngine;

/// <summary>
/// Represents a single tile on the farm grid.
/// Tracks tilled state, planted crop, watered state, and growth progress.
/// </summary>
[System.Serializable]
public class FarmTile
{
    public Vector2Int Coord { get; private set; }
    public Vector3 WorldPosition { get; private set; }

    public bool IsTilled { get; private set; }
    public bool IsWatered { get; private set; }
    public bool IsPlanted => PlantedCrop != null;

    public CropData PlantedCrop { get; private set; }
    public float GrowthProgress { get; private set; }  // 0f to 1f
    public float PlantedTime { get; private set; }
    public bool IsReadyToHarvest => IsPlanted && GrowthProgress >= 1f;

    public FarmTile(Vector2Int coord, Vector3 worldPosition)
    {
        Coord = coord;
        WorldPosition = worldPosition;
        IsTilled = true;  // Flower beds are pre-tilled — no till step required
        IsWatered = false;
        GrowthProgress = 0f;
    }

    public void Till()
    {
        IsTilled = true;
    }

    public bool Plant(CropData crop)
    {
        if (!IsTilled || IsPlanted) return false;

        PlantedCrop = crop;
        GrowthProgress = 0f;
        PlantedTime = Time.time;
        return true;
    }

    public void Water()
    {
        if (IsPlanted && !IsWatered)
            IsWatered = true;
    }

    /// <summary>
    /// Called every frame to advance crop growth.
    /// Watered crops grow 30% faster as per GDD.
    /// </summary>
    public void UpdateGrowth(float deltaTime)
    {
        if (!IsPlanted || IsReadyToHarvest) return;

        float growthRate = 1f / (PlantedCrop.GrowTimeSeconds);
        if (IsWatered) growthRate *= 1.3f; // 30% faster when watered

        GrowthProgress = Mathf.Clamp01(GrowthProgress + growthRate * deltaTime);
    }

    /// <summary>
    /// Returns the current growth stage index (0–3) for visual updates.
    /// 0 = Planted, 1 = Sprouting, 2 = Growing, 3 = Ready
    /// </summary>
    public int GetGrowthStage()
    {
        if (!IsPlanted) return -1;
        if (GrowthProgress < 0.33f) return 0;
        if (GrowthProgress < 0.66f) return 1;
        if (GrowthProgress < 1f) return 2;
        return 3;
    }

    public CropData Harvest()
    {
        if (!IsReadyToHarvest) return null;

        CropData harvested = PlantedCrop;
        PlantedCrop = null;
        GrowthProgress = 0f;
        IsWatered = false;
        // Tile stays tilled after harvest
        return harvested;
    }

    // --- Save/Load support ---
    public FarmTileSaveData ToSaveData()
    {
        return new FarmTileSaveData
        {
            coordX = Coord.x,
            coordY = Coord.y,
            isTilled = IsTilled,
            isWatered = IsWatered,
            cropId = PlantedCrop != null ? PlantedCrop.CropId : "",
            growthProgress = GrowthProgress
        };
    }

    public void LoadFromSaveData(FarmTileSaveData data, CropData crop)
    {
        IsTilled = data.isTilled;
        IsWatered = data.isWatered;
        PlantedCrop = crop;
        GrowthProgress = data.growthProgress;
    }
}

[System.Serializable]
public class FarmTileSaveData
{
    public int coordX;
    public int coordY;
    public bool isTilled;
    public bool isWatered;
    public string cropId;
    public float growthProgress;
}
