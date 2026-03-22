using UnityEngine;

/// <summary>
/// Farm Together-style isometric camera.
/// Smoothly follows the player. Scroll wheel zooms. No rotation.
/// </summary>
public class FarmCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Position")]
    [SerializeField] private float height = 22f;
    [SerializeField] private float distance = 16f;
    [SerializeField] private float yawAngle = 45f;    // Fixed compass heading (degrees)
    [SerializeField] private float pitchAngle = 45f;  // Fixed tilt down (degrees)

    [Header("Follow")]
    [SerializeField] private float followSpeed = 6f;  // Lower = more lag/polish

    [Header("Zoom")]
    [SerializeField] private float minDistance = 8f;
    [SerializeField] private float maxDistance = 40f;
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float zoomSmoothSpeed = 8f;

    private float targetDistance;

    private void Start()
    {
        targetDistance = distance;

        // Snap to correct position immediately (no slide-in on first frame)
        if (target != null)
            transform.position = DesiredPosition();
    }

    private void Update()
    {
        HandleZoom();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Smooth zoom
        distance = Mathf.Lerp(distance, targetDistance, zoomSmoothSpeed * Time.deltaTime);

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, DesiredPosition(), followSpeed * Time.deltaTime);

        // Always look toward the player (slightly above ground for nicer framing)
        transform.LookAt(target.position + Vector3.up * 1f);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;
        targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed * 10f, minDistance, maxDistance);
    }

    private Vector3 DesiredPosition()
    {
        // Build offset from a fixed yaw + pitch — no runtime rotation
        float yawRad   = yawAngle   * Mathf.Deg2Rad;
        float pitchRad = pitchAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Sin(yawRad) * distance,
            height,
            -Mathf.Cos(yawRad) * distance
        );

        return target.position + offset;
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}
