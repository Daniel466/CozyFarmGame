using System.Collections;
using UnityEngine;

/// <summary>
/// Animation only — no movement policy, no input, no farming.
/// Owns: animator reference, speed parameter, action triggers.
/// Can wait for an action clip to finish and invoke a callback.
/// </summary>
public class PlayerAnimationDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash   = Animator.StringToHash("Speed");
    private static readonly int PlantHash   = Animator.StringToHash("Plant");
    private static readonly int WaterHash   = Animator.StringToHash("Water");
    private static readonly int HarvestHash = Animator.StringToHash("Harvest");

    private static readonly int PlantStateHash   = Animator.StringToHash("Plant");
    private static readonly int WaterStateHash   = Animator.StringToHash("Water");
    private static readonly int HarvestStateHash = Animator.StringToHash("Harvest");

    private Coroutine _waitCoroutine;

    public void SetSpeed(float speed) => animator?.SetFloat(SpeedHash, speed);

    public void TriggerPlant()   => animator?.SetTrigger(PlantHash);
    public void TriggerWater()   => animator?.SetTrigger(WaterHash);
    public void TriggerHarvest() => animator?.SetTrigger(HarvestHash);

    /// <summary>
    /// Waits until the animator enters the given action state, plays through it,
    /// and exits back to a non-action state. Then invokes onComplete.
    /// </summary>
    public void WaitForActionComplete(System.Action onComplete)
    {
        if (_waitCoroutine != null) StopCoroutine(_waitCoroutine);
        _waitCoroutine = StartCoroutine(WaitForActionExit(onComplete));
    }

    private IEnumerator WaitForActionExit(System.Action onComplete)
    {
        if (animator == null) { onComplete?.Invoke(); yield break; }

        // Wait one frame for the trigger to take effect
        yield return null;

        // Wait until we enter an action state
        float timeout = 3f;
        while (timeout > 0f && !IsInActionState())
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        // Now wait until we leave the action state
        timeout = 5f;
        while (timeout > 0f && IsInActionState())
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        _waitCoroutine = null;
        onComplete?.Invoke();
    }

    /// <summary>Returns true if the animator is currently in a Plant, Water, or Harvest state.</summary>
    private bool IsInActionState()
    {
        if (animator == null) return false;
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.shortNameHash == PlantStateHash
            || state.shortNameHash == WaterStateHash
            || state.shortNameHash == HarvestStateHash;
    }
}
