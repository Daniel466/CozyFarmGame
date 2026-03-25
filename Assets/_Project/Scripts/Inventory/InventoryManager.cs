using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the player's inventory of harvested crops and purchased items.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxSlots = 20; // Starts at 20, upgradeable via Barn

    private Dictionary<string, InventoryItem> items = new Dictionary<string, InventoryItem>();

    public UnityEvent OnInventoryChanged = new UnityEvent();

    public int MaxSlots => maxSlots;
    public int UsedSlots => items.Count;
    public bool IsFull => items.Count >= maxSlots;

    public bool AddItem(CropData crop, int quantity = 1)
    {
        if (items.ContainsKey(crop.CropId))
        {
            items[crop.CropId].quantity += quantity;
        }
        else
        {
            if (IsFull)
            {
                Debug.Log("Inventory full!");
                return false;
            }
            items[crop.CropId] = new InventoryItem { crop = crop, quantity = quantity };
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(string cropId, int quantity = 1)
    {
        if (!items.ContainsKey(cropId)) return false;

        items[cropId].quantity -= quantity;
        if (items[cropId].quantity <= 0)
            items.Remove(cropId);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetQuantity(string cropId)
    {
        return items.TryGetValue(cropId, out InventoryItem item) ? item.quantity : 0;
    }

    public Dictionary<string, InventoryItem> GetAllItems() => items;

    public void ExpandSlots(int additionalSlots)
    {
        maxSlots += additionalSlots;
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Sells all of one crop type. Returns coins earned.</summary>
    public int SellItem(string cropId)
    {
        if (!items.TryGetValue(cropId, out InventoryItem item)) return 0;
        int earned = item.crop.SellValue * item.quantity;
        GameManager.Instance.Economy.AddCoins(earned);
        GameManager.Instance.Progression.AddXP(Mathf.FloorToInt(earned / 10f));
        items.Remove(cropId);
        OnInventoryChanged?.Invoke();
        return earned;
    }

    /// <summary>Sells all crops at a bonus rate. Used by Market Stall.</summary>
    public int SellAllWithBonus(float bonus)
    {
        if (items.Count == 0) return 0;
        int totalCoins = 0;
        foreach (var item in items.Values)
            totalCoins += Mathf.RoundToInt(item.crop.SellValue * item.quantity * (1f + bonus));
        int xpEarned = Mathf.FloorToInt(totalCoins / 10f);
        items.Clear();
        GameManager.Instance.Economy.AddCoins(totalCoins);
        GameManager.Instance.Progression.AddXP(xpEarned);
        OnInventoryChanged?.Invoke();
        return totalCoins;
    }

    // Sell all crops in inventory
    public int SellAll()
    {
        int totalCoins = 0;
        foreach (var item in items.Values)
            totalCoins += item.crop.SellValue * item.quantity;

        if (totalCoins > 0)
        {
            int xpEarned = Mathf.FloorToInt(totalCoins / 10f);
            GameManager.Instance.Economy.AddCoins(totalCoins);
            GameManager.Instance.Progression.AddXP(xpEarned);
        }

        items.Clear();
        OnInventoryChanged?.Invoke();
        return totalCoins;
    }
}

[System.Serializable]
public class InventoryItem
{
    public CropData crop;
    public int quantity;
}
