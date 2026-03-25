using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles all player interaction with the farm grid.
///
/// Tool model (GDD):
///   1 - Hoe      : till tile(s)
///   2 - Seed     : plant selected crop
///   3 - Harvest  : harvest ripe crop(s)
///   4 - Remove   : remove crop
///   5 - Build    : enter building placement mode
///
/// Single click  : walk to tile, then apply tool
/// Hold + drag   : instantly paint tool across multiple tiles (no walk)
/// Right click   : reset to Hoe tool
/// F             : sell all
/// Tab           : toggle inventory
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    // ── Tool enum ─────────────────────────────────────────────────────────────

    public enum FarmTool { Hoe, Seed, Harvest, Remove, Build }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Settings")]
    [SerializeField] private float walkStopDistance = 2.5f;
    [SerializeField] private float hoverRange       = 10f;
    [SerializeField] private float actionDelay      = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Hover")]
    [SerializeField] private float hoverYOffset = 0.05f;

    [Header("Tool")]
    [SerializeField] private FarmTool activeTool  = FarmTool.Hoe;
    [SerializeField] private CropData selectedCrop;

    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Debug")]
    [SerializeField] private bool debugRaycast = false;

    // ── Hover highlight ───────────────────────────────────────────────────────

    private GameObject hoverRoot;
    private Material   hoverMaterial;

    private static readonly Color ColourValid   = new Color(0.4f, 1.0f, 0.4f, 0.80f);  // green  — valid action
    private static readonly Color ColourHarvest = new Color(1.0f, 0.55f, 0.1f, 0.80f); // orange — ready to harvest
    private static readonly Color ColourRemove  = new Color(1.0f, 0.2f,  0.2f, 0.80f); // red    — remove tool
    private static readonly Color ColourGrowing = new Color(1.0f, 0.9f,  0.1f, 0.80f); // yellow — planted, growing
    private static readonly Color ColourInvalid = new Color(0.5f, 0.5f,  0.5f, 0.30f); // grey   — can't act here

    private const float PulsePeriod   = 2.0f;

    // ── Area drag ─────────────────────────────────────────────────────────────

    private bool                isDragging;
    private HashSet<Vector2Int> draggedTiles = new();

    // ── Walk-to (single click) ────────────────────────────────────────────────

    private Coroutine pendingInteraction;

    // ── Runtime refs ──────────────────────────────────────────────────────────

    private FarmingManager farming;
    private FarmGrid       grid;
    private Camera         mainCamera;

    private const string FARM_INTERACT_LAYER = "FarmInteract";
    private bool hasWarnedAboutMissingLayer;

    // ── Public accessors ──────────────────────────────────────────────────────

    public FarmTool  ActiveTool   => activeTool;
    public CropData  SelectedCrop => selectedCrop;

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (GameManager.Instance == null) { Debug.LogError("[PlayerInteraction] GameManager not found!"); enabled = false; return; }
        farming = FarmingManager.Instance;
        if (farming == null) { Debug.LogError("[PlayerInteraction] FarmingManager not found!"); enabled = false; return; }
        grid = GameManager.Instance.FarmGrid;
        if (grid == null) { Debug.LogError("[PlayerInteraction] FarmGrid not found!"); enabled = false; return; }
        mainCamera = Camera.main;
        if (mainCamera == null) { Debug.LogError("[PlayerInteraction] Main Camera not found!"); enabled = false; return; }

        CreateHoverHighlight();
        NotifyToolChanged();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        HandleToolHotkeys();
        UpdateHoverHighlight();
        UpdateToolText();

        if (Input.GetMouseButtonDown(0))  HandleMouseDown();
        if (Input.GetMouseButton(0))      HandleMouseHeld();
        if (Input.GetMouseButtonUp(0))    HandleMouseUp();
        if (Input.GetMouseButtonDown(1))  SetTool(FarmTool.Hoe); // right-click resets to Hoe
        if (Input.GetKeyDown(KeyCode.F))  SellAll();
    }

    // ── Tool hotkeys ──────────────────────────────────────────────────────────

    private void HandleToolHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetTool(FarmTool.Hoe);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetTool(FarmTool.Seed);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetTool(FarmTool.Harvest);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetTool(FarmTool.Remove);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetTool(FarmTool.Build);
    }

    public void SetTool(FarmTool tool)
    {
        activeTool = tool;
        NotifyToolChanged();

        if (tool == FarmTool.Build)
            HUDManager.Instance?.ShowNotification("Build tool selected - open build menu with G");
    }

    public void SetSelectedCrop(CropData crop)
    {
        selectedCrop = crop;
        activeTool   = FarmTool.Seed;
        NotifyToolChanged();
        HUDManager.Instance?.ShowSelectedCrop(crop);
    }

    private void NotifyToolChanged()
    {
        UpdateToolText();
    }

    // ── Mouse input ───────────────────────────────────────────────────────────

    private void HandleMouseDown()
    {
        if (IsInBuildMode) return;

        Vector2Int? coord = GetHoveredTileCoord();
        if (!coord.HasValue) return;

        isDragging = true;
        draggedTiles.Clear();

        // Single tile: walk-to then act (only when not dragging yet)
        // We start the walk immediately; if the player drags, we'll cancel it below
        StartSingleClickInteraction(coord.Value);
    }

    private void HandleMouseHeld()
    {
        if (!isDragging || IsInBuildMode) return;

        Vector2Int? coord = GetHoveredTileCoord();
        if (!coord.HasValue) return;

        // Once the player moves to a second tile, switch to instant drag mode
        if (draggedTiles.Count == 1 && !draggedTiles.Contains(coord.Value))
        {
            // Cancel the walk-to — we're dragging now
            CancelPendingInteraction();
        }

        if (draggedTiles.Contains(coord.Value)) return;

        draggedTiles.Add(coord.Value);
        ApplyToolInstant(coord.Value);
    }

    private void HandleMouseUp()
    {
        isDragging = false;
        draggedTiles.Clear();
    }

    // ── Single click — walk to tile, then act ─────────────────────────────────

    private void StartSingleClickInteraction(Vector2Int coord)
    {
        draggedTiles.Add(coord);
        CancelPendingInteraction();
        pendingInteraction = StartCoroutine(WalkThenAct(coord));
    }

    private IEnumerator WalkThenAct(Vector2Int coord)
    {
        Vector3 tileWorldPos = grid.GridToWorld(coord);

        float dist = Vector3.Distance(transform.position, tileWorldPos);
        if (dist > walkStopDistance)
        {
            bool arrived = false;
            playerController?.WalkTo(tileWorldPos, walkStopDistance, () => arrived = true);
            yield return new WaitUntil(() => arrived || playerController == null || !playerController.IsAutoMoving);
            if (!arrived) yield break; // WASD cancelled mid-walk
        }

        playerController?.FacePosition(tileWorldPos);
        yield return null;

        ApplyToolWithAnimation(coord);
        pendingInteraction = null;
    }

    private void ApplyToolWithAnimation(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null) return;

        switch (activeTool)
        {
            case FarmTool.Hoe:
                if (!tile.IsTilled)
                {
                    playerController?.TriggerPlant();
                    StartCoroutine(DelayedAction(actionDelay, () =>
                    {
                        farming.TillTile(coord);
                        playerController?.EndAction();
                    }));
                }
                break;

            case FarmTool.Seed:
                if (selectedCrop == null)
                {
                    HUDManager.Instance?.ShowNotification("No crop selected!");
                    break;
                }
                if (!tile.IsPlanted)
                {
                    playerController?.TriggerPlant();
                    StartCoroutine(DelayedAction(actionDelay, () =>
                    {
                        farming.PlantCrop(coord, selectedCrop);
                        playerController?.EndAction();
                    }));
                }
                break;

            case FarmTool.Harvest:
                if (tile.IsReadyToHarvest)
                {
                    playerController?.TriggerHarvest();
                    StartCoroutine(DelayedAction(actionDelay, () =>
                    {
                        farming.HarvestTile(coord);
                        playerController?.EndAction();
                    }));
                }
                break;

            case FarmTool.Remove:
                if (tile.IsPlanted)
                {
                    farming.RemoveCrop(coord);
                }
                break;
        }
    }

    private IEnumerator DelayedAction(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
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

    // ── Instant apply (area drag) ─────────────────────────────────────────────

    private void ApplyToolInstant(Vector2Int coord)
    {
        FarmTile tile = grid.GetTile(coord);
        if (tile == null) return;

        switch (activeTool)
        {
            case FarmTool.Hoe:
                farming.TillTile(coord);
                break;

            case FarmTool.Seed:
                if (selectedCrop != null && !tile.IsPlanted)
                    farming.PlantCrop(coord, selectedCrop);
                break;

            case FarmTool.Harvest:
                farming.HarvestTile(coord);
                break;

            case FarmTool.Remove:
                farming.RemoveCrop(coord);
                break;
        }
    }

    // ── Hover highlight ───────────────────────────────────────────────────────

    private void UpdateHoverHighlight()
    {
        if (hoverRoot == null) return;

        if (IsInBuildMode) { hoverRoot.SetActive(false); return; }

        Vector2Int? coord = GetHoveredTileCoord();
        if (!coord.HasValue)
        {
            hoverRoot.SetActive(false);
            HUDManager.Instance?.SetContextHint("1:Hoe  2:Seed  3:Harvest  4:Remove  5:Build");
            HUDManager.Instance?.HideTileInfo();
            return;
        }

        FarmTile tile = grid.GetTile(coord.Value);
        if (tile == null) { hoverRoot.SetActive(false); return; }

        hoverRoot.transform.position = grid.GridToWorld(coord.Value) + Vector3.up * hoverYOffset;
        hoverRoot.SetActive(true);

        // Pulse the alpha instead of scaling — no wobble, same visual feedback
        Color baseColour = GetHoverColour(tile);
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * (Mathf.PI * 2f / PulsePeriod));
        float pulseAlpha = Mathf.Lerp(baseColour.a * 0.6f, baseColour.a, t);
        baseColour.a = pulseAlpha;
        hoverMaterial.SetColor(colorPropertyId, baseColour);

        UpdateContextHint(tile);
        HUDManager.Instance?.ShowTileInfo(tile);
    }

    private Color GetHoverColour(FarmTile tile)
    {
        switch (activeTool)
        {
            case FarmTool.Hoe:
                return !tile.IsTilled ? ColourValid : ColourInvalid;

            case FarmTool.Seed:
                if (tile.IsPlanted)  return ColourGrowing;
                if (!tile.IsTilled)  return ColourValid;  // auto-tills
                return selectedCrop != null ? ColourValid : ColourInvalid;

            case FarmTool.Harvest:
                if (tile.IsReadyToHarvest) return ColourHarvest;
                if (tile.IsPlanted)        return ColourGrowing;
                return ColourInvalid;

            case FarmTool.Remove:
                return tile.IsPlanted ? ColourRemove : ColourInvalid;

            default:
                return ColourInvalid;
        }
    }

    private void UpdateContextHint(FarmTile tile)
    {
        string hint = activeTool switch
        {
            FarmTool.Hoe     => !tile.IsTilled
                                    ? "Click / Drag to Till"
                                    : "Already tilled",
            FarmTool.Seed    => tile.IsPlanted
                                    ? $"Growing: {FormatGrowTime(tile.GetRemainingSeconds())}"
                                    : selectedCrop != null
                                        ? $"Click / Drag to Plant {selectedCrop.CropName}"
                                        : "No crop selected - pick one from the toolbar",
            FarmTool.Harvest => tile.IsReadyToHarvest
                                    ? $"Click / Drag to Harvest {tile.PlantedCrop?.CropName}"
                                    : tile.IsPlanted
                                        ? $"Growing: {FormatGrowTime(tile.GetRemainingSeconds())}"
                                        : "Nothing to harvest here",
            FarmTool.Remove  => tile.IsPlanted
                                    ? "Click / Drag to Remove crop"
                                    : "Nothing to remove here",
            FarmTool.Build   => "Open build menu with G",
            _                => ""
        };

        HUDManager.Instance?.SetContextHint(hint);
    }

    // ── HUD tool text ─────────────────────────────────────────────────────────

    private string lastToolText;
    private void UpdateToolText()
    {
        string desired = activeTool switch
        {
            FarmTool.Hoe     => "Hoe",
            FarmTool.Seed    => selectedCrop != null ? $"Seed: {selectedCrop.CropName}" : "Seed (no crop)",
            FarmTool.Harvest => "Harvest",
            FarmTool.Remove  => "Remove",
            FarmTool.Build   => "Build",
            _                => "Farming Mode"
        };

        if (desired == lastToolText) return;
        lastToolText = desired;
        HUDManager.Instance?.UpdateToolIndicator(desired);
    }

    // ── Sell all ──────────────────────────────────────────────────────────────

    private void SellAll()
    {
        AudioManager.Instance?.PlaySell();
        int earned = GameManager.Instance.Inventory.SellAll();
        if (earned > 0)
        {
            // lifetime earnings already tracked inside AddCoins()
            GameManager.Instance.RealTime?.ResetAutosaveTimer();
        }
        HUDManager.Instance?.ShowNotification(earned > 0
            ? $"Sold everything for {earned} coins!"
            : "Nothing to sell!");
    }

    // ── Raycasting ────────────────────────────────────────────────────────────

    private Vector2Int? GetHoveredTileCoord()
    {
        if (mainCamera == null) return null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, GetGroundMask())) return null;

        if (debugRaycast)
            Debug.Log($"[Hover] Hit '{hit.collider.name}' at {hit.point}");

        if (Vector3.Distance(transform.position, hit.point) > hoverRange) return null;

        Vector2Int coord = grid.WorldToGrid(hit.point);
        if (!grid.IsValidCoord(coord)) return null;

        Vector3 centre = grid.GridToWorld(coord);
        float half = grid.TileSize * 0.5f;
        if (Mathf.Abs(hit.point.x - centre.x) > half) return null;
        if (Mathf.Abs(hit.point.z - centre.z) > half) return null;

        return grid.GetTile(coord) != null ? coord : null;
    }

    private LayerMask GetGroundMask()
    {
        if (groundLayer.value != 0) return groundLayer;
        if (!hasWarnedAboutMissingLayer)
        {
            hasWarnedAboutMissingLayer = true;
            int idx = LayerMask.NameToLayer(FARM_INTERACT_LAYER);
            if (idx >= 0)
            {
                groundLayer = 1 << idx;
                Debug.LogWarning($"[PlayerInteraction] groundLayer not set — auto-using '{FARM_INTERACT_LAYER}'. Assign in Inspector.", this);
            }
            else
            {
                groundLayer = ~0;
                Debug.LogError($"[PlayerInteraction] '{FARM_INTERACT_LAYER}' layer missing! Create it in Project Settings.", this);
            }
        }
        return groundLayer;
    }

    // ── Hover highlight creation ──────────────────────────────────────────────

    private void CreateHoverHighlight()
    {
        hoverRoot = new GameObject("TileHoverHighlight");
        hoverRoot.hideFlags = HideFlags.DontSave; // don't serialize into scene

        hoverMaterial = CreateTransparentMaterial();
        hoverMaterial.SetColor(colorPropertyId, ColourValid);

        float tile  = grid != null ? grid.TileSize : 1f;
        float thick = tile * 0.06f;
        float half  = tile * 0.5f;
        float inner = half - thick * 0.5f; // side edges inset to avoid corner overlap

        // Top & bottom edges span full tile width
        CreateEdge(new Vector3( 0,     0,  half),  new Vector3(tile, thick, 1f));  // top
        CreateEdge(new Vector3( 0,     0, -half),  new Vector3(tile, thick, 1f));  // bottom
        // Side edges shortened to fit between top & bottom (no overlap at corners)
        CreateEdge(new Vector3(-half,  0,  0),     new Vector3(thick, tile - thick * 2f, 1f)); // left
        CreateEdge(new Vector3( half,  0,  0),     new Vector3(thick, tile - thick * 2f, 1f)); // right

        hoverRoot.SetActive(false);
    }

    private static readonly int colorPropertyId = Shader.PropertyToID("_BaseColor");
    private static readonly int colorPropertyIdFallback = Shader.PropertyToID("_Color");

    /// <summary>
    /// Creates a transparent unlit material compatible with Unity 6 URP.
    /// Falls back gracefully to built-in shaders if URP is unavailable.
    /// </summary>
    private Material CreateTransparentMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        bool isURP = shader != null;

        if (!isURP)
        {
            shader = Shader.Find("Unlit/Transparent Colored")
                  ?? Shader.Find("Unlit/Color")
                  ?? Shader.Find("Standard");
        }

        var mat = new Material(shader);

        if (isURP)
        {
            // URP Unlit transparency setup for Unity 6
            mat.SetFloat("_Surface", 1f);  // 0 = opaque, 1 = transparent
            mat.SetFloat("_Blend", 0f);    // 0 = alpha, 1 = premultiply, 2 = additive, 3 = multiply
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.SetFloat("_Cull", 0f); // render both sides

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            // Built-in shader fallback
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        return mat;
    }

    private void CreateEdge(Vector3 localPos, Vector3 localScale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "HoverEdge";
        go.transform.SetParent(hoverRoot.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale    = localScale;
        go.layer = 2; // Ignore Raycast
        go.hideFlags = HideFlags.DontSave;
        Destroy(go.GetComponent<Collider>());
        var r = go.GetComponent<Renderer>();
        r.material          = hoverMaterial;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows    = false;
    }

    private void OnDestroy()
    {
        if (hoverRoot != null) Destroy(hoverRoot);
        if (hoverMaterial != null) Destroy(hoverMaterial);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsInBuildMode =>
        BuildingManager.Instance != null && BuildingManager.Instance.IsInBuildMode;

    private static string FormatGrowTime(float seconds)
    {
        if (seconds <= 0f) return "Ready!";
        int m = (int)seconds / 60;
        int s = (int)seconds % 60;
        return m > 0 ? $"{m}m {s:D2}s" : $"{s}s";
    }
}
