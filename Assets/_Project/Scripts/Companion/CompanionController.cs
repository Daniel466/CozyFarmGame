using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Companion NPC — idles near the farmhouse, periodically walks to the
/// SellBox and sells all inventory, then returns to idle position.
///
/// Uses NavMeshAgent for movement (simpler than CharacterController for NPCs).
/// Subscribes to RealTimeManager.OnTick for sell interval timing.
/// </summary>
public class CompanionController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sellIntervalSeconds = 60f;
    [SerializeField] private float sellRadius          = 2f;

    [Header("References")]
    [SerializeField] private SellBoxComponent sellBox;
    [SerializeField] private Transform        idlePosition;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private NavMeshAgent _agent;
    private float        _ticksUntilSell;
    private State        _state = State.Idle;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private enum State { Idle, WalkingToSell, Selling, Returning }

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null) _agent = gameObject.AddComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        _ticksUntilSell = sellIntervalSeconds;
    }

    private void Start()
    {
        if (RealTimeManager.Instance != null)
            RealTimeManager.Instance.OnTick += OnTick;

        if (idlePosition == null)
        {
            // Default idle = spawn position
            var idleGO = new GameObject("CompanionIdleSpot");
            idleGO.transform.position = transform.position;
            idlePosition = idleGO.transform;
        }

        // Auto-find SellBox if not assigned
        if (sellBox == null)
            sellBox = Object.FindFirstObjectByType<SellBoxComponent>();
    }

    private void OnDestroy()
    {
        if (RealTimeManager.Instance != null)
            RealTimeManager.Instance.OnTick -= OnTick;
    }

    // ── Tick ──────────────────────────────────────────────────────────────────

    private void OnTick()
    {
        if (_state != State.Idle) return;

        _ticksUntilSell--;
        if (_ticksUntilSell <= 0)
        {
            _ticksUntilSell = sellIntervalSeconds;
            TryStartSellRun();
        }
    }

    // ── State machine ─────────────────────────────────────────────────────────

    private void Update()
    {
        UpdateAnimation();

        switch (_state)
        {
            case State.WalkingToSell:
                if (HasArrived())
                {
                    _state = State.Selling;
                    DoSell();
                }
                break;

            case State.Returning:
                if (HasArrived())
                    _state = State.Idle;
                break;
        }
    }

    private void TryStartSellRun()
    {
        if (GameManager.Instance?.Inventory == null) return;
        if (GameManager.Instance.Inventory.UsedSlots == 0) return; // nothing to sell
        if (sellBox == null) return;

        _state = State.WalkingToSell;
        _agent.SetDestination(sellBox.transform.position);
    }

    private void DoSell()
    {
        int earned = GameManager.Instance.Inventory.SellAll();
        if (earned > 0)
        {
            GameManager.Instance.RealTime?.ResetAutosaveTimer();
            AudioManager.Instance?.PlaySell();
            HUDManager.Instance?.ShowNotification($"Companion sold crops for {earned} coins!");
            Debug.Log($"[Companion] Sold for {earned} coins.");
        }

        // Return home
        _state = State.Returning;
        _agent.SetDestination(idlePosition.position);
    }

    private bool HasArrived()
    {
        if (_agent.pathPending) return false;
        return _agent.remainingDistance <= sellRadius;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;
        float speed = _agent.velocity.magnitude;
        animator.SetFloat(SpeedHash, speed > 0.1f ? 1f : 0f);
    }
}
