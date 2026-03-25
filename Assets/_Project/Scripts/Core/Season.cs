/// <summary>
/// The four farm seasons. Used by GameTimeManager and CropData.
/// </summary>
public enum Season { Spring, Summer, Fall, Winter }

/// <summary>
/// Flags version used on CropData so crops can belong to multiple seasons.
/// e.g. Leek grows in Fall | Winter.
/// </summary>
[System.Flags]
public enum GrowingSeason
{
    None   = 0,
    Spring = 1 << 0,
    Summer = 1 << 1,
    Fall   = 1 << 2,
    Winter = 1 << 3,
}

public static class SeasonExtensions
{
    /// <summary>Returns true if the crop's growing seasons include the current season.</summary>
    public static bool CanGrowIn(this GrowingSeason cropSeasons, Season current)
    {
        return current switch
        {
            Season.Spring => (cropSeasons & GrowingSeason.Spring) != 0,
            Season.Summer => (cropSeasons & GrowingSeason.Summer) != 0,
            Season.Fall   => (cropSeasons & GrowingSeason.Fall)   != 0,
            Season.Winter => (cropSeasons & GrowingSeason.Winter) != 0,
            _             => false,
        };
    }

    public static string DisplayName(this Season season) => season switch
    {
        Season.Spring => "Spring",
        Season.Summer => "Summer",
        Season.Fall   => "Fall",
        Season.Winter => "Winter",
        _             => "Unknown",
    };
}
