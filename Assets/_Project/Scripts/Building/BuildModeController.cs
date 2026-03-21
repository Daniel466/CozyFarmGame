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
        groundMask = ~0; // Everything for now
    }

    private void Update()
    {
        // Toggle build mode UI with B key (Mac-friendly)
        // Note: B is also used for shop — build mode takes priority when not farming
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (buildingManager.IsInBuildMode)
                buildingManager.ExitBuildMode();
            else
                BuildModeUI.Instance?.ToggleBuildPanel();
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
