using UnityEngine;

/// <summary>
/// Central game manager — singleton that holds references to all major systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Systems")]
    public FarmGrid FarmGrid { get; private set; }
    public InventoryManager Inventory { get; private set; }
    public EconomyManager Economy { get; private set; }
    public ProgressionManager Progression { get; private set; }
    public SaveManager SaveManager { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Grab system references (all on same GameObject)
        FarmGrid = GetComponent<FarmGrid>();
        Inventory = GetComponent<InventoryManager>();
        Economy = GetComponent<EconomyManager>();
        Progression = GetComponent<ProgressionManager>();
        SaveManager = GetComponent<SaveManager>();

        if (FarmGrid == null) Debug.LogError("[GameManager] FarmGrid component missing!");
        if (Inventory == null) Debug.LogError("[GameManager] InventoryManager component missing!");
        if (Economy == null) Debug.LogError("[GameManager] EconomyManager component missing!");
        if (Progression == null) Debug.LogError("[GameManager] ProgressionManager component missing!");
        if (SaveManager == null) Debug.LogWarning("[GameManager] SaveManager component missing — saves disabled.");
    }

    private void Start()
    {
        SaveManager?.LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveManager?.SaveGame();
    }
}
