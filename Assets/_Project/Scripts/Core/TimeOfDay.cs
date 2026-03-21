using UnityEngine;

/// <summary>
/// Optional soft time-of-day system.
/// Gently shifts the sun colour and intensity over time for a living, cozy atmosphere.
/// Attach to the same GameObject as CozyLightingSetup.
/// </summary>
public class TimeOfDay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light sunLight;

    [Header("Settings")]
    [SerializeField] private bool enableTimeOfDay = true;
    [SerializeField] private float dayDurationSeconds = 600f; // 10 min real time = 1 game day

    [Range(0f, 1f)]
    [SerializeField] private float timeOfDay = 0.35f; // Start at mid-morning (0=midnight, 0.5=noon, 1=midnight)

    [Header("Sun Colours Throughout the Day")]
    [SerializeField] private Gradient sunColorGradient;
    [SerializeField] private AnimationCurve sunIntensityCurve;

    private void Reset()
    {
        // Default sun colour gradient: dawn orange → midday white → dusk pink
        sunColorGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.1f, 0.05f, 0.0f), 0.0f),  // Night
            new GradientColorKey(new Color(1.0f, 0.6f, 0.3f), 0.25f),  // Dawn
            new GradientColorKey(new Color(1.0f, 0.95f, 0.8f), 0.5f),  // Midday
            new GradientColorKey(new Color(1.0f, 0.65f, 0.35f), 0.75f),// Dusk
            new GradientColorKey(new Color(0.1f, 0.05f, 0.0f), 1.0f),  // Night
        };
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };
        sunColorGradient.SetKeys(colorKeys, alphaKeys);

        // Sun intensity: low at night, peaks at noon
        sunIntensityCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 0.6f),
            new Keyframe(0.5f, 1.2f),
            new Keyframe(0.75f, 0.6f),
            new Keyframe(1f, 0f)
        );
    }

    private void Update()
    {
        if (!enableTimeOfDay) return;

        // Advance time
        timeOfDay += Time.deltaTime / dayDurationSeconds;
        if (timeOfDay > 1f) timeOfDay -= 1f;

        ApplyTimeOfDay();
    }

    private void ApplyTimeOfDay()
    {
        if (sunLight == null) return;

        // Sun angle: rotate from -90° (midnight) to 270° (next midnight)
        float sunAngle = (timeOfDay * 360f) - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

        // Sun colour and intensity
        if (sunColorGradient != null)
            sunLight.color = sunColorGradient.Evaluate(timeOfDay);

        if (sunIntensityCurve != null)
            sunLight.intensity = sunIntensityCurve.Evaluate(timeOfDay);

        // Ambient light shifts slightly with time of day
        float t = timeOfDay;
        RenderSettings.ambientSkyColor = Color.Lerp(
            new Color(0.1f, 0.1f, 0.2f),   // Night sky
            new Color(0.55f, 0.75f, 0.95f), // Day sky
            sunIntensityCurve?.Evaluate(t) ?? 1f
        );
    }

    public float GetTimeOfDay() => timeOfDay;
    public void SetTimeOfDay(float t) => timeOfDay = Mathf.Clamp01(t);
}
