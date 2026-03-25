using UnityEngine;

/// <summary>
/// Movement lock only — tracks whether an action animation is in progress.
/// Has optional timeout so a missed EndAction() call can't lock the player forever.
/// No input. No physics. No animation.
/// </summary>
public class PlayerActionLock : MonoBehaviour
{
    private const float DefaultTimeout = 3f;

    public bool IsLocked { get; private set; }

    private float _timeoutRemaining;

    /// <summary>Lock movement. timeout = 0 means use the default safety timeout.</summary>
    public void Begin(float timeout = 0f)
    {
        IsLocked         = true;
        _timeoutRemaining = timeout > 0f ? timeout : DefaultTimeout;
    }

    /// <summary>Unlock movement immediately.</summary>
    public void End()
    {
        IsLocked         = false;
        _timeoutRemaining = 0f;
    }

    private void Update()
    {
        if (!IsLocked) return;

        _timeoutRemaining -= Time.deltaTime;
        if (_timeoutRemaining <= 0f)
        {
            Debug.LogWarning("[PlayerActionLock] Action lock timed out — forcing unlock. Call EndAction() from your animation event.");
            End();
        }
    }
}
