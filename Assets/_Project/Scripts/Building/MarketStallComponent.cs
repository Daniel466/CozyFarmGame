using System.Collections;
using UnityEngine;

/// <summary>
/// Attached to a placed Market Stall building.
/// Auto-sells all crops in inventory at a bonus rate every interval seconds.
/// </summary>
public class MarketStallComponent : MonoBehaviour
{
    private float interval;
    private float bonus;

    public void Initialise(float sellInterval, float sellBonus)
    {
        interval = sellInterval;
        bonus    = sellBonus;
        StartCoroutine(SellRoutine());
    }

    private IEnumerator SellRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            yield return new WaitForSeconds(interval);
            TrySell();
        }
    }

    private void TrySell()
    {
        if (GameManager.Instance?.Inventory == null) return;

        int earned = GameManager.Instance.Inventory.SellAllWithBonus(bonus);
        if (earned > 0)
        {
            int bonusPct = Mathf.RoundToInt(bonus * 100f);
            AudioManager.Instance?.PlaySell();
            HUDManager.Instance?.ShowNotification(
                $"Market Stall sold your crops! +{earned} coins (+{bonusPct}% bonus)");
        }
    }
}
