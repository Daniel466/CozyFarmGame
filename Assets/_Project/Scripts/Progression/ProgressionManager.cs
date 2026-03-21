using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages player XP and leveling (levels 1-15 as per GDD).
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    private static readonly int[] XPThresholds = {
        0, 100, 250, 450, 700, 1000, 1400, 1900, 2500, 3200, 4000, 5000, 6200, 7500, 9000
    };

    private int currentXP;
    private int currentLevel = 1;

    public int CurrentXP => currentXP;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => XPThresholds.Length;

    public UnityEvent<int, int> OnXPChanged = new UnityEvent<int, int>(); // xp, level
    public UnityEvent<int> OnLevelUp = new UnityEvent<int>();             // new level

    public void AddXP(int amount)
    {
        currentXP += amount;

        // Check for level up
        while (currentLevel < MaxLevel && currentXP >= XPThresholds[currentLevel])
        {
            currentLevel++;
            Debug.Log($"[Progression] Level Up! Now level {currentLevel}");
            OnLevelUp?.Invoke(currentLevel);
        }

        OnXPChanged?.Invoke(currentXP, currentLevel);
    }

    public int XPForNextLevel()
    {
        if (currentLevel >= MaxLevel) return 0;
        return XPThresholds[currentLevel] - currentXP;
    }

    public float LevelProgress()
    {
        if (currentLevel >= MaxLevel) return 1f;
        int prevThreshold = currentLevel > 1 ? XPThresholds[currentLevel - 1] : 0;
        int nextThreshold = XPThresholds[currentLevel];
        return (float)(currentXP - prevThreshold) / (nextThreshold - prevThreshold);
    }

    public void SetState(int xp, int level)
    {
        currentXP = xp;
        currentLevel = level;
        OnXPChanged?.Invoke(currentXP, currentLevel);
    }
}
