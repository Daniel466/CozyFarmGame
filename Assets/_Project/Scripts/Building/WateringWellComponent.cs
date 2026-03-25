using UnityEngine;

/// <summary>
/// Watering Well — deferred to post-Phase 1.
/// Watering mechanic removed from Phase 1 scope (real-time growth requires no watering).
/// This component is a no-op placeholder so BuildingManager can still attach it without errors.
/// </summary>
public class WateringWellComponent : MonoBehaviour
{
    public void Initialise(Vector2Int coord, int waterRadius, float waterInterval)
    {
        // No-op — watering deferred to post-Phase 1
    }
}
