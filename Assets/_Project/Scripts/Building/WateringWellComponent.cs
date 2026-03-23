using System.Collections;
using UnityEngine;

/// <summary>
/// Attached to a placed Watering Well building.
/// Auto-waters all unwatered planted tiles within radius every interval seconds.
/// Tile markers and growth bonuses apply; audio/particles/XP are handled here (not per-tile).
/// </summary>
public class WateringWellComponent : MonoBehaviour
{
    private Vector2Int origin;
    private int radius;
    private float interval;

    public void Initialise(Vector2Int coord, int waterRadius, float waterInterval)
    {
        origin   = coord;
        radius   = waterRadius;
        interval = waterInterval;
        StartCoroutine(WaterRoutine());
    }

    private IEnumerator WaterRoutine()
    {
        // Brief startup delay so FarmingManager is ready on scene load
        yield return new WaitForSeconds(2f);

        while (true)
        {
            WaterNearbyTiles();
            yield return new WaitForSeconds(interval);
        }
    }

    private void WaterNearbyTiles()
    {
        if (FarmingManager.Instance == null) return;

        int count = 0;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int coord = origin + new Vector2Int(x, y);
                if (FarmingManager.Instance.WaterTile(coord, playEffects: false))
                    count++;
            }
        }

        if (count > 0)
        {
            AudioManager.Instance?.PlayWater();
            HUDManager.Instance?.ShowNotification(
                count == 1 ? "Well watered 1 crop." : $"Well watered {count} crops.");
        }
    }
}
