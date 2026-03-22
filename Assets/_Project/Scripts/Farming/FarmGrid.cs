using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the farm grid of tiles. Each tile can be tilled, planted, watered and harvested.
/// </summary>
public class FarmGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float tileSize = 1f;

    [Header("Grid Origin")]
    [SerializeField] private Vector3 gridOrigin = Vector3.zero; // Set to match flower bed positions

    [Header("Tile Visuals")]
    [SerializeField] private GameObject normalTilePrefab;
    [SerializeField] private GameObject tilledTilePrefab;

    private Dictionary<Vector2Int, FarmTile> tiles = new Dictionary<Vector2Int, FarmTile>();

    private void Start()
    {
        GenerateGrid();
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

                if (normalTilePrefab != null)
                    Instantiate(normalTilePrefab, worldPos, Quaternion.identity, transform);
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

    public Vector3 GridToWorld(Vector2Int coord)
    {
        return new Vector3(
            gridOrigin.x + coord.x * tileSize,
            gridOrigin.y,
            gridOrigin.z + coord.y * tileSize);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - gridOrigin.x) / tileSize);
        int z = Mathf.RoundToInt((worldPos.z - gridOrigin.z) / tileSize);
        return new Vector2Int(x, z);
    }

    public bool IsValidCoord(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < gridWidth && coord.y >= 0 && coord.y < gridHeight;
    }

    public Dictionary<Vector2Int, FarmTile> GetAllTiles() => tiles;

#if UNITY_EDITOR
    /// <summary>
    /// Draws the grid in the Scene view so you can visually align it with flower beds.
    /// Yellow = grid outline, White = individual tile borders.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw outer grid boundary in yellow
        UnityEditor.Handles.color = Color.yellow;
        Vector3 origin = gridOrigin;
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

        // Draw centre cross on tile (0,0)
        UnityEditor.Handles.color = Color.red;
        Vector3 tile00 = GridToWorld(Vector2Int.zero);
        UnityEditor.Handles.DrawLine(tile00 - Vector3.right * 0.5f, tile00 + Vector3.right * 0.5f);
        UnityEditor.Handles.DrawLine(tile00 - Vector3.forward * 0.5f, tile00 + Vector3.forward * 0.5f);
    }
#endif
}
