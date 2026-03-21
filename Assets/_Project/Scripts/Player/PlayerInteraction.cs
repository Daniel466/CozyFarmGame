using UnityEngine;

/// <summary>
/// Handles player interaction with farm tiles via mouse clicks.
/// Left click = interact (till / plant / harvest)
/// Right click = water
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Tool")]
    [SerializeField] private CropData selectedCrop; // Set via UI

    private FarmingManager farming;
    private FarmGrid grid;
    private Camera mainCamera;

    public enum PlayerTool { Till, Plant, Water, Harvest }
    public PlayerTool CurrentTool { get; private set; } = PlayerTool.Till;

    private void Start()
    {
        farming = FarmingManager.Instance;
        grid = GameManager.Instance.FarmGrid;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
            HandleLeftClick();

        if (Input.GetMouseButtonDown(1)) // Right click
            HandleRightClick();
    }

    private void HandleLeftClick()
    {
        Vector2Int? coord = GetClickedTileCoord();
        if (!coord.HasValue) return;

        FarmTile tile = grid.GetTile(coord.Value);
        if (tile == null) return;

        if (!tile.IsTilled)
        {
            farming.TillTile(coord.Value);
        }
        else if (!tile.IsPlanted && selectedCrop != null)
        {
            farming.PlantCrop(coord.Value, selectedCrop);
        }
        else if (tile.IsReadyToHarvest)
        {
            farming.HarvestTile(coord.Value);
        }
    }

    private void HandleRightClick()
    {
        Vector2Int? coord = GetClickedTileCoord();
        if (!coord.HasValue) return;

        farming.WaterTile(coord.Value);
    }

    private Vector2Int? GetClickedTileCoord()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            // Check interaction range
            if (Vector3.Distance(transform.position, hit.point) > interactionRange)
                return null;

            return grid.WorldToGrid(hit.point);
        }
        return null;
    }

    public void SetTool(PlayerTool tool) => CurrentTool = tool;
    public void SetSelectedCrop(CropData crop) => selectedCrop = crop;
    public void SetGroundLayer(LayerMask mask) => groundLayer = mask;
}
