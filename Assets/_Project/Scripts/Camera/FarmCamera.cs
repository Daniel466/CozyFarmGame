using UnityEngine;

/// <summary>
/// Isometric-style farm camera.
/// Follows the player with zoom (scroll wheel) and 90-degree rotation (Q/E).
/// </summary>
public class FarmCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Position")]
    [SerializeField] private float distance = 12f;
    [SerializeField] private float height = 10f;
    [SerializeField] private float followSpeed = 8f;

    [Header("Zoom")]
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 25f;
    [SerializeField] private float zoomSpeed = 3f;

    [Header("Rotation")]
    [SerializeField] private float rotationAngle = 45f; // Starting angle
    [SerializeField] private float rotationStep = 90f;
    [SerializeField] private float rotationSpeed = 8f;

    private float targetRotation;
    private float currentRotation;

    private void Start()
    {
        targetRotation = rotationAngle;
        currentRotation = rotationAngle;
    }

    private void Update()
    {
        HandleZoom();
        HandleRotation();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Smooth rotation
        currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Calculate camera position
        Quaternion rot = Quaternion.Euler(0f, currentRotation, 0f);
        Vector3 offset = rot * new Vector3(0f, height, -distance);
        Vector3 desiredPos = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * zoomSpeed * 10f, minDistance, maxDistance);
    }

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            targetRotation -= rotationStep;

        if (Input.GetKeyDown(KeyCode.E))
            targetRotation += rotationStep;
    }
}
