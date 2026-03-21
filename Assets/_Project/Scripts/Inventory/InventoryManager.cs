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

    // Sell all crops in inventory
    public int SellAll()
    {
        int totalCoins = 0;
        foreach (var item in items.Values)
        {
            totalCoins += item.crop.SellValue * item.quantity;
            int xpEarned = Mathf.FloorToInt(totalCoins / 10f);
            GameManager.Instance.Progression.AddXP(xpEarned);
        }
        items.Clear();
        GameManager.Instance.Economy.AddCoins(totalCoins);
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
