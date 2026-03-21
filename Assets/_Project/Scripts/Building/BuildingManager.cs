using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all placed buildings on the farm.
/// Handles placement validation, rotation, and removal.
/// </summary>
public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;

    // Track all placed buildings: grid position -> placed building info
    private Dictionary<Vector2Int, PlacedBuilding> placedBuildings = new Dictionary<Vector2Int, PlacedBuilding>();

    // Ghost preview object
    private GameObject ghostObject;
    private BuildingData selectedBuilding;
    private int currentRotation = 0; // 0, 90, 180, 270

    public bool IsInBuildMode { get; private set; }

    private Camera mainCamera;
    private FarmGrid grid;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        grid = GameManager.Instance?.FarmGrid;
    }

    private void Update()
    {
        if (!IsInBuildMode) return;

        UpdateGhostPosition();

        if (Input.GetKeyDown(KeyCode.R))
            RotateGhost();

        if (Input.GetMouseButtonDown(0))
            TryPlace();

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            ExitBuildMode();
    }

    // --- Build Mode ---

    public void EnterBuildMode(BuildingData building)
    {
        IsInBuildMode = true;
        selectedBuilding = building;
        currentRotation = 0;
        CreateGhost();
        HUDManager.Instance?.ShowNotification($"Placing {building.BuildingName} | R: Rotate | RClick/Esc: Cancel");
        HUDManager.Instance?.UpdateToolIndicator($"🏗️ {building.BuildingName}");
    }

    public void ExitBuildMode()
    {
        IsInBuildMode = false;
        selectedBuilding = null;
        DestroyGhost();
        HUDManager.Instance?.UpdateToolIndicator("🌱 Farming Mode");
        HUDManager.Instance?.ShowNotification("Exited build mode");
    }

    // --- Ghost Preview ---

    private void CreateGhost()
    {
        DestroyGhost();
        if (selectedBuilding == null) return;

        if (selectedBuilding.Prefab != null)
        {
            ghostObject = Instantiate(selectedBuilding.Prefab);
        }
        else
        {
            ghostObject = CreatePlaceholderBuilding(selectedBuilding, Vector3.zero);
        }

        // Make ghost semi-transparent
        SetGhostTransparency(ghostObject, 0.5f);
        ghostObject.name = "GhostPreview";

        // Disable all colliders on ghost
        foreach (var col in ghostObject.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Set to Ignore Raycast layer
        SetLayerRecursively(ghostObject, LayerMask.NameToLayer("Ignore Raycast"));
    }

    private void UpdateGhostPosition()
    {
        if (ghostObject == null || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = groundLayer.value == 0 ? ~0 : groundLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
        {
            Vector2Int coord = grid.WorldToGrid(hit.point);
            Vector3 snappedPos = grid.GridToWorld(coord);
            ghostObject.transform.position = snappedPos + Vector3.up * 0.01f;
            ghostObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

            // Colour ghost based on whether placement is valid
            bool canPlace = CanPlace(coord, selectedBuilding);
            SetGhostColor(ghostObject, canPlace
                ? new Color(0.3f, 1f, 0.3f, 0.5f)   // Green = valid
                : new Color(1f, 0.2f, 0.2f, 0.5f));  // Red = invalid
        }
    }

    private void DestroyGhost()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
        }
    }

    private void RotateGhost()
    {
        currentRotation = (currentRotation + 90) % 360;
        if (ghostObject != null)
            ghostObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    // --- Placement ---

    private bool TryPlace()
    {
        if (selectedBuilding == null || mainCamera == null) return false;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = groundLayer.value == 0 ? ~0 : groundLayer;

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, mask)) return false;

        Vector2Int coord = grid.WorldToGrid(hit.point);

        if (!CanPlace(coord, selectedBuilding))
        {
            HUDManager.Instance?.ShowNotification("Can't place here!");
            return false;
        }

        // Check cost
        if (!GameManager.Instance.Economy.SpendCoins(selectedBuilding.Cost))
        {
            HUDManager.Instance?.ShowNotification($"Not enough coins! Need {selectedBuilding.Cost} 🪙");
            return false;
        }

        // Place it!
        Vector3 worldPos = grid.GridToWorld(coord) + Vector3.up * 0.01f;
        GameObject placed;

        if (selectedBuilding.Prefab != null)
            placed = Instantiate(selectedBuilding.Prefab, worldPos, Quaternion.Euler(0, currentRotation, 0));
        else
            placed = CreatePlaceholderBuilding(selectedBuilding, worldPos);

        placed.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
        placed.name = $"Building_{selectedBuilding.BuildingId}_{coord}";

        // Mark grid cells as occupied
        OccupyCells(coord, selectedBuilding);

        placedBuildings[coord] = new PlacedBuilding
        {
            data = selectedBuilding,
            coord = coord,
            gameObject = placed,
            rotation = currentRotation
        };

        // Award XP
        GameManager.Instance.Progression.AddXP(selectedBuilding.PlaceXP);

        HUDManager.Instance?.ShowNotification($"{selectedBuilding.BuildingName} placed! +{selectedBuilding.PlaceXP} XP");
        return true;
    }

    public bool RemoveBuilding(Vector2Int coord)
    {
        if (!placedBuildings.TryGetValue(coord, out PlacedBuilding building)) return false;

        Destroy(building.gameObject);
        FreeCells(coord, building.data);
        placedBuildings.Remove(coord);

        // Refund 50% of cost
        int refund = building.data.Cost / 2;
        GameManager.Instance.Economy.AddCoins(refund);
        HUDManager.Instance?.ShowNotification($"{building.data.BuildingName} removed. Refunded {refund} 🪙");
        return true;
    }

    // --- Validation ---

    private bool CanPlace(Vector2Int coord, BuildingData building)
    {
        if (!grid.IsValidCoord(coord)) return false;

        for (int x = 0; x < building.Size.x; x++)
        {
            for (int y = 0; y < building.Size.y; y++)
            {
                Vector2Int cell = coord + new Vector2Int(x, y);
                if (!grid.IsValidCoord(cell)) return false;
                if (placedBuildings.ContainsKey(cell)) return false;
            }
        }
        return true;
    }

    private void OccupyCells(Vector2Int origin, BuildingData building)
    {
        for (int x = 0; x < building.Size.x; x++)
            for (int y = 0; y < building.Size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (cell != origin)
                    placedBuildings[cell] = new PlacedBuilding { data = building, coord = origin };
            }
    }

    private void FreeCells(Vector2Int origin, BuildingData building)
    {
        for (int x = 0; x < building.Size.x; x++)
            for (int y = 0; y < building.Size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (placedBuildings.ContainsKey(cell) && placedBuildings[cell].coord == origin)
                    placedBuildings.Remove(cell);
            }
    }

    // --- Placeholder Visuals ---

    public static GameObject CreatePlaceholderBuilding(BuildingData data, Vector3 position)
    {
        GameObject go = new GameObject($"Building_{data.BuildingId}");
        go.transform.position = position;

        // Create a simple box shape
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(go.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        body.transform.localScale = new Vector3(
            data.Size.x * 0.9f,
            data.Type == BuildingType.Decoration ? 0.5f : 1.5f,
            data.Size.y * 0.9f);

        // Roof (for functional buildings)
        if (data.Type == BuildingType.Functional)
        {
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(go.transform, false);
            roof.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            roof.transform.localScale = new Vector3(data.Size.x * 1.05f, 0.3f, data.Size.y * 1.05f);
            SetPrimitiveColor(roof, data.PlaceholderColor * 0.7f);
            Destroy(roof.GetComponent<Collider>());
        }

        SetPrimitiveColor(body, data.PlaceholderColor);
        Destroy(body.GetComponent<Collider>());

        return go;
    }

    private static void SetPrimitiveColor(GameObject obj, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        obj.GetComponent<Renderer>().material = mat;
    }

    private void SetGhostTransparency(GameObject go, float alpha)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Color c = r.material.color;
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
            mat.color = c;
            // Enable transparency
            mat.SetFloat("_Surface", 1f); // 0=Opaque, 1=Transparent
            mat.SetFloat("_Blend", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            r.material = mat;
        }
    }

    private void SetGhostColor(GameObject go, Color color)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            r.material.SetColor("_BaseColor", color);
            r.material.color = color;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer == -1) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public bool IsCellOccupied(Vector2Int coord) => placedBuildings.ContainsKey(coord);
    public Dictionary<Vector2Int, PlacedBuilding> GetAllBuildings() => placedBuildings;
}

[System.Serializable]
public class PlacedBuilding
{
    public BuildingData data;
    public Vector2Int coord;
    public GameObject gameObject;
    public int rotation;
}
