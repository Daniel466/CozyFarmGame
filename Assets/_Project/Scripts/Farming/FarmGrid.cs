using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the farm grid of tiles. Each tile can be tilled, planted, watered and harvested.
/// </summary>
public class FarmGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth  = 10;   // Phase 1: one 10x10 plot (100 tiles)
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float tileSize = 1.5f; // 1.5 matches Poly Universal Pack crop model scale

    private Dictionary<Vector2Int, FarmTile> tiles = new Dictionary<Vector2Int, FarmTile>();

    private void Start()
    {
        GenerateGrid();
        CreateGroundCollider();
    }

    private const string FARM_INTERACT_LAYER = "FarmInteract";

    /// <summary>
    /// Spawns an invisible flat box collider covering the entire grid so raycasts always register.
    /// Sits on the "FarmInteract" layer — PlayerInteraction.groundLayer must include this layer.
    /// </summary>
    private void CreateGroundCollider()
    {
        var go = new GameObject("FarmGridCollider");
        go.transform.SetParent(transform, false);
        go.transform.position = transform.position;

        int layerIndex = LayerMask.NameToLayer(FARM_INTERACT_LAYER);
        if (layerIndex < 0)
        {
            Debug.LogError($"[FarmGrid] Layer '{FARM_INTERACT_LAYER}' not found! " +
                           "Add it in Project Settings > Tags and Layers. Falling back to Default.");
            layerIndex = 0;
        }
        go.layer = layerIndex;

        var col = go.AddComponent<BoxCollider>();
        float w = gridWidth  * tileSize;
        float h = gridHeight * tileSize;
        col.size   = new Vector3(w, 0.02f, h);
        col.center = Vector3.zero; // grid is centred on transform
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector2Int coord = new Vector2Int(x, z);
                Vector3 worldPos = GridToWorld(coord);

                FarmTile tile = new FarmTile(coord, worldPos);
                tiles[coord] = tile;
            }
        }
    }

    public FarmTile GetTile(Vector2Int coord)
    {
        tiles.TryGetValue(coord, out FarmTile tile);
        return tile;
    }

    public FarmTile GetTileAtWorldPos(Vector3 worldPos)
    {
        Vector2Int coord = WorldToGrid(worldPos);
        return GetTile(coord);
    }

    // Grid is centred on transform.position so you can position it by placing the GameObject at the pad centre.
    private Vector3 GridOffset => new Vector3(
        (gridWidth  - 1) * tileSize * 0.5f,
        0f,
        (gridHeight - 1) * tileSize * 0.5f);

    public Vector3 GridToWorld(Vector2Int coord)
    {
        Vector3 centre = transform.position;
        return new Vector3(
            centre.x + coord.x * tileSize - GridOffset.x,
            centre.y,
            centre.z + coord.y * tileSize - GridOffset.z);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 centre = transform.position;
        int x = Mathf.RoundToInt((worldPos.x - centre.x + GridOffset.x) / tileSize);
        int z = Mathf.RoundToInt((worldPos.z - centre.z + GridOffset.z) / tileSize);
        return new Vector2Int(x, z);
    }

    public bool IsValidCoord(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < gridWidth && coord.y >= 0 && coord.y < gridHeight;
    }

    public Dictionary<Vector2Int, FarmTile> GetAllTiles() => tiles;

    public float TileSize => tileSize;

#if UNITY_EDITOR
    /// <summary>
    /// Draws the grid in the Scene view so you can visually align it with flower beds.
    /// Yellow = grid outline, White = individual tile borders.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw outer grid boundary in yellow
        UnityEditor.Handles.color = Color.yellow;
        Vector3 gridStart = GridToWorld(Vector2Int.zero);
        Vector3 origin = gridStart - new Vector3(tileSize * 0.5f, 0, tileSize * 0.5f);
        float w = gridWidth * tileSize;
        float h = gridHeight * tileSize;

        // Outer border
        UnityEditor.Handles.DrawLine(origin, origin + new Vector3(w, 0, 0));
        UnityEditor.Handles.DrawLine(origin + new Vector3(w, 0, 0), origin + new Vector3(w, 0, h));
        UnityEditor.Handles.DrawLine(origin + new Vector3(w, 0, h), origin + new Vector3(0, 0, h));
        UnityEditor.Handles.DrawLine(origin + new Vector3(0, 0, h), origin);

        // Individual tile lines
        UnityEditor.Handles.color = new Color(1, 1, 1, 0.3f);
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = origin + new Vector3(x * tileSize, 0, 0);
            Vector3 end = start + new Vector3(0, 0, h);
            UnityEditor.Handles.DrawLine(start, end);
        }
        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 start = origin + new Vector3(0, 0, z * tileSize);
            Vector3 end = start + new Vector3(w, 0, 0);
            UnityEditor.Handles.DrawLine(start, end);
        }

        // Draw centre cross on tile (0,0) — this is where the FarmGrid GameObject sits
        UnityEditor.Handles.color = Color.red;
        Vector3 tile00 = transform.position;
        UnityEditor.Handles.DrawLine(tile00 - Vector3.right * 0.5f, tile00 + Vector3.right * 0.5f);
        UnityEditor.Handles.DrawLine(tile00 - Vector3.forward * 0.5f, tile00 + Vector3.forward * 0.5f);
    }
#endif
}
