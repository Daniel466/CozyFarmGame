using UnityEngine;

/// <summary>
/// Animation only — no movement policy, no input, no farming.
/// Owns: animator reference, speed parameter, action triggers.
/// </summary>
public class PlayerAnimationDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash   = Animator.StringToHash("Speed");
    private static readonly int PlantHash   = Animator.StringToHash("Plant");
    private static readonly int WaterHash   = Animator.StringToHash("Water");
    private static readonly int HarvestHash = Animator.StringToHash("Harvest");

    public void SetSpeed(float speed)      => animator?.SetFloat(SpeedHash, speed);
    public void TriggerPlant()             => animator?.SetTrigger(PlantHash);
    public void TriggerWater()             => animator?.SetTrigger(WaterHash);
    public void TriggerHarvest()           => animator?.SetTrigger(HarvestHash);
}
