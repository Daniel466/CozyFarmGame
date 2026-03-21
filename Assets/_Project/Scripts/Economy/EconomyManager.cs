using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the player's coin balance.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    [Header("Starting Balance")]
    [SerializeField] private int startingCoins = 500;

    private int coins;

    public int Coins => coins;
    public UnityEvent<int> OnCoinsChanged = new UnityEvent<int>();

    private void Start()
    {
        coins = startingCoins;
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
        Debug.Log($"[Economy] +{amount} coins. Total: {coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
        {
            Debug.Log($"[Economy] Not enough coins. Have {coins}, need {amount}.");
            return false;
        }
        coins -= amount;
        OnCoinsChanged?.Invoke(coins);
        Debug.Log($"[Economy] -{amount} coins. Total: {coins}");
        return true;
    }

    public void SetCoins(int amount)
    {
        coins = amount;
        OnCoinsChanged?.Invoke(coins);
    }
}
