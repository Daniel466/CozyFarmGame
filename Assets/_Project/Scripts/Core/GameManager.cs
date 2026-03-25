using System.Collections;
using UnityEngine;

/// <summary>
/// Central singleton — holds references to all major systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public FarmGrid           FarmGrid        { get; private set; }
    public InventoryManager   Inventory       { get; private set; }
    public EconomyManager     Economy         { get; private set; }
    public ProgressionManager Progression     { get; private set; }
    public SaveManager        SaveManager     { get; private set; }
    public BuildingManager    BuildingManager { get; private set; }
    public ToolManager        ToolManager     { get; private set; }
    public RealTimeManager    RealTime        { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        FarmGrid    = GetComponent<FarmGrid>();
        Inventory   = GetComponent<InventoryManager>();
        Economy     = GetComponent<EconomyManager>();
        Progression = GetComponent<ProgressionManager>();
        SaveManager = GetComponent<SaveManager>();

        BuildingManager = FindFirstObjectByType<BuildingManager>();
        ToolManager     = FindFirstObjectByType<ToolManager>();
        RealTime        = GetComponent<RealTimeManager>();

        if (FarmGrid  == null) Debug.LogError("[GameManager] FarmGrid missing!");
        if (Inventory == null) Debug.LogError("[GameManager] InventoryManager missing!");
        if (Economy   == null) Debug.LogError("[GameManager] EconomyManager missing!");
    }

    private void Start()
    {
        StartCoroutine(LoadAfterStart());
    }

    private IEnumerator LoadAfterStart()
    {
        yield return null;
        SaveManager?.LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveManager?.SaveGame();
    }
}
