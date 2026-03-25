using UnityEngine;

/// <summary>
/// Low-level physical movement only.
/// Owns: CharacterController, gravity, rotation, moving in a world-space direction.
/// No input. No farming. No animation. No auto-move logic.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed     = 5f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float gravity       = -9.81f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayerMask = ~0;

    private CharacterController _cc;
    private Vector3 _velocity;

    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (_cc == null) Debug.LogError("[PlayerMotor] CharacterController missing!");
    }

    private void Start()
    {
        SnapToGround();
    }

    /// <summary>
    /// Raycasts down and repositions the player so the CC capsule bottom
    /// sits exactly on the ground surface. Fixes mesh-pivot mismatches.
    /// </summary>
    private void SnapToGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 3f;
        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f, groundLayerMask)) return;

        // CC capsule bottom = center.y - height/2. We want that to sit on the hit surface.
        float capsuleBottomOffset = _cc.center.y - _cc.height * 0.5f;
        Vector3 snapped = transform.position;
        snapped.y = hit.point.y - capsuleBottomOffset;

        _cc.enabled = false;
        transform.position = snapped;
        _cc.enabled = true;

        Debug.Log($"[PlayerMotor] Snapped to ground at Y={snapped.y:F3} (hit '{hit.collider.name}' at Y={hit.point.y:F3})");
    }

    /// <summary>Move in a world-space direction this frame. Applies gravity.</summary>
    public void Move(Vector3 worldDirection)
    {
        ApplyGravity();
        _cc.Move((worldDirection * moveSpeed + _velocity) * Time.deltaTime);

        if (worldDirection.sqrMagnitude > 0.01f)
            FaceDirection(worldDirection);
    }

    /// <summary>Apply gravity only — no horizontal movement.</summary>
    public void ApplyGravityOnly()
    {
        ApplyGravity();
        _cc.Move(_velocity * Time.deltaTime);
    }

    /// <summary>Instantly face a world-space direction (ignores Y).</summary>
    public void FaceDirection(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        Quaternion target = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
    }

    /// <summary>Instantly face a world position (ignores Y).</summary>
    public void FacePosition(Vector3 worldPos)
    {
        FaceDirection(worldPos - transform.position);
    }

    private void ApplyGravity()
    {
        if (_cc.isGrounded && _velocity.y < 0f) _velocity.y = -2f;
        _velocity.y += gravity * Time.deltaTime;
    }
}
