using UnityEngine;

/// <summary>
/// Farm Together-style full orbit camera.
/// - Pivot SmoothDamps to player position
/// - Q/E or middle-mouse drag to orbit horizontally (yaw)
/// - Middle-mouse drag vertically to tilt (pitch), clamped to avoid clipping
/// - Scroll wheel zooms smoothly
/// - All transitions use SmoothDamp for weighted, damped feel
/// - SphereCast collision prevents clipping through terrain/buildings
/// </summary>
public class FarmCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private float followDamping = 0.12f;   // Lower = tighter, 0.05 = snappy

    [Header("Orbit")]
    [SerializeField] private float yaw            = 45f;    // Starting horizontal angle
    [SerializeField] private float pitch          = 55f;    // Starting vertical tilt
    [SerializeField] private float minPitch       = 30f;    // Can't go below horizon
    [SerializeField] private float maxPitch       = 75f;    // Can't go full top-down
    [SerializeField] private float orbitDamping   = 0.10f;  // Orbit smoothing time

    [Header("Rotation Input")]
    [SerializeField] private float keyRotateSpeed  = 90f;   // Degrees/sec for Q/E
    [SerializeField] private float mouseSensitivity = 0.25f; // Degrees/pixel for drag

    [Header("Zoom")]
    [SerializeField] private float distance        = 20f;
    [SerializeField] private float minDistance     = 12f;
    [SerializeField] private float maxDistance     = 35f;
    [SerializeField] private float scrollSensitivity = 3f;
    [SerializeField] private float zoomDamping     = 0.15f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius   = 0.35f;

    // ── Smoothed state ─────────────────────────────────────────────────────────
    private float currentYaw,   targetYaw,   yawVelocity;
    private float currentPitch, targetPitch, pitchVelocity;
    private float currentDist,  targetDist,  distVelocity;
    private Vector3 pivot, pivotVelocity;

    // ── Middle-mouse drag ──────────────────────────────────────────────────────
    private bool  isDragging;
    private Vector2 lastMousePos;

    // ── Init guard — snap on first frame target is available ──────────────────
    private bool ready;

    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        collisionMask &= ~(1 << 2); // always exclude Ignore Raycast layer
    }

    private void Start()
    {
        currentYaw   = targetYaw   = yaw;
        currentPitch = targetPitch = pitch;
        currentDist  = targetDist  = distance;
    }

    private void Update()
    {
        HandleMouseDrag();
        HandleKeyRotate();
        HandleScroll();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // First frame the target is available: snap instantly, no lerp slide-in
        if (!ready)
        {
            pivot        = target.position;
            currentYaw   = targetYaw;
            currentPitch = targetPitch;
            currentDist  = targetDist;
            SnapCamera();
            ready = true;
            return;
        }

        // 1. Pivot follows player
        pivot = Vector3.SmoothDamp(pivot, target.position, ref pivotVelocity, followDamping);

        // 2. Smooth orbit — SmoothDampAngle handles yaw wraparound cleanly
        currentYaw   = Mathf.SmoothDampAngle(currentYaw, targetYaw,   ref yawVelocity,   orbitDamping);
        currentPitch = Mathf.SmoothDamp      (currentPitch, targetPitch, ref pitchVelocity, orbitDamping);
        currentDist  = Mathf.SmoothDamp      (currentDist,  targetDist,  ref distVelocity,  zoomDamping);

        // 3. Spherical offset from pivot
        Vector3 offset = SphericalOffset(currentYaw, currentPitch, currentDist);

        // 4. Collision
        Vector3 desiredPos = CollisionAdjusted(pivot, offset);

        // 5. Hard floors — camera never goes underground or inside the character
        desiredPos.y = Mathf.Max(desiredPos.y, pivot.y + 1f);
        if (Vector3.Distance(desiredPos, target.position) < minDistance * 0.8f)
            desiredPos = target.position + (desiredPos - target.position).normalized * (minDistance * 0.8f);

        // 6. Apply
        transform.position = desiredPos;
        transform.LookAt(pivot + Vector3.up * 1f);
    }

    // ── Input ──────────────────────────────────────────────────────────────────

    private void HandleMouseDrag()
    {
        // Begin drag
        if (Input.GetMouseButtonDown(2))
        {
            isDragging   = true;
            lastMousePos = Input.mousePosition;
        }

        // End drag
        if (Input.GetMouseButtonUp(2))
            isDragging = false;

        if (!isDragging) return;

        Vector2 delta    = (Vector2)Input.mousePosition - lastMousePos;
        lastMousePos     = Input.mousePosition;

        targetYaw   += delta.x * mouseSensitivity;
        targetPitch -= delta.y * mouseSensitivity; // drag up → less top-down
        targetPitch  = Mathf.Clamp(targetPitch, minPitch, maxPitch);
    }

    private void HandleKeyRotate()
    {
        if (Input.GetKey(KeyCode.Q)) targetYaw -= keyRotateSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) targetYaw += keyRotateSpeed * Time.deltaTime;
    }

    private void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;
        targetDist = Mathf.Clamp(targetDist - scroll * scrollSensitivity * 10f, minDistance, maxDistance);
    }

    // ── Maths ──────────────────────────────────────────────────────────────────

    /// <summary>Converts yaw/pitch/distance to a world-space offset from pivot.</summary>
    private Vector3 SphericalOffset(float yawDeg, float pitchDeg, float dist)
    {
        float y = yawDeg   * Mathf.Deg2Rad;
        float p = pitchDeg * Mathf.Deg2Rad;
        float horizDist = dist * Mathf.Cos(p);
        return new Vector3(
             Mathf.Sin(y) * horizDist,
             dist * Mathf.Sin(p),
            -Mathf.Cos(y) * horizDist
        );
    }

    /// <summary>SphereCast from pivot toward desired camera position, snap forward if blocked.</summary>
    private Vector3 CollisionAdjusted(Vector3 pivotPos, Vector3 offset)
    {
        Vector3 desiredPos = pivotPos + offset;
        Vector3 origin     = pivotPos + Vector3.up * 2.5f;   // above head, outside character collider
        Vector3 dir        = (desiredPos - origin).normalized;
        float   maxDist    = Vector3.Distance(origin, desiredPos);

        if (Physics.SphereCast(origin, collisionRadius, dir, out RaycastHit hit, maxDist, collisionMask))
            return origin + dir * Mathf.Max(hit.distance - collisionRadius, 0f);

        return desiredPos;
    }

    /// <summary>Instantly positions the camera with no interpolation (called on Start).</summary>
    private void SnapCamera()
    {
        if (target == null) return;
        transform.position = pivot + SphericalOffset(currentYaw, currentPitch, currentDist);
        transform.LookAt(pivot + Vector3.up * 1f);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}
