using UnityEngine;

/// <summary>
/// Handles keyboard input to toggle build mode and remove buildings.
/// Attach to the Player or a persistent manager GameObject.
/// F5 = Toggle build mode UI
/// Delete/Backspace = Remove building under cursor
/// </summary>
public class BuildModeController : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private BuildingDatabase buildingDatabase;

    private BuildingManager buildingManager;
    private FarmGrid grid;
    private Camera mainCamera;
    private LayerMask groundMask;

    private void Start()
    {
        buildingManager = BuildingManager.Instance;
        grid = GameManager.Instance?.FarmGrid;
        mainCamera = Camera.main;
        groundMask = ~0;

        if (buildingManager == null)
            Debug.LogError("[BuildModeController] BuildingManager.Instance is null! Add BuildingManager to GameManager.");
        if (BuildModeUI.Instance == null)
        {
            // Try to find it in scene in case Awake order was off
            var found = FindFirstObjectByType<BuildModeUI>();
            if (found != null)
                Debug.Log("[BuildModeController] Found BuildModeUI via FindFirstObjectByType.");
            else
                Debug.LogError("[BuildModeController] BuildModeUI not found! Make sure HUDBootstrapper has BuildingDatabase assigned and HUD GameObject is in scene.");
        }

        Debug.Log($"[BuildModeController] Ready. BuildingManager={buildingManager != null}, BuildModeUI={BuildModeUI.Instance != null}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Lazy lookup in case systems weren't ready at Start
            if (buildingManager == null)
                buildingManager = BuildingManager.Instance;

            var buildUI = BuildModeUI.Instance ?? FindFirstObjectByType<BuildModeUI>();

            Debug.Log($"[BuildModeController] G pressed. buildingManager={buildingManager != null}, BuildModeUI={buildUI != null}");

            if (buildingManager == null) { Debug.LogError("[BuildModeController] No BuildingManager!"); return; }
            if (buildUI == null) { Debug.LogError("[BuildModeController] No BuildModeUI! Is HUD in scene with BuildingDatabase assigned?"); return; }

            if (buildingManager.IsInBuildMode)
                buildingManager.ExitBuildMode();
            else
                buildUI.ToggleBuildPanel();
        }

        // Remove building with Delete key
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            TryRemoveBuilding();
    }

    private void TryRemoveBuilding()
    {
        if (mainCamera == null || grid == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            Vector2Int coord = grid.WorldToGrid(hit.point);
            bool removed = buildingManager.RemoveBuilding(coord);
            if (!removed)
                HUDManager.Instance?.ShowNotification("No building to remove here.");
        }
    }
}
