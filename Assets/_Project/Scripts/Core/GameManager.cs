using System.Collections;
using UnityEngine;

/// <summary>
/// Central singleton — holds references to all major systems.
/// New systems (Time, Energy, Tools) are found on child/scene objects.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Systems (on this GameObject)")]
    public FarmGrid         FarmGrid    { get; private set; }
    public InventoryManager Inventory   { get; private set; }
    public EconomyManager   Economy     { get; private set; }
    public ProgressionManager Progression { get; private set; }
    public SaveManager      SaveManager { get; private set; }
    public BuildingManager  BuildingManager { get; private set; }

    [Header("New Phase-1 Systems (found in scene)")]
    public GameTimeManager  TimeManager   { get; private set; }
    public EnergyManager    EnergyManager { get; private set; }
    public ToolManager      ToolManager   { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Systems on this GameObject
        FarmGrid    = GetComponent<FarmGrid>();
        Inventory   = GetComponent<InventoryManager>();
        Economy     = GetComponent<EconomyManager>();
        Progression = GetComponent<ProgressionManager>();
        SaveManager = GetComponent<SaveManager>();

        // Systems in the scene
        BuildingManager = FindFirstObjectByType<BuildingManager>();
        TimeManager     = FindFirstObjectByType<GameTimeManager>();
        EnergyManager   = FindFirstObjectByType<EnergyManager>();
        ToolManager     = FindFirstObjectByType<ToolManager>();

        if (FarmGrid    == null) Debug.LogError("[GameManager] FarmGrid missing!");
        if (Inventory   == null) Debug.LogError("[GameManager] InventoryManager missing!");
        if (Economy     == null) Debug.LogError("[GameManager] EconomyManager missing!");
        if (TimeManager == null) Debug.LogWarning("[GameManager] GameTimeManager not found — add it to the scene.");
        if (EnergyManager == null) Debug.LogWarning("[GameManager] EnergyManager not found.");
        if (ToolManager == null) Debug.LogWarning("[GameManager] ToolManager not found.");
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
