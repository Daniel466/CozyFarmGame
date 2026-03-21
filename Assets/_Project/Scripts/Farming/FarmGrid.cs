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
        return new Vector3(coord.x * tileSize, 0f, coord.y * tileSize);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int z = Mathf.RoundToInt(worldPos.z / tileSize);
        return new Vector2Int(x, z);
    }

    public bool IsValidCoord(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < gridWidth && coord.y >= 0 && coord.y < gridHeight;
    }

    public Dictionary<Vector2Int, FarmTile> GetAllTiles() => tiles;
}
