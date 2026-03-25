using UnityEngine;

/// <summary>
/// Walk-to-target only.
/// Owns: target, stop distance, arrival callback, cancel.
/// Called by PlayerController each frame via Tick().
/// No input. No physics. No animation.
/// </summary>
public class PlayerAutoMoveAgent : MonoBehaviour
{
    public bool IsActive { get; private set; }

    private Vector3       _target;
    private float         _stopDistance;
    private System.Action _onArrived;

    /// <summary>Begin walking to a world position. Replaces any current move.</summary>
    public void MoveTo(Vector3 target, float stopDistance, System.Action onArrived)
    {
        _target       = target;
        _stopDistance = stopDistance;
        _onArrived    = onArrived;
        IsActive      = true;
    }

    /// <summary>Cancel current auto-move without firing the arrival callback.</summary>
    public void Cancel()
    {
        IsActive   = false;
        _onArrived = null;
    }

    /// <summary>
    /// Called by PlayerController each frame.
    /// Returns the normalised direction to move this frame.
    /// Returns Vector3.zero when arrived — fires callback and deactivates.
    /// </summary>
    public Vector3 Tick(Vector3 currentPosition)
    {
        if (!IsActive) return Vector3.zero;

        Vector3 toTarget = _target - currentPosition;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        if (dist <= _stopDistance)
        {
            IsActive = false;
            var cb = _onArrived;
            _onArrived = null;
            cb?.Invoke();
            return Vector3.zero;
        }

        return toTarget.normalized;
    }
}
