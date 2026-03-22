using UnityEngine;

/// <summary>
/// Handles player movement using WASD input.
/// Uses Unity's CharacterController for smooth movement with collision.
/// Supports polyperfect character models with walk/idle animations.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Character Model")]
    [SerializeField] private Transform characterModel; // Drag polyperfect character here
    [SerializeField] private float modelScale = 3f;    // Polyperfect people are small — try 3-5
    [SerializeField] private Vector3 modelOffset = new Vector3(0f, -1f, 0f);

    [Header("Animation")]
    [SerializeField] private Animator animator; // Assign CTL_People_Walk controller

    private CharacterController controller;
    private Vector3 velocity;
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int Speed = Animator.StringToHash("Speed");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("[PlayerController] CharacterController missing!", gameObject);
            enabled = false;
        }
    }

    private void Start()
    {
        if (characterModel != null)
        {
            characterModel.localScale = Vector3.one * modelScale;
            characterModel.localPosition = modelOffset;
        }
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
        Transform cam = mainCam.transform;

        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
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

        // Animation
        if (animator != null)
        {
            animator.SetBool(IsWalking, isMoving);
            animator.SetFloat(Speed, isMoving ? 1f : 0f, 0.1f, Time.deltaTime);
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
