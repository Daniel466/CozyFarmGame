using UnityEngine;

/// <summary>
/// ScriptableObject defining all data for a placeable building or decoration.
/// Create via: Right-click > Create > CozyFarm > Building Data
/// </summary>
[CreateAssetMenu(fileName = "NewBuilding", menuName = "CozyFarm/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string buildingId;
    [SerializeField] private string buildingName;
    [SerializeField] [TextArea(2, 4)] private string description;
    [SerializeField] private Sprite icon;

    [Header("Type")]
    [SerializeField] private BuildingType buildingType;

    [Header("Progression")]
    [SerializeField] private int unlockLevel = 1;

    [Header("Economy")]
    [SerializeField] private int cost = 100;

    [Header("Size")]
    [SerializeField] private Vector2Int size = Vector2Int.one; // Grid cells occupied

    [Header("Visuals")]
    [SerializeField] private GameObject prefab; // Real 3D model (optional)
    [SerializeField] private Color placeholderColor = new Color(0.6f, 0.4f, 0.2f);

    [Header("XP")]
    [SerializeField] private int placeXP = 10;

    [Header("Auto Watering (0 = disabled)")]
    [SerializeField] private int autoWaterRadius = 0;
    [SerializeField] private float autoWaterInterval = 30f;

    [Header("Auto Selling (0 = disabled)")]
    [SerializeField] private float autoSellInterval = 0f;  // Seconds between auto-sell cycles
    [SerializeField] private float autoSellBonus = 0.1f;   // 0.1 = 10% bonus on top of sell value

    [Header("Dog System")]
    [Tooltip("If true, placing this building spawns the dog companion and removing it despawns it.")]
    [SerializeField] private bool isDoghouse = false;

    // Public accessors
    public string BuildingId => buildingId;
    public string BuildingName => buildingName;
    public string Description => description;
    public Sprite Icon => icon;
    public BuildingType Type => buildingType;
    public int UnlockLevel => unlockLevel;
    public int Cost => cost;
    public Vector2Int Size => size;
    public GameObject Prefab => prefab;
    public Color PlaceholderColor => placeholderColor;
    public int PlaceXP => placeXP;
    public int AutoWaterRadius => autoWaterRadius;
    public float AutoWaterInterval => autoWaterInterval;
    public float AutoSellInterval => autoSellInterval;
    public float AutoSellBonus => autoSellBonus;
    public bool IsDoghouse => isDoghouse;
}

public enum BuildingType
{
    Functional,   // Barn, Greenhouse, Silo, etc.
    Decoration    // Fences, flowers, paths, etc.
}
