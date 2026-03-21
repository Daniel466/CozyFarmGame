using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages player XP and leveling (levels 1-15 as per GDD).
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    private static readonly int[] XPThresholds = {
        // Level 1→2: easy start, feel progress quickly
        // Level 2→5: steady pace, unlocking basics
        // Level 6→10: meaningful grind, unlocking good stuff
        // Level 11→15: end game, prestige crops
        0, 80, 200, 380, 600, 900, 1300, 1800, 2450, 3200, 4100, 5200, 6500, 8000, 10000
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
            AudioManager.Instance?.PlayLevelUp();
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

    private void Start()
    {
        // Fire initial event after one frame so HUDManager has time to subscribe
        StartCoroutine(FireInitialEvent());
    }

    private System.Collections.IEnumerator FireInitialEvent()
    {
        yield return null;
        OnXPChanged?.Invoke(currentXP, currentLevel);
    }

    public void SetState(int xp, int level)
    {
        currentXP = xp;
        currentLevel = level;
        OnXPChanged?.Invoke(currentXP, currentLevel);
    }
}
