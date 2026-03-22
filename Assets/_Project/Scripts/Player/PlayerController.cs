using UnityEngine;

/// <summary>
/// Handles player movement using WASD input.
/// Swaps between polyperfect Idle/Walk animator controllers based on movement.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Character Model")]
    [SerializeField] private Transform characterModel;
    [SerializeField] private float modelScale = 3f;
    [SerializeField] private Vector3 modelOffset = new Vector3(0f, -1f, 0f);

    [Header("Animation Controllers")]
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController idleController;  // CTL_People_Idle
    [SerializeField] private RuntimeAnimatorController walkController;  // CTL_People_Walk

    private CharacterController controller;
    private Vector3 velocity;
    private bool wasMoving = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null) { Debug.LogError("[PlayerController] CharacterController missing!"); enabled = false; }
    }

    private void Start()
    {
        if (characterModel != null)
        {
            characterModel.localScale = Vector3.one * modelScale;
            characterModel.localPosition = modelOffset;
        }

        // Start with idle
        if (animator != null && idleController != null)
            animator.runtimeAnimatorController = idleController;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (controller == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Use camera yaw only — ignores pitch so zoom level doesn't affect movement direction
        float camYaw = mainCam.transform.eulerAngles.y;
        Vector3 camForward = new Vector3(Mathf.Sin(camYaw * Mathf.Deg2Rad), 0f, Mathf.Cos(camYaw * Mathf.Deg2Rad));
        Vector3 camRight   = new Vector3(Mathf.Cos(camYaw * Mathf.Deg2Rad), 0f, -Mathf.Sin(camYaw * Mathf.Deg2Rad));
        Vector3 moveDir = (camForward * v + camRight * h).normalized;
        bool isMoving = moveDir.magnitude > 0.1f;

        if (isMoving)
        {
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Gravity
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Swap animator controller on state change
        if (animator != null && isMoving != wasMoving)
        {
            wasMoving = isMoving;
            if (isMoving && walkController != null)
                animator.runtimeAnimatorController = walkController;
            else if (!isMoving && idleController != null)
                animator.runtimeAnimatorController = idleController;
        }
    }

    public void SetCharacterModel(Transform model, float scale = 3f)
    {
        characterModel = model;
        modelScale = scale;
        if (model != null)
        {
            model.localScale = Vector3.one * scale;
            model.localPosition = modelOffset;
        }
    }
}
