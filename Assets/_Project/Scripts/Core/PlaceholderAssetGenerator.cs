using UnityEngine;

/// <summary>
/// Generates placeholder 3D primitive prefabs for crops and tiles at runtime.
/// This replaces the need for real 3D assets during prototyping.
/// Attach to a GameObject in the scene and call GenerateAll() or let it run on Start.
/// </summary>
public class PlaceholderAssetGenerator : MonoBehaviour
{
    [Header("Auto Generate on Start")]
    [SerializeField] private bool generateOnStart = true;

    [Header("Generated Parents")]
    private GameObject cropParent;

    private void Start()
    {
        if (generateOnStart)
            GenerateAll();
    }

    public void GenerateAll()
    {
        CreateTileMaterials();
        Debug.Log("[PlaceholderAssetGenerator] All placeholder assets generated!");
    }

    private void CreateTileMaterials()
    {
        // Ground plane material — warm green
        var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        groundMat.color = new Color(0.44f, 0.71f, 0.35f);
        groundMat.name = "GroundGrass_Runtime";

        // Apply to any object tagged Ground
        var ground = GameObject.FindWithTag("Ground");
        if (ground != null)
            ground.GetComponent<Renderer>()?.SetMaterial(groundMat);
    }

    /// <summary>
    /// Creates a simple placeholder crop visual for a given growth stage.
    /// Stage 0: tiny sphere, 1: small capsule, 2: medium capsule, 3: full sphere with glow colour
    /// </summary>
    public static GameObject CreatePlaceholderCrop(int stage, Color cropColour, Vector3 position)
    {
        GameObject go = new GameObject($"Crop_Stage{stage}");
        go.transform.position = position;

        GameObject visual = null;

        switch (stage)
        {
            case 0: // Planted — tiny brown mound
                visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.transform.localScale = new Vector3(0.15f, 0.08f, 0.15f);
                SetMaterialColor(visual, new Color(0.38f, 0.25f, 0.15f));
                break;

            case 1: // Sprouting — small green shoot
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
                visual.transform.localPosition = new Vector3(0, 0.2f, 0);
                SetMaterialColor(visual, new Color(0.3f, 0.7f, 0.2f));
                break;

            case 2: // Growing — medium plant
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.localScale = new Vector3(0.2f, 0.35f, 0.2f);
                visual.transform.localPosition = new Vector3(0, 0.35f, 0);
                SetMaterialColor(visual, cropColour * 0.8f);
                break;

            case 3: // Ready — full size with crop colour + sparkle
                visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.transform.localScale = new Vector3(0.45f, 0.55f, 0.45f);
                visual.transform.localPosition = new Vector3(0, 0.4f, 0);
                SetMaterialColor(visual, cropColour);

                // Add a subtle emission glow for "ready to harvest"
                var mat = visual.GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", cropColour * 0.3f);
                break;
        }

        if (visual != null)
        {
            visual.transform.SetParent(go.transform, false);
            // Remove collider from visual — only parent needs it
            Destroy(visual.GetComponent<Collider>());
        }

        return go;
    }

    private static void SetMaterialColor(GameObject obj, Color color)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        renderer.material = mat;
    }
}

// Extension to set material cleanly
public static class RendererExtensions
{
    public static void SetMaterial(this Renderer renderer, Material mat)
    {
        if (renderer != null) renderer.material = mat;
    }
}
