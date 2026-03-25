using UnityEngine;

/// <summary>
/// Handles player movement and animation.
/// Supports manual WASD movement and auto-move (walk to tile on click).
/// Auto-move is cancelled the moment the player presses WASD.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed     = 5f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float gravity       = -9.81f;

    [Header("Character Model")]
    [SerializeField] private Transform characterModel;
    [SerializeField] private float     modelScale  = 3f;
    [SerializeField] private Vector3   modelOffset = new Vector3(0f, -1f, 0f);

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // Auto-move state
    private bool          isAutoMoving;
    private Vector3       autoMoveTarget;
    private float         autoMoveStopDistance;
    private System.Action onAutoMoveArrived;

    // Action lock — blocks WASD while an action animation plays
    private bool isPerformingAction;

    private CharacterController controller;
    private Vector3 velocity;

    private static readonly int SpeedHash   = Animator.StringToHash("Speed");
    private static readonly int PlantHash   = Animator.StringToHash("Plant");
    private static readonly int WaterHash   = Animator.StringToHash("Water");
    private static readonly int HarvestHash = Animator.StringToHash("Harvest");

    public bool IsAutoMoving      => isAutoMoving;
    public bool IsPerformingAction => isPerformingAction;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null) { Debug.LogError("[PlayerController] CharacterController missing!"); enabled = false; }
    }

    private void Start()
    {
        if (characterModel != null)
        {
            characterModel.localScale    = Vector3.one * modelScale;
            characterModel.localPosition = modelOffset;
        }
    }

    private void Update()
    {
        // Action lock — only apply gravity while an action animation is playing
        if (isPerformingAction)
        {
            ApplyGravity();
            controller.Move(velocity * Time.deltaTime);
            animator?.SetFloat(SpeedHash, 0f);
            return;
        }

        // WASD cancels auto-move so the player always feels in control
        if (isAutoMoving && HasManualInput())
            CancelAutoMove();

        if (isAutoMoving)
            HandleAutoMove();
        else
            HandleMovement();
    }

    // ---- Manual movement ----

    private void HandleMovement()
    {
        if (controller == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 camForward = mainCam.transform.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight   = mainCam.transform.right;   camRight.y   = 0f; camRight.Normalize();
        Vector3 moveDir    = (camForward * v + camRight * h).normalized;
        bool    isMoving   = moveDir.magnitude > 0.1f;

        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        ApplyGravity();
        controller.Move((moveDir * moveSpeed + velocity) * Time.deltaTime);

        animator?.SetFloat(SpeedHash, isMoving ? 1f : 0f);
    }

    // ---- Auto-move ----

    /// <summary>Walks the player to target, calls onArrived when within stopDistance.</summary>
    public void WalkTo(Vector3 target, float stopDistance, System.Action onArrived)
    {
        autoMoveTarget       = target;
        autoMoveStopDistance = stopDistance;
        onAutoMoveArrived    = onArrived;
        isAutoMoving         = true;
    }

    public void CancelAutoMove()
    {
        isAutoMoving      = false;
        onAutoMoveArrived = null;
        // Speed is intentionally not reset here — the update loop manages it:
        // HandleAutoMove sets Speed=1, HandleMovement sets Speed from WASD input.
        // Zeroing here caused a 1-frame idle blip on every re-click.
    }

    private void HandleAutoMove()
    {
        Vector3 toTarget = autoMoveTarget - transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        if (dist <= autoMoveStopDistance)
        {
            isAutoMoving = false;
            ApplyGravity();
            controller.Move(velocity * Time.deltaTime);
            animator?.SetFloat(SpeedHash, 0f);
            var callback = onAutoMoveArrived;
            onAutoMoveArrived = null;
            callback?.Invoke();
            return;
        }

        Vector3 dir = toTarget.normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);

        ApplyGravity();
        controller.Move((dir * moveSpeed + velocity) * Time.deltaTime);
        animator?.SetFloat(SpeedHash, 1f);
    }

    // ---- Helpers ----

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
    }

    private static bool HasManualInput() =>
        Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f;

    /// <summary>Instantly face a world position (ignores Y).</summary>
    public void FacePosition(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    // ---- Animation triggers ----

    /// <summary>Locks movement so WASD is ignored during an action animation.</summary>
    public void BeginAction()
    {
        CancelAutoMove();
        isPerformingAction = true;
        animator?.SetFloat(SpeedHash, 0f);
    }

    /// <summary>Unlocks movement after an action animation completes.</summary>
    public void EndAction()
    {
        isPerformingAction = false;
    }

    public void TriggerPlant()   { BeginAction(); animator?.SetTrigger(PlantHash); }
    public void TriggerWater()   { BeginAction(); animator?.SetTrigger(WaterHash); }
    public void TriggerHarvest() { BeginAction(); animator?.SetTrigger(HarvestHash); }

    // ---- Inspector helpers ----

    public void SetCharacterModel(Transform model, float scale = 3f)
    {
        characterModel = model;
        modelScale     = scale;
        if (model != null)
        {
            model.localScale    = Vector3.one * scale;
            model.localPosition = modelOffset;
        }
    }
}
