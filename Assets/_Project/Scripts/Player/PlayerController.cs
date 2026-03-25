using UnityEngine;

/// <summary>
/// Thin coordinator — decides intent each frame and delegates to focused modules.
///
/// Decision order:
///   1. Action locked  → gravity only, no movement
///   2. Manual input   → cancel auto-move, move via motor
///   3. Auto-moving    → let agent drive direction, move via motor
///   4. Idle           → gravity only
///
/// Public API is identical to the old monolithic PlayerController so
/// PlayerInteraction and other callers need no changes.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ── Module refs ───────────────────────────────────────────────────────────

    private PlayerMotor           _motor;
    private PlayerAnimationDriver _anim;
    private PlayerInputReader     _input;
    private PlayerAutoMoveAgent   _autoMove;
    private PlayerActionLock      _actionLock;

    private Camera _mainCam;

    // ── Public API (matches old PlayerController exactly) ─────────────────────

    public bool IsAutoMoving       => _autoMove  != null && _autoMove.IsActive;
    public bool IsPerformingAction => _actionLock != null && _actionLock.IsLocked;

    public void WalkTo(Vector3 target, float stopDistance, System.Action onArrived)
        => _autoMove?.MoveTo(target, stopDistance, onArrived);

    public void CancelAutoMove() => _autoMove?.Cancel();

    public void FacePosition(Vector3 worldPos) => _motor?.FacePosition(worldPos);

    public void BeginAction(float timeout = 0f)
    {
        CancelAutoMove();
        _actionLock?.Begin(timeout);
        _anim?.SetSpeed(0f);
    }

    public void EndAction() => _actionLock?.End();

    public void TriggerPlant()   { BeginAction(); _anim?.TriggerPlant(); }
    public void TriggerWater()   { BeginAction(); _anim?.TriggerWater(); }
    public void TriggerHarvest() { BeginAction(); _anim?.TriggerHarvest(); }

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _motor      = GetComponent<PlayerMotor>();
        _anim       = GetComponent<PlayerAnimationDriver>();
        _input      = GetComponent<PlayerInputReader>();
        _autoMove   = GetComponent<PlayerAutoMoveAgent>();
        _actionLock = GetComponent<PlayerActionLock>();

        if (_motor      == null) Debug.LogError("[PlayerController] PlayerMotor missing!");
        if (_input      == null) Debug.LogError("[PlayerController] PlayerInputReader missing!");
        if (_autoMove   == null) Debug.LogError("[PlayerController] PlayerAutoMoveAgent missing!");
        if (_actionLock == null) Debug.LogError("[PlayerController] PlayerActionLock missing!");
    }

    private void Start()
    {
        _mainCam = Camera.main;
        if (_mainCam == null) Debug.LogWarning("[PlayerController] No Main Camera found.");
    }

    // ── Update — the only decision logic ─────────────────────────────────────

    private void Update()
    {
        if (_motor == null) return;

        // 1. Action locked — no movement, just gravity
        if (_actionLock != null && _actionLock.IsLocked)
        {
            _motor.ApplyGravityOnly();
            return;
        }

        // 2. Manual input cancels auto-move
        bool hasInput = _input != null && _input.HasMoveInput;
        if (hasInput && _autoMove != null && _autoMove.IsActive)
            _autoMove.Cancel();

        // 3. Auto-move
        if (_autoMove != null && _autoMove.IsActive)
        {
            Vector3 dir = _autoMove.Tick(transform.position);
            if (dir.sqrMagnitude > 0.01f)
            {
                _motor.Move(dir);
                _anim?.SetSpeed(1f);
            }
            else
            {
                _motor.ApplyGravityOnly();
                _anim?.SetSpeed(0f);
            }
            return;
        }

        // 4. Manual movement
        if (hasInput)
        {
            Vector3 dir = CameraRelativeDirection(_input.MoveInput);
            _motor.Move(dir);
            _anim?.SetSpeed(dir.sqrMagnitude > 0.01f ? 1f : 0f);
        }
        else
        {
            _motor.ApplyGravityOnly();
            _anim?.SetSpeed(0f);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Vector3 CameraRelativeDirection(Vector2 input)
    {
        if (_mainCam == null) return new Vector3(input.x, 0f, input.y);

        Vector3 forward = _mainCam.transform.forward; forward.y = 0f; forward.Normalize();
        Vector3 right   = _mainCam.transform.right;   right.y   = 0f; right.Normalize();
        return (forward * input.y + right * input.x).normalized;
    }
}
