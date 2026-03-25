using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the 20x20 farm grid. Tiles are 1x1 Unity units, centred on the GameObject.
/// A flat BoxCollider on the FarmInteract layer receives all player raycasts.
/// </summary>
public class FarmGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth  = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float tileSize = 1f;

    private Dictionary<Vector2Int, FarmTile> tiles = new Dictionary<Vector2Int, FarmTile>();

    private const string FARM_INTERACT_LAYER = "FarmInteract";

    public int   Width    => gridWidth;
    public int   Height   => gridHeight;
    public float TileSize => tileSize;

    private void Start()
    {
        GenerateGrid();
        CreateGroundCollider();
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector2Int coord    = new Vector2Int(x, z);
                Vector3    worldPos = GridToWorld(coord);
                tiles[coord] = new FarmTile(coord, worldPos);
            }
        }
    }

    /// <summary>
    /// Spawns an invisible flat BoxCollider covering the entire grid so raycasts always register.
    /// Placed on the FarmInteract layer — PlayerInteraction.groundLayer must include this layer.
    /// </summary>
    private void CreateGroundCollider()
    {
        var go = new GameObject("FarmGridCollider");
        go.transform.SetParent(transform, false);
        go.transform.position = transform.position;

        int layerIndex = LayerMask.NameToLayer(FARM_INTERACT_LAYER);
        if (layerIndex < 0)
        {
            Debug.LogError($"[FarmGrid] Layer '{FARM_INTERACT_LAYER}' not found. " +
                           "Add it in Project Settings > Tags and Layers.");
            layerIndex = 0;
        }
        go.layer = layerIndex;

        var col    = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(gridWidth * tileSize, 0.02f, gridHeight * tileSize);
        col.center = Vector3.zero; // grid is centred on transform.position
    }

    // Grid is centred on transform.position.
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

    public bool IsValidCoord(Vector2Int coord) =>
        coord.x >= 0 && coord.x < gridWidth &&
        coord.y >= 0 && coord.y < gridHeight;

    public FarmTile GetTile(Vector2Int coord)
    {
        tiles.TryGetValue(coord, out FarmTile tile);
        return tile;
    }

    public FarmTile GetTileAtWorldPos(Vector3 worldPos) =>
        GetTile(WorldToGrid(worldPos));

    public Dictionary<Vector2Int, FarmTile> GetAllTiles() => tiles;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 gridStart = GridToWorld(Vector2Int.zero);
        Vector3 origin    = gridStart - new Vector3(tileSize * 0.5f, 0, tileSize * 0.5f);
        float   w         = gridWidth  * tileSize;
        float   h         = gridHeight * tileSize;

        // Outer border
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawLine(origin,                          origin + new Vector3(w, 0, 0));
        UnityEditor.Handles.DrawLine(origin + new Vector3(w, 0, 0),  origin + new Vector3(w, 0, h));
        UnityEditor.Handles.DrawLine(origin + new Vector3(w, 0, h),  origin + new Vector3(0, 0, h));
        UnityEditor.Handles.DrawLine(origin + new Vector3(0, 0, h),  origin);

        // Tile lines
        UnityEditor.Handles.color = new Color(1, 1, 1, 0.2f);
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 s = origin + new Vector3(x * tileSize, 0, 0);
            UnityEditor.Handles.DrawLine(s, s + new Vector3(0, 0, h));
        }
        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 s = origin + new Vector3(0, 0, z * tileSize);
            UnityEditor.Handles.DrawLine(s, s + new Vector3(w, 0, 0));
        }

        // Origin marker
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawLine(transform.position - Vector3.right   * 0.5f,
                                     transform.position + Vector3.right   * 0.5f);
        UnityEditor.Handles.DrawLine(transform.position - Vector3.forward * 0.5f,
                                     transform.position + Vector3.forward * 0.5f);
    }
#endif
}
