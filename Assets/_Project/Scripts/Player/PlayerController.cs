using UnityEngine;

/// <summary>
/// Handles player movement using WASD input.
/// Uses Unity's CharacterController for smooth movement with collision.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Read input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Relative to camera
        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        // Apply movement
        if (moveDir.magnitude > 0.1f)
        {
            controller.Move(moveDir * moveSpeed * Time.deltaTime);

            // Rotate to face direction
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Gravity
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Animation
        animator?.SetBool(IsWalking, moveDir.magnitude > 0.1f);
    }
}
