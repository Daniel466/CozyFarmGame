using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the DogPanel happiness fill bar and percentage label in the HUD each frame.
/// Attach to the DogPanel GameObject.
///
/// DogManager calls SetDogPanelVisible(true/false) when a dog is spawned or despawned.
/// The panel is hidden by default and only shown when a doghouse is placed.
/// </summary>
public class DogHappinessHUD : MonoBehaviour
{
    public static DogHappinessHUD Instance { get; private set; }

    [SerializeField] private RectTransform   happinessFill;
    [SerializeField] private TextMeshProUGUI happinessValueLabel;

    private static readonly Color ColorSad   = new Color(0.85f, 0.25f, 0.20f);
    private static readonly Color ColorHappy = new Color(0.35f, 0.82f, 0.40f);

    private const string ContextHintWithDog    = "B: Shop  |  Tab: Inv  |  G: Build  |  E: Pet Dog";
    private const string ContextHintWithoutDog = "B: Shop  |  Tab: Inventory  |  G: Build";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        AutoFindReferences();
        // Start hidden — DogManager shows it when doghouse is placed
        gameObject.SetActive(false);
    }

    private void Update()
    {
        var dog = DogManager.Instance?.ActiveDog;
        if (dog == null) return;

        float h = dog.Happiness;

        if (happinessFill != null)
        {
            Vector2 max = happinessFill.anchorMax;
            max.x = h;
            happinessFill.anchorMax = max;

            var img = happinessFill.GetComponent<Image>();
            if (img != null)
                img.color = Color.Lerp(ColorSad, ColorHappy, h);
        }

        if (happinessValueLabel != null)
            happinessValueLabel.text = $"{Mathf.RoundToInt(h * 100f)}%";
    }

    // -------------------------------------------------------------------------
    // Public API — called by DogManager
    // -------------------------------------------------------------------------

    /// <summary>Show or hide the DogPanel and update the controls overlay hint.</summary>
    public void SetDogPanelVisible(bool visible)
    {
        gameObject.SetActive(visible);
        HUDManager.Instance?.SetContextHint(visible ? ContextHintWithDog : ContextHintWithoutDog);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void AutoFindReferences()
    {
        if (happinessFill == null)
        {
            var bg = transform.Find("DogHappinessBG");
            if (bg != null)
            {
                var fill = bg.Find("DogHappinessFill");
                if (fill != null) happinessFill = fill.GetComponent<RectTransform>();
            }
        }

        if (happinessValueLabel == null)
        {
            var labelGO = transform.Find("DogHappinessValue");
            if (labelGO != null) happinessValueLabel = labelGO.GetComponent<TextMeshProUGUI>();
        }
    }
}
