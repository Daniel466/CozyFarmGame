using UnityEngine;

/// <summary>
/// Handles player interaction with farm tiles via mouse clicks.
/// Left click = interact (till / plant / harvest)
/// Right click = water
/// F = sell all
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRange = 10f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Tool")]
    [SerializeField] private CropData selectedCrop;

    private FarmingManager farming;
    private FarmGrid grid;
    private Camera mainCamera;

    public enum PlayerTool { Till, Plant, Water, Harvest }
    public PlayerTool CurrentTool { get; private set; } = PlayerTool.Till;

    private void Start()
    {
        if (GameManager.Instance == null) { Debug.LogError("[PlayerInteraction] GameManager not found!"); enabled = false; return; }
        farming = FarmingManager.Instance;
        if (farming == null) { Debug.LogError("[PlayerInteraction] FarmingManager not found!"); enabled = false; return; }
        grid = GameManager.Instance.FarmGrid;
        if (grid == null) { Debug.LogError("[PlayerInteraction] FarmGrid not found!"); enabled = false; return; }
        mainCamera = Camera.main;
        if (mainCamera == null) { Debug.LogError("[PlayerInteraction] Main Camera not found!"); enabled = false; return; }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        if (Input.GetMouseButtonDown(1)) HandleRightClick();
        if (Input.GetKeyDown(KeyCode.F)) SellAll();
    }

    private void SellAll()
    {
        AudioManager.Instance?.PlaySell();
        int earned = GameManager.Instance.Inventory.SellAll();
        HUDManager.Instance?.ShowNotification(earned > 0
            ? $"Sold everything for {earned} coins!"
            : "Nothing to sell!");
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
        else if (!tile.IsPlanted && selectedCrop == null)
        {
            HUDManager.Instance?.ShowNotification("No crop selected - press B to open shop!");
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
        if (mainCamera == null) return null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = groundLayer.value == 0 ? ~0 : groundLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
        {
            if (Vector3.Distance(transform.position, hit.point) > interactionRange) return null;
            return grid.WorldToGrid(hit.point);
        }
        return null;
    }

    public void SetTool(PlayerTool tool) => CurrentTool = tool;
    public void SetSelectedCrop(CropData crop) => selectedCrop = crop;
    public void SetGroundLayer(LayerMask mask) => groundLayer = mask;
}
