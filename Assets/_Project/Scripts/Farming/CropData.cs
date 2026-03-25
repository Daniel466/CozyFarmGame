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

    [Header("Growth")]
    [Tooltip("Seconds from planting to ready-to-harvest. 120 = 2 min (test). 3600 = 1 hour (release).")]
    [SerializeField] private float growthTimeSeconds = 120f;

    [Header("Economy")]
    [SerializeField] private int seedCost  = 20;
    [SerializeField] private int sellValue = 50;

    [Header("Model")]
    [SerializeField] private GameObject[] growthStagePrefabs = new GameObject[4];
    [SerializeField] private Vector3      modelRotationOffset = Vector3.zero;
    [SerializeField] private float        modelBaseScale      = 1f;

    public string       CropId              => cropId;
    public string       CropName            => cropName;
    public Sprite       Icon                => icon;
    public float        GrowthTimeSeconds   => Mathf.Max(1f, growthTimeSeconds);
    public int          SeedCost            => seedCost;
    public int          SellValue           => sellValue;
    public GameObject[] GrowthStagePrefabs  => growthStagePrefabs;
    public Vector3      ModelRotationOffset => modelRotationOffset;
    public float        ModelBaseScale      => modelBaseScale;
}
