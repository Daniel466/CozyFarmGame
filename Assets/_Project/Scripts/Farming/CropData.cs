using UnityEngine;

/// <summary>
/// ScriptableObject defining all data for a crop type.
/// Create assets via: Right-click > Create > CozyFarm > Crop Data
/// </summary>
[CreateAssetMenu(fileName = "NewCrop", menuName = "CozyFarm/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string cropId;
    [SerializeField] private string cropName;
    [SerializeField] private Sprite icon;

    [Header("Progression")]
    [SerializeField] private int unlockLevel = 1;

    [Header("Economy")]
    [SerializeField] private int seedCost = 5;
    [SerializeField] private int sellValue = 10;

    [Header("Farming")]
    [SerializeField] private float growTimeSeconds = 300f; // 5 minutes default
    [SerializeField] private GameObject[] growthStagePrefabs = new GameObject[4]; // 0=planted, 1=sprout, 2=growing, 3=ready

    [Header("XP")]
    [SerializeField] private int harvestXP = 5;
    [SerializeField] private int plantXP = 2;
    [SerializeField] private int waterXP = 1;

    // Public accessors
    public string CropId => cropId;
    public string CropName => cropName;
    public Sprite Icon => icon;
    public int UnlockLevel => unlockLevel;
    public int SeedCost => seedCost;
    public int SellValue => sellValue;
    public float GrowTimeSeconds => growTimeSeconds;
    public GameObject[] GrowthStagePrefabs => growthStagePrefabs;
    public int HarvestXP => harvestXP;
    public int PlantXP => plantXP;
    public int WaterXP => waterXP;
}
