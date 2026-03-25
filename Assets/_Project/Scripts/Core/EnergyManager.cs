using UnityEngine;

/// <summary>
/// Tracks the player's daily energy.
/// Energy restores fully each time the player sleeps (AdvanceDay).
/// Actions that cost energy should call TrySpend() — returns false if not enough.
/// </summary>
public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    public const int MaxEnergy = 100;

    public int CurrentEnergy { get; private set; } = MaxEnergy;

    public event System.Action<int, int> OnEnergyChanged; // current, max

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnDayChanged += OnDayChanged;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnDayChanged -= OnDayChanged;
    }

    private void OnDayChanged(int day, Season season, int year)
    {
        RestoreEnergy();
    }

    /// <summary>
    /// Attempts to spend energy. Returns false (and does nothing) if not enough energy.
    /// </summary>
    public bool TrySpend(int amount)
    {
        if (CurrentEnergy < amount)
        {
            HUDManager.Instance?.ShowNotification("Not enough energy! Sleep to restore.", 2.5f);
            return false;
        }

        CurrentEnergy = Mathf.Max(0, CurrentEnergy - amount);
        OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
        return true;
    }

    /// <summary>Restores a set amount of energy (e.g. from eating food).</summary>
    public void Restore(int amount)
    {
        CurrentEnergy = Mathf.Min(MaxEnergy, CurrentEnergy + amount);
        OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
    }

    /// <summary>Full restore — called when sleeping.</summary>
    public void RestoreEnergy()
    {
        CurrentEnergy = MaxEnergy;
        OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
    }

    // ── Save / Load ──────────────────────────────────────────────────────────

    public int ToSaveData()                  => CurrentEnergy;
    public void LoadFromSaveData(int energy) => CurrentEnergy = Mathf.Clamp(energy, 0, MaxEnergy);
}
