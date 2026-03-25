using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen fade to black between days.
/// Call DayTransition.Instance.Play() to trigger.
/// Automatically created at runtime — no prefab needed.
/// </summary>
public class DayTransition : MonoBehaviour
{
    public static DayTransition Instance { get; private set; }

    private Image overlay;
    private TMPro.TextMeshProUGUI label;

    private const float FadeTime = 0.6f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    private void BuildOverlay()
    {
        var canvas = new GameObject("DayTransitionCanvas").AddComponent<Canvas>();
        canvas.transform.SetParent(transform);
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        canvas.gameObject.AddComponent<CanvasScaler>();

        var bg = new GameObject("Overlay").AddComponent<Image>();
        bg.transform.SetParent(canvas.transform, false);
        bg.color = new Color(0f, 0f, 0f, 0f);
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.offsetMin = Vector2.zero;
        bg.rectTransform.offsetMax = Vector2.zero;
        overlay = bg;

        var textGO = new GameObject("Label");
        textGO.transform.SetParent(canvas.transform, false);
        label = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        label.alignment = TMPro.TextAlignmentOptions.Center;
        label.fontSize  = 28f;
        label.color     = new Color(1f, 1f, 1f, 0f);
        var rt = label.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(600f, 80f);
        rt.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Fades out, calls midAction (advance day, save, etc.), then fades back in.
    /// </summary>
    public void Play(string morningText, System.Action midAction)
    {
        StartCoroutine(RunTransition(morningText, midAction));
    }

    private IEnumerator RunTransition(string morningText, System.Action midAction)
    {
        // Fade to black
        yield return Fade(0f, 1f, FadeTime);

        // Show morning message
        label.text  = morningText;
        yield return FadeLabel(0f, 1f, 0.3f);

        // Fire the actual day advance + save
        midAction?.Invoke();

        yield return new WaitForSeconds(1.2f);

        // Fade label out, then screen in
        yield return FadeLabel(1f, 0f, 0.3f);
        label.text = "";
        yield return Fade(1f, 0f, FadeTime);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            overlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        overlay.color = new Color(0f, 0f, 0f, to);
    }

    private IEnumerator FadeLabel(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            label.color = new Color(1f, 1f, 1f, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        label.color = new Color(1f, 1f, 1f, to);
    }
}
