using UnityEngine;

/// <summary>
/// Tracks the in-game calendar: Day (1-28), Season, and Year.
/// Time only advances when AdvanceDay() is called (i.e. when the player sleeps).
///
/// Subscribe to events to react to day/season/year changes:
///   GameTimeManager.Instance.OnDayChanged   += MyHandler;
///   GameTimeManager.Instance.OnSeasonChanged += MyHandler;
///   GameTimeManager.Instance.OnYearChanged   += MyHandler;
/// </summary>
public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    public const int DaysPerSeason = 28;

    public int    CurrentDay    { get; private set; } = 1;
    public Season CurrentSeason { get; private set; } = Season.Spring;
    public int    CurrentYear   { get; private set; } = 1;

    // Fires after the calendar has been updated
    public event System.Action<int, Season, int> OnDayChanged;    // day, season, year
    public event System.Action<Season, Season>   OnSeasonChanged; // oldSeason, newSeason
    public event System.Action<int>              OnYearChanged;   // newYear

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Advances to the next day. Call this when the player sleeps.
    /// Fires season/year events automatically when the calendar rolls over.
    /// </summary>
    public void AdvanceDay()
    {
        CurrentDay++;

        if (CurrentDay > DaysPerSeason)
        {
            CurrentDay = 1;
            AdvanceSeason();
        }

        OnDayChanged?.Invoke(CurrentDay, CurrentSeason, CurrentYear);
        Debug.Log($"[Time] Day {CurrentDay}, {CurrentSeason}, Year {CurrentYear}");
    }

    private void AdvanceSeason()
    {
        Season oldSeason = CurrentSeason;
        int nextIndex    = ((int)CurrentSeason + 1) % 4;

        if (nextIndex == 0) // wrapped back to Spring
            AdvanceYear();

        CurrentSeason = (Season)nextIndex;
        OnSeasonChanged?.Invoke(oldSeason, CurrentSeason);
        Debug.Log($"[Time] Season changed: {oldSeason} -> {CurrentSeason}");
    }

    private void AdvanceYear()
    {
        CurrentYear++;
        OnYearChanged?.Invoke(CurrentYear);
        Debug.Log($"[Time] Year {CurrentYear} begins.");
    }

    // ── Save / Load ──────────────────────────────────────────────────────────

    public TimeSaveData ToSaveData() => new TimeSaveData
    {
        day    = CurrentDay,
        season = (int)CurrentSeason,
        year   = CurrentYear,
    };

    public void LoadFromSaveData(TimeSaveData data)
    {
        CurrentDay    = Mathf.Max(1, data.day);
        CurrentSeason = (Season)Mathf.Clamp(data.season, 0, 3);
        CurrentYear   = Mathf.Max(1, data.year);
    }
}

[System.Serializable]
public class TimeSaveData
{
    public int day;
    public int season;
    public int year;
}
