using UnityEngine;

/// <summary>
/// Handles player interaction with farm tiles via mouse clicks.
/// Left click = interact (till / plant / harvest)
/// Right click = water
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRange = 10f;
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
        if (GameManager.Instance == null)
        {
            Debug.LogError("[PlayerInteraction] GameManager not found! Disabling.");
            enabled = false;
            return;
        }

        farming = FarmingManager.Instance;
        if (farming == null)
        {
            Debug.LogError("[PlayerInteraction] FarmingManager not found! Disabling.");
            enabled = false;
            return;
        }

        grid = GameManager.Instance.FarmGrid;
        if (grid == null)
        {
            Debug.LogError("[PlayerInteraction] FarmGrid not found! Disabling.");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[PlayerInteraction] Main Camera not found! Disabling.");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
            HandleLeftClick();

        if (Input.GetMouseButtonDown(1)) // Right click
            HandleRightClick();

        if (Input.GetKeyDown(KeyCode.F)) // F = Sell all instantly
            SellAll();
    }

    private void SellAll()
    {
        AudioManager.Instance?.PlaySell();
        int earned = GameManager.Instance.Inventory.SellAll();
        if (earned > 0)
        {
            HUDManager.Instance?.ShowNotification($"Sold everything for {earned} 🪙!");
            Debug.Log($"[PlayerInteraction] Sold all crops for {earned} coins!");
        }
        else
        {
            HUDManager.Instance?.ShowNotification("Nothing to sell!");
        }
    }

    private void HandleLeftClick()
    {
        Vector2Int? coord = GetClickedTileCoord();
        if (!coord.HasValue)
        {
            Debug.Log("[PlayerInteraction] Left click — no tile hit (missed ground or out of range)");
            return;
        }

        Debug.Log($"[PlayerInteraction] Left click on tile {coord.Value}");

        FarmTile tile = grid.GetTile(coord.Value);
        if (tile == null)
        {
            Debug.Log($"[PlayerInteraction] Tile {coord.Value} not found in grid");
            return;
        }

        if (!tile.IsTilled)
        {
            bool result = farming.TillTile(coord.Value);
            Debug.Log($"[PlayerInteraction] Till result: {result}");
        }
        else if (!tile.IsPlanted && selectedCrop != null)
        {
            bool result = farming.PlantCrop(coord.Value, selectedCrop);
            Debug.Log($"[PlayerInteraction] Plant result: {result}");
        }
        else if (!tile.IsPlanted && selectedCrop == null)
        {
            Debug.Log("[PlayerInteraction] Tile tilled but no crop selected — press B to open shop!");
        }
        else if (tile.IsReadyToHarvest)
        {
            farming.HarvestTile(coord.Value);
            Debug.Log("[PlayerInteraction] Harvested!");
        }
        else
        {
            Debug.Log($"[PlayerInteraction] Tile state: Tilled={tile.IsTilled}, Planted={tile.IsPlanted}, Growth={tile.GrowthProgress:P0}");
        }
    }

    private void HandleRightClick()
    {
        Vector2Int? coord = GetClickedTileCoord();
        if (!coord.HasValue)
        {
            Debug.Log("[PlayerInteraction] Right click — no tile hit");
            return;
        }

        bool result = farming.WaterTile(coord.Value);
        Debug.Log($"[PlayerInteraction] Water result: {result}");
    }

    private Vector2Int? GetClickedTileCoord()
    {
        if (mainCamera == null) return null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Use groundLayer if set, otherwise fall back to Everything so farming always works
        LayerMask mask = groundLayer.value == 0 ? ~0 : groundLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
        {
            float dist = Vector3.Distance(transform.position, hit.point);
            Debug.Log($"[PlayerInteraction] Hit '{hit.collider.name}' on layer '{LayerMask.LayerToName(hit.collider.gameObject.layer)}' dist={dist:F1}m");

            if (dist > interactionRange)
            {
                Debug.Log($"[PlayerInteraction] Too far ({dist:F1}m, max {interactionRange}m) — walk closer!");
                return null;
            }
            return grid.WorldToGrid(hit.point);
        }

        Debug.Log($"[PlayerInteraction] Raycast missed completely. Ground layer mask value: {groundLayer.value}");
        return null;
    }

    public void SetTool(PlayerTool tool) => CurrentTool = tool;
    public void SetSelectedCrop(CropData crop) => selectedCrop = crop;
    public void SetGroundLayer(LayerMask mask) => groundLayer = mask;
}
