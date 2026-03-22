using UnityEngine;

/// <summary>
/// Handles player movement using WASD input.
/// Compatible with polyperfect CTL_People_Walk animator controller.
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

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private Vector3 velocity;

    // We'll detect the correct parameter name at runtime
    private int animParamHash = -1;
    private bool animParamIsBool = false;

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

        // Auto-detect animator parameter
        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                Debug.Log($"[PlayerController] Animator param: '{param.name}' type={param.type}");

                // Look for any movement-related parameter
                string lower = param.name.ToLower();
                if (lower.Contains("walk") || lower.Contains("move") || lower.Contains("speed") || lower.Contains("run"))
                {
                    animParamHash = param.nameHash;
                    animParamIsBool = param.type == AnimatorControllerParameterType.Bool;
                    Debug.Log($"[PlayerController] Using animator param: '{param.name}'");
                    break;
                }
            }

            if (animParamHash == -1 && animator.parameters.Length > 0)
            {
                // Just use the first parameter as fallback
                var first = animator.parameters[0];
                animParamHash = first.nameHash;
                animParamIsBool = first.type == AnimatorControllerParameterType.Bool;
                Debug.Log($"[PlayerController] Fallback to first animator param: '{first.name}'");
            }
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

        // Animation — safe set using detected parameter
        if (animator != null && animParamHash != -1)
        {
            try
            {
                if (animParamIsBool)
                    animator.SetBool(animParamHash, isMoving);
                else
                    animator.SetFloat(animParamHash, isMoving ? 1f : 0f, 0.1f, Time.deltaTime);
            }
            catch { } // Silently ignore animation errors
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
