using UnityEngine;

/// <summary>
/// Bootstraps the farm scene at runtime using Unity primitives.
/// Attach this to an empty GameObject called "Bootstrapper" in your scene.
/// This sets up the ground plane, tile visuals, and a placeholder player
/// so you can playtest immediately without any external assets.
/// </summary>
public class SceneBootstrapper : MonoBehaviour
{
    [Header("Ground")]
    [SerializeField] private bool createGround = true;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(20, 20);

    [Header("Player")]
    [SerializeField] private bool createPlayer = true;
    [SerializeField] private Vector3 playerStartPosition = new Vector3(10f, 1f, 10f);

    [Header("Lighting")]
    [SerializeField] private bool setupLighting = true;

    private void Awake()
    {
        if (createGround) SetupGround();
        if (createPlayer) SetupPlayer();
        if (setupLighting) SetupLighting();
    }

    private void SetupGround()
    {
        // Create a large green ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.tag = "Ground";
        ground.transform.position = new Vector3(gridSize.x * 0.5f, 0f, gridSize.y * 0.5f);
        ground.transform.localScale = new Vector3(gridSize.x * 0.1f + 0.5f,
                                                   1f,
                                                   gridSize.y * 0.1f + 0.5f);

        // Warm green URP material
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.44f, 0.71f, 0.35f);
        mat.SetFloat("_Smoothness", 0.1f);
        ground.GetComponent<Renderer>().material = mat;

        // Set layer to Ground so raycasts work
        ground.layer = LayerMask.NameToLayer("Default");
        Debug.Log("[Bootstrapper] Ground plane created.");
    }

    private void SetupPlayer()
    {
        // Check if a player already exists
        if (FindFirstObjectByType<PlayerController>() != null)
        {
            Debug.Log("[Bootstrapper] Player already exists, skipping.");
            return;
        }

        // Create a capsule as placeholder player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = playerStartPosition;

        // Apply a friendly blue colour
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.3f, 0.55f, 0.9f);
        player.GetComponent<Renderer>().material = mat;

        // Remove default collider, add CharacterController
        Destroy(player.GetComponent<CapsuleCollider>());
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 0, 0);

        // Add player scripts
        player.AddComponent<PlayerController>();
        var interaction = player.AddComponent<PlayerInteraction>();

        // Add camera targeting
        var cam = Camera.main;
        if (cam != null)
        {
            var farmCam = cam.GetComponent<FarmCamera>();
            if (farmCam == null) farmCam = cam.gameObject.AddComponent<FarmCamera>();

            // Use reflection to set the private target field via SerializedObject workaround
            // Instead, expose target via a public setter
            farmCam.SetTarget(player.transform);
        }

        // Add a hat (small cube on top) for personality
        GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hat.name = "Hat";
        hat.transform.SetParent(player.transform);
        hat.transform.localPosition = new Vector3(0, 0.75f, 0);
        hat.transform.localScale = new Vector3(0.6f, 0.25f, 0.6f);
        Destroy(hat.GetComponent<Collider>());
        var hatMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hatMat.color = new Color(0.4f, 0.25f, 0.1f); // Brown hat
        hat.GetComponent<Renderer>().material = hatMat;

        Debug.Log("[Bootstrapper] Placeholder player created.");
    }

    private void SetupLighting()
    {
        // Find or create directional light
        Light sun = FindFirstObjectByType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.color = new Color(1.0f, 0.95f, 0.80f);
            sun.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.5f;
        }

        // Ambient gradient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.75f, 0.95f);
        RenderSettings.ambientEquatorColor = new Color(0.70f, 0.82f, 0.65f);
        RenderSettings.ambientGroundColor = new Color(0.38f, 0.28f, 0.18f);

        // Subtle fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.80f, 0.88f, 0.95f);
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 80f;

        Debug.Log("[Bootstrapper] Lighting configured.");
    }
}
