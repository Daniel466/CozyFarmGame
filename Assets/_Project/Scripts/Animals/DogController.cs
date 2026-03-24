using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Dog companion controller — ShibaInu (Ultimate Animated Animals).
///
/// State machine (three states):
///   Wander — roams within wanderRadius of the doghouse. Picks new random waypoints.
///   Follow — player enters followTriggerRange; dog trots over and stays nearby.
///   Return — player exceeds returnRange; dog walks home and resumes Wander.
///
/// Animator contract (ShibaInu_AC.controller):
///   Float   "Speed"  — 0=Idle, 1=Walk, 2=Gallop
///   Trigger "Eat"    — plays Eating clip then returns to locomotion
///   Trigger "Pet"    — plays Idle_2_HeadLow clip then returns to locomotion
///
/// Happiness, growth bonus, pet/feed interaction, and crop alerts are unchanged.
/// Call SetHome() after spawn (done by DogManager).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class DogController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("State Machine")]
    [Tooltip("Player within this distance switches Wander -> Follow.")]
    [SerializeField] private float followTriggerRange = 12f;
    [Tooltip("Player beyond this distance while Following switches to Return.")]
    [SerializeField] private float returnRange        = 60f;
    [Tooltip("Player within this distance while Returning re-engages Follow. Keep below followTriggerRange to avoid flip-flop.")]
    [SerializeField] private float reFollowRange      = 10f;
    [Tooltip("How far from the doghouse the dog roams while Wandering.")]
    [SerializeField] private float wanderRadius       = 5f;
    [Tooltip("Seconds the dog pauses at each wander waypoint.")]
    [SerializeField] private float wanderIdleTime     = 3f;

    [Header("Follow")]
    [Tooltip("Stop following when within this distance of the player.")]
    [SerializeField] private float followStopDistance = 1.8f;
    [SerializeField] private float walkSpeed          = 2.0f;
    [SerializeField] private float runSpeed           = 5.0f;
    [Tooltip("Distance from player at which dog switches from walk to run.")]
    [SerializeField] private float runThreshold       = 7.0f;

    [Header("Animation")]
    [Tooltip("Animator on the ShibaInu model root. Auto-found in children if left empty.")]
    [SerializeField] private Animator dogAnimator;

    [Header("Interaction")]
    [SerializeField] private float   interactionRange    = 2.5f;
    [SerializeField] private KeyCode interactKey         = KeyCode.E;
    [Tooltip("Seconds before the player can interact again.")]
    [SerializeField] private float   interactionCooldown = 3.5f;
    [Tooltip("Optional world-space prompt shown when player is in range and off cooldown.")]
    [SerializeField] private GameObject interactionPromptRoot;

    [Header("Happiness")]
    [SerializeField] private float petHappinessGain   = 0.30f;
    [SerializeField] private float feedHappinessGain  = 0.50f;
    [Tooltip("Happiness lost per second. Drains fully in ~4 min at default.")]
    [SerializeField] private float happinessDrainRate = 0.004f;

    [Header("Growth Bonus")]
    [Tooltip("Max additive bonus on FarmingManager.EffectiveGrowthMultiplier at full happiness.")]
    [SerializeField] private float maxGrowthBonus = 0.5f;
    [Tooltip("Happiness must exceed this before any bonus applies.")]
    [SerializeField] private float bonusThreshold = 0.30f;

    // -------------------------------------------------------------------------
    // Animator hashes — must match ShibaInu_AC.controller
    // -------------------------------------------------------------------------

    private static readonly int ParamSpeed = Animator.StringToHash("Speed");
    private static readonly int ParamEat   = Animator.StringToHash("Eat");
    private static readonly int ParamPet   = Animator.StringToHash("Pet");

    // -------------------------------------------------------------------------
    // State machine
    // -------------------------------------------------------------------------

    private enum DogState { Wander, Follow, Return }
    private DogState state = DogState.Wander;

    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    /// <summary>Current happiness in [0, 1].</summary>
    public float Happiness { get; private set; } = 0.5f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private NavMeshAgent   agent;
    private Transform      playerTransform;
    private Vector3        homePosition;
    private bool           homeSet;

    private float          lastInteractionTime = -99f;
    private float          lastAlertTime       = -999f;
    private bool           isInteracting;
    private bool           wanderPaused;

    // Interaction highlight ring
    private GameObject     highlightRing;
    private Material       highlightMat;
    private const float    RingSize  = 1.2f;
    private const float    RingThick = 0.08f;

    private const float AlertCheckInterval = 30f;
    private const float AlertCooldown      = 120f;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed  = 300f;
        agent.acceleration  = 14f;
        agent.autoBraking   = true;

        if (dogAnimator == null)
            dogAnimator = GetComponentInChildren<Animator>();

        BuildHighlightRing();
    }

    private void Start()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) playerTransform = go.transform;

        if (playerTransform == null)
            Debug.LogWarning("[DogController] Player not found. Tag the player 'Player'.");

        // If SetHome was not yet called (e.g. placed in scene directly), home = spawn point
        if (!homeSet) homePosition = transform.position;

        StartCoroutine(WanderRoutine());
        StartCoroutine(CropAlertRoutine());
    }

    private void Update()
    {
        if (playerTransform == null) return;

        if (!isInteracting)
        {
            EvaluateState();
            ExecuteState();
        }

        DrainHappiness();
        SyncGrowthBonus();
        HandleInteractionInput();
        UpdatePromptVisibility();
    }

    private void OnDestroy()
    {
        if (FarmingManager.Instance != null)
            FarmingManager.Instance.DogGrowthBonus = 0f;
        if (highlightRing != null) Destroy(highlightRing);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Called by DogManager immediately after spawning.</summary>
    public void SetHome(Vector3 position)
    {
        homePosition = position;
        homeSet      = true;
    }

    /// <summary>Called by SaveManager after load to restore persisted happiness.</summary>
    public void SetHappiness(float value)
    {
        Happiness = Mathf.Clamp01(value);
    }

    // -------------------------------------------------------------------------
    // State machine — evaluation
    // -------------------------------------------------------------------------

    private void EvaluateState()
    {
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float distToHome   = Vector3.Distance(transform.position, homePosition);

        switch (state)
        {
            case DogState.Wander:
                if (distToPlayer <= followTriggerRange)
                    TransitionTo(DogState.Follow);
                break;

            case DogState.Follow:
                if (distToPlayer > returnRange)
                    TransitionTo(DogState.Return);
                break;

            case DogState.Return:
                if (distToHome <= 2f)
                    TransitionTo(DogState.Wander);
                else if (distToPlayer <= reFollowRange)
                    TransitionTo(DogState.Follow);
                break;
        }
    }

    private void TransitionTo(DogState next)
    {
        state = next;
        agent.ResetPath();

        if (next == DogState.Return)
            agent.SetDestination(homePosition);
    }

    // -------------------------------------------------------------------------
    // State machine — execution
    // -------------------------------------------------------------------------

    private void ExecuteState()
    {
        switch (state)
        {
            case DogState.Wander:
                // WanderRoutine coroutine drives movement; here we just sync speed
                float wanderSpeed = agent.velocity.sqrMagnitude > 0.05f ? 1f : 0f;
                SetAnimatorSpeed(wanderSpeed);
                agent.speed = walkSpeed;
                break;

            case DogState.Follow:
                ExecuteFollow();
                break;

            case DogState.Return:
                agent.speed = walkSpeed;
                SetAnimatorSpeed(agent.velocity.sqrMagnitude > 0.05f ? 1f : 0f);
                break;
        }
    }

    private void ExecuteFollow()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist > followStopDistance)
        {
            bool shouldRun = dist > runThreshold;
            agent.speed    = shouldRun ? runSpeed : walkSpeed;

            Vector3 awayDir = (transform.position - playerTransform.position).normalized;
            Vector3 target  = playerTransform.position + awayDir * followStopDistance;

            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(playerTransform.position);

        }
        else
        {
            agent.ResetPath();
        }

        // Drive animation from actual movement, not distance — prevents walk flicker at stop boundary
        bool moving = agent.velocity.sqrMagnitude > 0.05f;
        float dist2 = Vector3.Distance(transform.position, playerTransform.position);
        SetAnimatorSpeed(moving ? (dist2 > runThreshold ? 2f : 1f) : 0f);
    }

    // -------------------------------------------------------------------------
    // Wander coroutine
    // -------------------------------------------------------------------------

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            // Only pick waypoints in Wander state and when not interacting
            if (state != DogState.Wander || isInteracting || wanderPaused)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Random bark during wander (occasional, ~10% chance per pick)
            if (Random.value < 0.1f)
                AudioManager.Instance?.PlayDogBark();

            // Pick a random point within wanderRadius of home
            Vector2 circle   = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = homePosition + new Vector3(circle.x, 0f, circle.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);

            // Wait until arrived or state changes
            float timeout = 8f;
            while (timeout > 0f && state == DogState.Wander && !isInteracting)
            {
                if (!agent.pathPending && agent.remainingDistance < 0.5f) break;
                timeout -= Time.deltaTime;
                yield return null;
            }

            // Idle pause at waypoint
            float idleTimer = wanderIdleTime + Random.Range(-1f, 1f);
            float elapsed   = 0f;
            while (elapsed < idleTimer && state == DogState.Wander && !isInteracting)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Animation helpers
    // -------------------------------------------------------------------------

    private void SetAnimatorSpeed(float target)
    {
        if (dogAnimator != null)
            dogAnimator.SetFloat(ParamSpeed, target, 0.12f, Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Pet interaction (Phase 2)
    // -------------------------------------------------------------------------

    private void HandleInteractionInput()
    {
        if (!Input.GetKeyDown(interactKey)) return;
        if (playerTransform == null || isInteracting) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > interactionRange) return;
        if (Time.time - lastInteractionTime < interactionCooldown) return;

        var interaction = playerTransform.GetComponent<PlayerInteraction>();
        CropData crop   = interaction != null ? interaction.SelectedCrop : null;

        if (crop != null) Feed(crop);
        else              Pet();
    }

    /// <summary>Pet the dog — happiness gain, Pet trigger, bark, notification.</summary>
    public void Pet()
    {
        lastInteractionTime = Time.time;
        Happiness = Mathf.Min(1f, Happiness + petHappinessGain);

        FacePlayer();
        AudioManager.Instance?.PlayDogBark();
        HUDManager.Instance?.ShowNotification(PetMessage());
        StartCoroutine(InteractionAnimation(ParamPet, 2.0f));
    }

    private void FacePlayer()
    {
        if (playerTransform == null) return;
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    // ASCII-only — Kenney Future SDF
    private string PetMessage()
    {
        if (Happiness > 0.85f) return "Max is very happy!";
        if (Happiness > 0.55f) return "Max wags his tail!";
        return "Max enjoys the attention!";
    }

    // -------------------------------------------------------------------------
    // Optional feeding (Phase 3)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Feed the dog a crop — Eating animation, larger happiness gain.
    /// Crop is NOT consumed from inventory (sharing feels cozy).
    /// </summary>
    public void Feed(CropData crop)
    {
        lastInteractionTime = Time.time;
        Happiness = Mathf.Min(1f, Happiness + feedHappinessGain);

        FacePlayer();
        AudioManager.Instance?.PlayDogBark();
        HUDManager.Instance?.ShowNotification($"You gave Max a {crop.CropName}! He loves it!");
        StartCoroutine(InteractionAnimation(ParamEat, 2.67f));
    }

    private IEnumerator InteractionAnimation(int triggerHash, float holdSeconds)
    {
        isInteracting  = true;
        wanderPaused   = true;
        agent.ResetPath();
        SetAnimatorSpeed(0f);

        if (dogAnimator != null)
            dogAnimator.SetTrigger(triggerHash);

        yield return new WaitForSeconds(holdSeconds);
        isInteracting = false;
        wanderPaused  = false;
    }

    // -------------------------------------------------------------------------
    // Happiness + growth bonus (Phase 4)
    // -------------------------------------------------------------------------

    private void DrainHappiness()
    {
        Happiness = Mathf.Max(0f, Happiness - happinessDrainRate * Time.deltaTime);
    }

    private void SyncGrowthBonus()
    {
        if (FarmingManager.Instance == null) return;

        float bonus = 0f;
        if (Happiness > bonusThreshold)
        {
            float t = (Happiness - bonusThreshold) / (1f - bonusThreshold);
            bonus   = Mathf.Lerp(0f, maxGrowthBonus, t);
        }

        FarmingManager.Instance.DogGrowthBonus = bonus;
    }

    // -------------------------------------------------------------------------
    // Crop alert
    // -------------------------------------------------------------------------

    private IEnumerator CropAlertRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(AlertCheckInterval);

            if (Happiness < bonusThreshold) continue;
            if (Time.time - lastAlertTime < AlertCooldown) continue;
            if (FarmingManager.Instance == null) continue;

            int readyCount = CountReadyCrops();
            if (readyCount <= 0) continue;

            lastAlertTime = Time.time;
            AudioManager.Instance?.PlayDogBark();

            string msg = readyCount == 1
                ? "Max is barking - a crop is ready to harvest!"
                : $"Max is barking - {readyCount} crops are ready to harvest!";

            HUDManager.Instance?.ShowNotification(msg, 4f);
        }
    }

    private int CountReadyCrops()
    {
        var farmGrid = GameManager.Instance?.FarmGrid;
        if (farmGrid == null) return 0;

        int count = 0;
        foreach (var tile in farmGrid.GetAllTiles().Values)
            if (tile.IsReadyToHarvest) count++;
        return count;
    }

    // -------------------------------------------------------------------------
    // Interaction prompt
    // -------------------------------------------------------------------------

    private void UpdatePromptVisibility()
    {
        if (playerTransform == null) return;

        float dist       = Vector3.Distance(transform.position, playerTransform.position);
        bool  inRange    = dist <= interactionRange;
        bool  onCooldown = Time.time - lastInteractionTime < interactionCooldown;
        bool  showPrompt = inRange && !onCooldown && !isInteracting;

        if (interactionPromptRoot != null)
            interactionPromptRoot.SetActive(showPrompt);

        // Ground highlight ring
        UpdateHighlightRing(showPrompt);

        // Screen-space context hint
        if (showPrompt)
        {
            int happyPct = Mathf.RoundToInt(Happiness * 100f);
            var interaction = playerTransform.GetComponent<PlayerInteraction>();
            bool hasCrop = interaction != null && interaction.SelectedCrop != null;
            string action = hasCrop ? $"E: Feed Max  -  Happy: {happyPct}%" : $"E: Pet Max  -  Happy: {happyPct}%";
            HUDManager.Instance?.SetContextHint(action);
        }
    }

    // -------------------------------------------------------------------------
    // Highlight ring
    // -------------------------------------------------------------------------

    private void BuildHighlightRing()
    {
        highlightRing = new GameObject("DogHighlightRing");
        highlightRing.transform.SetParent(transform, false);
        highlightRing.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        highlightRing.SetActive(false);

        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        highlightMat = new Material(shader);
        highlightMat.SetFloat("_Surface", 1f);
        highlightMat.SetFloat("_Blend",   0f);
        highlightMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        highlightMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        highlightMat.SetFloat("_ZWrite", 0f);
        highlightMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        highlightMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Four quads forming a hollow square ring, same technique as tile highlight
        float h = RingSize / 2f;
        float i = (RingSize - 2f * RingThick) / 2f;
        CreateRingEdge(new Vector3( 0,  0,  h), new Vector3(RingSize, RingThick, 1f));
        CreateRingEdge(new Vector3( 0,  0, -h), new Vector3(RingSize, RingThick, 1f));
        CreateRingEdge(new Vector3(-h,  0,  0), new Vector3(RingThick, RingSize - 2f * RingThick, 1f));
        CreateRingEdge(new Vector3( h,  0,  0), new Vector3(RingThick, RingSize - 2f * RingThick, 1f));
    }

    private void CreateRingEdge(Vector3 localPos, Vector3 localScale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.transform.SetParent(highlightRing.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale    = localScale;
        go.layer = 2; // Ignore Raycast
        Destroy(go.GetComponent<Collider>());
        var r = go.GetComponent<Renderer>();
        r.material          = highlightMat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows    = false;
    }

    private void UpdateHighlightRing(bool show)
    {
        if (highlightRing == null) return;
        highlightRing.SetActive(show);
        if (!show) return;

        // Gentle pulse between soft yellow and white
        float t     = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.PI);
        Color color = Color.Lerp(new Color(1f, 0.92f, 0.3f, 0.7f), new Color(1f, 1f, 1f, 0.9f), t);
        highlightMat.SetColor("_BaseColor", color);

        // Slight scale pulse
        float s = Mathf.Lerp(0.95f, 1.05f, t);
        highlightRing.transform.localScale = Vector3.one * s;
    }
}
