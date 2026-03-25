using UnityEngine;

/// <summary>
/// Tracks which tool the player currently has equipped.
/// Watering can has limited capacity — refilled at the water well.
///
/// Tool energy costs (per use):
///   Hoe:         10
///   WateringCan:  5
///   Sickle:       8
///   Seeds:        4  (planting — handled by FarmingManager)
/// </summary>
public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance { get; private set; }

    public enum Tool { None, Hoe, WateringCan, Sickle }

    [Header("Watering Can")]
    [SerializeField] private int wateringCanMaxCapacity = 20;

    public Tool  CurrentTool         { get; private set; } = Tool.Hoe;
    public int   WateringCanCapacity { get; private set; }
    public int   WateringCanMax      => wateringCanMaxCapacity;

    public const int EnergyCostHoe      = 10;
    public const int EnergyCostWater    = 2;
    public const int EnergyCostHarvest  = 8;
    public const int EnergyCostPlant    = 4;

    public event System.Action<Tool>  OnToolChanged;
    public event System.Action<int, int> OnCanChanged; // current, max

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        WateringCanCapacity = wateringCanMaxCapacity;
    }

    // ── Tool switching ───────────────────────────────────────────────────────

    public void EquipHoe()         => SetTool(Tool.Hoe);
    public void EquipWateringCan() => SetTool(Tool.WateringCan);
    public void EquipSickle()      => SetTool(Tool.Sickle);

    private void SetTool(Tool tool)
    {
        CurrentTool = tool;
        OnToolChanged?.Invoke(tool);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipHoe();
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWateringCan();
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipSickle();
    }

    // ── Watering can ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true and decrements capacity if can has water.
    /// </summary>
    public bool TryUseWateringCan()
    {
        if (WateringCanCapacity <= 0)
        {
            HUDManager.Instance?.ShowNotification("Watering can is empty! Refill at the well.", 2.5f);
            return false;
        }

        WateringCanCapacity--;
        OnCanChanged?.Invoke(WateringCanCapacity, wateringCanMaxCapacity);
        return true;
    }

    /// <summary>Refill the watering can to full (called by Water Well interaction).</summary>
    public void RefillWateringCan()
    {
        WateringCanCapacity = wateringCanMaxCapacity;
        OnCanChanged?.Invoke(WateringCanCapacity, wateringCanMaxCapacity);
        HUDManager.Instance?.ShowNotification("Watering can refilled!", 2f);
    }

    // ── Save / Load ──────────────────────────────────────────────────────────

    public ToolSaveData ToSaveData() => new ToolSaveData
    {
        equippedTool        = (int)CurrentTool,
        wateringCanCapacity = WateringCanCapacity,
    };

    public void LoadFromSaveData(ToolSaveData data)
    {
        CurrentTool         = (Tool)data.equippedTool;
        WateringCanCapacity = Mathf.Clamp(data.wateringCanCapacity, 0, wateringCanMaxCapacity);
    }
}

[System.Serializable]
public class ToolSaveData
{
    public int equippedTool;
    public int wateringCanCapacity;
}
