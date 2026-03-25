using UnityEngine;
using System.Collections;

/// <summary>
/// Handles player interaction with farm tiles via mouse clicks.
/// Left click  = walk to tile, then plant / harvest
/// Right click = walk to tile, then water
/// F           = sell all
///
/// Flow: click → auto-walk → face tile → animation → action fires mid-animation
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Distance at which the player stops walking toward a tile before acting.")]
    [SerializeField] private float walkStopDistance   = 2.5f;
    [Tooltip("Maximum raycast distance for the hover highlight and click detection.")]
    [SerializeField] private float hoverRange         = 10f;
    [SerializeField] private float plantActionDelay   = 0.6f; // seconds into anim when crop spawns
    [SerializeField] private float waterActionDelay   = 0.4f;
    [SerializeField] private float harvestActionDelay = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Tool")]
    [SerializeField] private CropData selectedCrop;

    [Header("References")]
    [SerializeField] private PlayerController playerController;

    private Coroutine pendingInteraction;

    private FarmingManager farming;
    private FarmGrid grid;
    private Camera mainCamera;

    // Hover highlight — four-edge outline
    private GameObject hoverRoot;
    private Material hoverMaterial;  // shared across all 4 edges
    private static readonly Color ColourEmpty    = new Color(0.4f, 1.0f, 0.4f, 0.80f);   // green
    private static readonly Color ColourPlanted  = new Color(1.0f, 0.9f, 0.1f, 0.80f);   // yellow
    private static readonly Color ColourWatered  = new Color(0.2f, 0.5f, 1.0f, 0.80f);   // blue
    private static readonly Color ColourHarvest  = new Color(1.0f, 0.55f, 0.1f, 0.80f);  // orange
    private const float PulsePeriod   = 2.0f;
    private const float PulseScaleMin = 0.97f;
    private const float PulseScaleMax = 1.03f;

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

        CreateHoverQuad();
    }

    private void CreateHoverQuad()
    {
        hoverRoot = new GameObject("TileHoverHighlight");

        // Shared transparent URP material — one SetColor call updates all 4 edges
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
                  ?? Shader.Find("Unlit/Color")
                  ?? Shader.Find("Standard");
        hoverMaterial = new Material(shader);
        hoverMaterial.SetFloat("_Surface", 1f);
        hoverMaterial.SetFloat("_Blend", 0f);
        hoverMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        hoverMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        hoverMaterial.SetFloat("_ZWrite", 0f);
        hoverMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        hoverMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        hoverMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        hoverMaterial.SetColor("_BaseColor", ColourEmpty);

        // Tile footprint: 3.5 × 3.5 units. Border thickness: 0.1 units.
        // Top/Bottom edges span the full 3.5 width.
        // Left/Right edges span the inner 3.3 height (3.5 - 2 × 0.1) to fill the gap cleanly.
        const float tile      = 3.5f;
        const float thick     = 0.1f;
        const float half      = tile  / 2f;          // 1.75
        const float innerHalf = (tile - 2f * thick) / 2f; // 1.65

        CreateEdge(new Vector3( 0,     0,  half), new Vector3(tile,  thick, 1f)); // top
        CreateEdge(new Vector3( 0,     0, -half), new Vector3(tile,  thick, 1f)); // bottom
        CreateEdge(new Vector3(-half,  0,  0),    new Vector3(thick, tile - 2f * thick, 1f)); // left
        CreateEdge(new Vector3( half,  0,  0),    new Vector3(thick, tile - 2f * thick, 1f)); // right

        hoverRoot.SetActive(false);
    }

    private void CreateEdge(Vector3 localPos, Vector3 localScale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.transform.SetParent(hoverRoot.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale    = localScale;
        go.layer = 2; // Ignore Raycast
        Destroy(go.GetComponent<Collider>());
        var r = go.GetComponent<Renderer>();
        r.material            = hoverMaterial;
        r.shadowCastingMode   = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows      = false;
    }

    private void Update()
    {
        UpdateHoverHighlight();
        UpdateToolText();
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        if (Input.GetMouseButtonDown(1)) HandleRightClick();
        if (Input.GetKeyDown(KeyCode.F)) SellAll();
    }

    private bool IsInBuildMode => BuildingManager.Instance != null && BuildingManager.Instance.IsInBuildMode;

    private void UpdateHoverHighlight()
    {
        if (IsInBuildMode)
        {
            hoverRoot.SetActive(false);
            return;
        }

        Vector2Int? coord = GetHoveredTileCoord();
        if (!coord.HasValue)
        {
            hoverRoot.SetActive(false);
            HUDManager.Instance?.SetContextHint("B: Shop  /  Tab: Inventory  /  G: Build");
            HUDManager.Instance?.HideTileInfo();
            return;
        }

        FarmTile tile = grid.GetTile(coord.Value);
        if (tile == null)
        {
            hoverRoot.SetActive(false);
            HUDManager.Instance?.SetContextHint("B: Shop  /  Tab: Inventory  /  G: Build");
            HUDManager.Instance?.HideTileInfo();
            return;
        }

        Vector3 worldPos = grid.GridToWorld(coord.Value);
        hoverRoot.transform.position = worldPos + Vector3.up * 0.04f;
        hoverRoot.SetActive(true);

        Color c;
        if      (tile.IsReadyToHarvest)  c = ColourHarvest;
        else if (tile.IsPlanted)         c = ColourPlanted;  // yellow for all planted (watered or not)
        else                             c = ColourEmpty;

        hoverMaterial.SetColor("_BaseColor", c);

        // Context hint — state-aware with grow time
        if (tile.IsReadyToHarvest)
        {
            HUDManager.Instance?.SetContextHint($"Left Click to Harvest  -  {tile.PlantedCrop.CropName} is ready!");
            HUDManager.Instance?.ShowTileInfo(tile);
        }
        else if (tile.IsPlanted)
        {
            float secs = tile.GetRemainingSeconds();
            string timeStr = FormatGrowTime(secs);
            string waterHint = tile.IsWatered ? "" : "  -  Right Click to Water";
            HUDManager.Instance?.SetContextHint($"Growing: {timeStr}{waterHint}");
            HUDManager.Instance?.ShowTileInfo(tile);
        }
        else
        {
            HUDManager.Instance?.SetContextHint("Left Click to Plant");
            HUDManager.Instance?.ShowTileInfo(tile);
        }

        // Pulse: scale the root so all 4 edges breathe together
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * (Mathf.PI * 2f / PulsePeriod));
        float s = Mathf.Lerp(PulseScaleMin, PulseScaleMax, t);
        hoverRoot.transform.localScale = Vector3.one * s;
    }

    // Hover version — no range limit for display, range checked in GetClickedTileCoord for actions
    private Vector2Int? GetHoveredTileCoord()
    {
        if (mainCamera == null) return null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = groundLayer.value == 0 ? ~0 : groundLayer;
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, mask)) return null;
        if (Vector3.Distance(transform.position, hit.point) > hoverRange) return null;
        Vector2Int coord = grid.WorldToGrid(hit.point);
        return grid.GetTile(coord) != null ? coord : null;
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
        if (IsInBuildMode) return;
        Vector2Int? coord = GetClickedTileCoord();
        if (!coord.HasValue) return;

        FarmTile tile = grid.GetTile(coord.Value);
        if (tile == null) return;

        if (!tile.IsReadyToHarvest && tile.IsPlanted)
        {
            // Already growing — cancel any pending interaction and do nothing
            CancelPendingInteraction();
            return;
        }
        if (!tile.IsPlanted && selectedCrop == null)
        {
            CancelPendingInteraction();
            HUDManager.Instance?.ShowNotification("No crop selected - press B to open shop!");
            return;
        }

        // Snapshot crop at click time — prevents mid-walk shop changes from affecting this action
        StartInteraction(coord.Value, isWater: false, selectedCrop);
    }

    private void CancelPendingInteraction()
    {
        if (pendingInteraction != null)
        {
            StopCoroutine(pendingInteraction);
            pendingInteraction = null;
        }
        playerController?.EndAction();
        playerController?.CancelAutoMove();
    }

    private void HandleRightClick()
    {
        if (IsInBuildMode) return;
        Vector2Int? coord = GetClickedTileCoord();
        if (!coord.HasValue) return;
        StartInteraction(coord.Value, isWater: true, null);
    }

    private void StartInteraction(Vector2Int coord, bool isWater, CropData crop)
    {
        CancelPendingInteraction();
        pendingInteraction = StartCoroutine(WalkThenAct(coord, isWater, crop));
    }

    private IEnumerator WalkThenAct(Vector2Int coord, bool isWater, CropData crop)
    {
        Vector3 tileWorldPos = grid.GridToWorld(coord);

        // --- Walk to tile (skip if already close enough) ---
        float distToTile = Vector3.Distance(transform.position, tileWorldPos);
        if (distToTile > walkStopDistance)
        {
            bool arrived = false;
            playerController?.WalkTo(tileWorldPos, walkStopDistance, () => arrived = true);

            yield return new WaitUntil(() => arrived || playerController == null || !playerController.IsAutoMoving);

            // Player pressed WASD mid-walk — abort
            if (!arrived) yield break;
        }

        // --- Face the tile ---
        playerController?.FacePosition(tileWorldPos);
        yield return null; // one frame for rotation to apply

        // --- Re-read tile state (may have changed while walking) ---
        FarmTile tile = grid.GetTile(coord);
        if (tile == null) yield break;

        if (isWater)
        {
            if (!tile.IsPlanted || tile.IsWatered)
            {
                HUDManager.Instance?.ShowNotification(tile.IsWatered ? "Already watered!" : "Nothing planted here!");
                yield break;
            }
            playerController?.TriggerWater();
            yield return new WaitForSeconds(waterActionDelay);
            farming.WaterTile(coord);
            playerController?.EndAction();
        }
        else if (tile.IsReadyToHarvest)
        {
            playerController?.TriggerHarvest();
            yield return new WaitForSeconds(harvestActionDelay);
            farming.HarvestTile(coord);
            playerController?.EndAction();
        }
        else if (!tile.IsPlanted && crop != null)
        {
            playerController?.TriggerPlant();
            yield return new WaitForSeconds(plantActionDelay);
            farming.PlantCrop(coord, crop);
            playerController?.EndAction();
        }
        else if (!tile.IsPlanted && crop == null)
        {
            HUDManager.Instance?.ShowNotification("No crop selected - press B to open shop!");
        }

        pendingInteraction = null;
    }

    private Vector2Int? GetClickedTileCoord()
    {
        if (mainCamera == null) return null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = groundLayer.value == 0 ? ~0 : groundLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
        {
            Vector2Int coord = grid.WorldToGrid(hit.point);
            return grid.GetTile(coord) != null ? coord : null;
        }
        return null;
    }

    public void SetTool(PlayerTool tool) => CurrentTool = tool;

    /// <summary>Returns the crop currently selected by the player, or null if none.</summary>
    public CropData SelectedCrop => selectedCrop;

    public void SetSelectedCrop(CropData crop)
    {
        selectedCrop = crop;
        HUDManager.Instance?.ShowSelectedCrop(crop);
    }

    public void SetGroundLayer(LayerMask mask) => groundLayer = mask;

    private string lastToolText;
    private void UpdateToolText()
    {
        string desired;
        if (BuildModeUI.Instance != null && BuildModeUI.Instance.IsOpen)
            desired = "Build Mode";
        else if (Input.GetMouseButton(1))
            desired = "Watering Can";
        else if (selectedCrop != null)
            desired = $"Planting: {selectedCrop.CropName}";
        else
            desired = "Farming Mode";

        if (desired == lastToolText) return;
        lastToolText = desired;
        HUDManager.Instance?.UpdateToolIndicator(desired);
    }

    private static string FormatGrowTime(float seconds)
    {
        if (seconds <= 0f) return "Ready!";
        int m = (int)seconds / 60;
        int s = (int)seconds % 60;
        return m > 0 ? $"{m}m {s:D2}s" : $"{s}s";
    }
}
