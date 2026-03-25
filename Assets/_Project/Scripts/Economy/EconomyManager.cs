using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the player's coin balance.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    [Header("Starting Balance")]
    [SerializeField] private int startingCoins = 150; // Enough for ~10 carrots — feels like a fresh start

    private int coins;

    public int Coins => coins;
    public UnityEvent<int> OnCoinsChanged = new UnityEvent<int>();

    private bool initialized;

    private void Start()
    {
        if (!initialized)
        {
            coins = startingCoins;
            initialized = true;
        }
        // Delay the initial event by one frame so HUDManager has time to subscribe
        StartCoroutine(FireInitialEvent());
    }

    private System.Collections.IEnumerator FireInitialEvent()
    {
        yield return null;
        OnCoinsChanged?.Invoke(coins);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }

    public void SetCoins(int amount)
    {
        coins = amount;
        initialized = true; // prevent Start from overwriting loaded value
        OnCoinsChanged?.Invoke(coins);
    }
}
