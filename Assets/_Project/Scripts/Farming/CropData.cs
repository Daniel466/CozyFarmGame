using UnityEngine;

/// <summary>
/// ScriptableObject defining all data for a crop type.
/// Create via: Right-click > Create > CozyFarm > Crop Data
/// </summary>
[CreateAssetMenu(fileName = "NewCrop", menuName = "CozyFarm/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string cropId;
    [SerializeField] private string cropName;
    [SerializeField] private Sprite icon;

    [Header("Season")]
    [SerializeField] private GrowingSeason growingSeason = GrowingSeason.Spring;

    [Header("Growth")]
    [SerializeField] private int growthDays = 4; // days to grow from planted to ripe

    [Header("Economy")]
    [SerializeField] private int seedCost  = 20;
    [SerializeField] private int sellValue = 50;

    [Header("Model")]
    [SerializeField] private GameObject[] growthStagePrefabs = new GameObject[4];
    [SerializeField] private Vector3 modelRotationOffset = Vector3.zero;
    [SerializeField] private float   modelBaseScale      = 1f;

    [Header("XP (legacy — kept for compatibility)")]
    [SerializeField] private int harvestXP = 5;
    [SerializeField] private int plantXP   = 2;
    [SerializeField] private int waterXP   = 1;

    // Accessors
    public string       CropId             => cropId;
    public string       CropName           => cropName;
    public Sprite       Icon               => icon;
    public GrowingSeason GrowingSeason     => growingSeason;
    public int          GrowthDays         => Mathf.Max(1, growthDays);
    public int          SeedCost           => seedCost;
    public int          SellValue          => sellValue;
    public GameObject[] GrowthStagePrefabs => growthStagePrefabs;
    public Vector3      ModelRotationOffset => modelRotationOffset;
    public float        ModelBaseScale      => modelBaseScale;
    public int          HarvestXP          => harvestXP;
    public int          PlantXP            => plantXP;
    public int          WaterXP            => waterXP;

    /// <summary>Returns true if this crop can be planted in the given season.</summary>
    public bool CanGrowIn(Season season) => growingSeason.CanGrowIn(season);
}
