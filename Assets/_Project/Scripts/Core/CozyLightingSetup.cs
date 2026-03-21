using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Sets up cozy warm lighting at runtime for URP.
/// Attach this to a GameObject in your scene alongside a Volume component.
/// Also configure your Directional Light using the settings below.
/// </summary>
public class CozyLightingSetup : MonoBehaviour
{
    [Header("Sun (Directional Light)")]
    [SerializeField] private Light sunLight;

    [Header("Cozy Sun Settings")]
    [SerializeField] private Color sunColor = new Color(1.0f, 0.95f, 0.80f); // Warm golden
    [SerializeField] private float sunIntensity = 1.2f;
    [SerializeField] private Vector3 sunRotation = new Vector3(50f, -30f, 0f); // Angled afternoon sun

    [Header("Ambient Light")]
    [SerializeField] private Color skyColor = new Color(0.55f, 0.75f, 0.95f);    // Soft blue sky
    [SerializeField] private Color equatorColor = new Color(0.70f, 0.82f, 0.65f); // Gentle green horizon
    [SerializeField] private Color groundColor = new Color(0.38f, 0.28f, 0.18f);  // Warm earth

    private void Start()
    {
        ApplyLighting();
    }

    public void ApplyLighting()
    {
        // --- Directional Sun Light ---
        if (sunLight != null)
        {
            sunLight.color = sunColor;
            sunLight.intensity = sunIntensity;
            sunLight.transform.rotation = Quaternion.Euler(sunRotation);
            sunLight.shadows = LightShadows.Soft;
            sunLight.shadowStrength = 0.5f; // Soft, cozy shadows
            sunLight.shadowBias = 0.05f;
        }

        // --- Ambient Lighting (Gradient) ---
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientEquatorColor = equatorColor;
        RenderSettings.ambientGroundColor = groundColor;

        // --- Fog (subtle, cozy atmospheric depth) ---
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.80f, 0.88f, 0.95f);
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 80f;
    }

#if UNITY_EDITOR
    // Allow previewing in editor
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        ApplyLighting();
    }
#endif
}
