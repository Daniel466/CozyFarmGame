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

    // Hover highlight
    private GameObject hoverQuad;
    private Renderer hoverRenderer;
    private static readonly Color ColourEmpty    = new Color(0.2f, 0.9f, 0.2f, 0.30f);   // green
    private static readonly Color ColourPlanted  = new Color(1.0f, 0.9f, 0.1f, 0.30f);   // yellow
    private static readonly Color ColourWatered  = new Color(0.2f, 0.5f, 1.0f, 0.30f);   // blue
    private static readonly Color ColourHarvest  = new Color(1.0f, 0.55f, 0.1f, 0.30f);  // orange/glow
    private const float PulsePeriod = 2.0f;   // seconds per full cycle
    private const float PulseScaleMin = 0.95f;
    private const float PulseScaleMax = 1.05f;

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
        hoverQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        hoverQuad.name = "TileHoverHighlight";
        hoverQuad.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        hoverQuad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        hoverQuad.layer = 2; // Ignore Raycast

        // Destroy collider so it never intercepts raycasts
        Destroy(hoverQuad.GetComponent<Collider>());

        hoverRenderer = hoverQuad.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        // URP transparent surface setup
        mat.SetFloat("_Surface", 1f);                      // 1 = Transparent
        mat.SetFloat("_Blend", 0f);                        // 0 = Alpha blend
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Set colour via _BaseColor (URP) — alpha 0.3 for soft glow
        var startColour = new Color(0f, 1f, 0f, 0.3f);
        mat.SetColor("_BaseColor", startColour);
        mat.color = startColour;

        hoverRenderer.material = mat;
        hoverRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        hoverRenderer.receiveShadows = false;

        hoverQuad.SetActive(false);
    }

    private void Update()
    {
        UpdateHoverHighlight();
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        if (Input.GetMouseButtonDown(1)) HandleRightClick();
        if (Input.GetKeyDown(KeyCode.F)) SellAll();
    }

    private void UpdateHoverHighlight()
    {
        Vector2Int? coord = GetHoveredTileCoord();
        if (!coord.HasValue)
        {
            hoverQuad.SetActive(false);
            return;
        }

        FarmTile tile = grid.GetTile(coord.Value);
        if (tile == null)
        {
            hoverQuad.SetActive(false);
            return;
        }

        Vector3 worldPos = grid.GridToWorld(coord.Value);
        hoverQuad.transform.position = worldPos + Vector3.up * 0.25f;
        hoverQuad.SetActive(true);

        Color c;
        if      (tile.IsReadyToHarvest)         c = ColourHarvest;
        else if (tile.IsWatered)                c = ColourWatered;
        else if (tile.IsPlanted)                c = ColourPlanted;
        else                                    c = ColourEmpty;

        hoverRenderer.material.SetColor("_BaseColor", c);
        hoverRenderer.material.color = c;

        // Scale pulse: gentle breathe on XY, period 2 seconds
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * (Mathf.PI * 2f / PulsePeriod));
        float s = Mathf.Lerp(PulseScaleMin, PulseScaleMax, t);
        hoverQuad.transform.localScale = new Vector3(2.5f * s, 2.5f * s, 1f);
    }

    // Hover version — no range limit for display, range checked in GetClickedTileCoord for actions
    private Vector2Int? GetHoveredTileCoord()
    {
        if (mainCamera == null) return null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = groundLayer.value == 0 ? ~0 : groundLayer;
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, mask)) return null;
        if (Vector3.Distance(transform.position, hit.point) > interactionRange) return null;
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
        Vector2Int? coord = GetClickedTileCoord();
        if (!coord.HasValue) return;

        FarmTile tile = grid.GetTile(coord.Value);
        if (tile == null) return;

        // Flower beds are pre-tilled — no till step needed (Farm Together style)
        if (tile.IsReadyToHarvest)
        {
            farming.HarvestTile(coord.Value);
        }
        else if (!tile.IsPlanted && selectedCrop != null)
        {
            farming.PlantCrop(coord.Value, selectedCrop);
        }
        else if (!tile.IsPlanted && selectedCrop == null)
        {
            HUDManager.Instance?.ShowNotification("No crop selected - press B to open shop!");
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
