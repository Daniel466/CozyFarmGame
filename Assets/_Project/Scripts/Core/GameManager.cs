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

        // Grab system references
        FarmGrid = GetComponentInChildren<FarmGrid>();
        Inventory = GetComponentInChildren<InventoryManager>();
        Economy = GetComponentInChildren<EconomyManager>();
        Progression = GetComponentInChildren<ProgressionManager>();
        SaveManager = GetComponentInChildren<SaveManager>();
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
