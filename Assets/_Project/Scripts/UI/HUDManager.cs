using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the main HUD display: coins, XP bar, level, and tool indicator.
/// Attach to a Canvas GameObject in your scene.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Coins")]
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Level & XP")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI xpText;

    [Header("Level Up Panel")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private float levelUpDisplayTime = 3f;

    [Header("Tool Indicator")]
    [SerializeField] private TextMeshProUGUI toolText;

    [Header("Notification")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Hide panels first (safe before GameManager is ready)
        if (levelUpPanel) levelUpPanel.SetActive(false);
        if (notificationPanel) notificationPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // Delay subscription by one frame to ensure all systems are initialised
        StartCoroutine(SubscribeNextFrame());
    }

    private System.Collections.IEnumerator SubscribeNextFrame()
    {
        yield return null; // Wait one frame

        if (GameManager.Instance == null) yield break;

        // Subscribe to events
        GameManager.Instance.Economy.OnCoinsChanged.AddListener(UpdateCoins);
        GameManager.Instance.Progression.OnXPChanged.AddListener(UpdateXP);
        GameManager.Instance.Progression.OnLevelUp.AddListener(ShowLevelUp);

        // Initial update
        UpdateCoins(GameManager.Instance.Economy.Coins);
        UpdateXP(GameManager.Instance.Progression.CurrentXP,
                 GameManager.Instance.Progression.CurrentLevel);
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.Economy.OnCoinsChanged.RemoveListener(UpdateCoins);
        GameManager.Instance.Progression.OnXPChanged.RemoveListener(UpdateXP);
        GameManager.Instance.Progression.OnLevelUp.RemoveListener(ShowLevelUp);
    }

    /// <summary>
    /// Called by HUDBootstrapper to wire up all UI references at runtime.
    /// </summary>
    public void Setup(
        TextMeshProUGUI coins,
        TextMeshProUGUI level,
        Slider xp,
        TextMeshProUGUI xpLabel,
        GameObject levelUp,
        TextMeshProUGUI levelUpLabel,
        GameObject notification,
        TextMeshProUGUI notificationLabel,
        TextMeshProUGUI tool)
    {
        coinsText = coins;
        levelText = level;
        xpSlider = xp;
        xpText = xpLabel;
        levelUpPanel = levelUp;
        levelUpText = levelUpLabel;
        notificationPanel = notification;
        notificationText = notificationLabel;
        toolText = tool;
    }

    private void UpdateCoins(int coins)
    {
        if (coinsText) coinsText.text = $"🪙 {coins:N0}";
    }

    private void UpdateXP(int xp, int level)
    {
        if (levelText) levelText.text = $"Lv. {level}";

        float progress = GameManager.Instance.Progression.LevelProgress();
        if (xpSlider) xpSlider.value = progress;

        int xpToNext = GameManager.Instance.Progression.XPForNextLevel();
        if (xpText)
        {
            if (level >= GameManager.Instance.Progression.MaxLevel)
                xpText.text = "MAX";
            else
                xpText.text = $"{xpToNext} XP to next level";
        }
    }

    private void ShowLevelUp(int newLevel)
    {
        if (levelUpPanel == null) return;
        levelUpPanel.SetActive(true);
        if (levelUpText) levelUpText.text = $"Level Up!\nLevel {newLevel}";
        Invoke(nameof(HideLevelUp), levelUpDisplayTime);
    }

    private void HideLevelUp()
    {
        if (levelUpPanel) levelUpPanel.SetActive(false);
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationPanel == null) return;
        notificationPanel.SetActive(true);
        if (notificationText) notificationText.text = message;
        CancelInvoke(nameof(HideNotification));
        Invoke(nameof(HideNotification), duration);
    }

    private void HideNotification()
    {
        if (notificationPanel) notificationPanel.SetActive(false);
    }

    public void UpdateToolIndicator(string toolName)
    {
        if (toolText) toolText.text = toolName;
    }
}
