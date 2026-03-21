using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's inventory in a scrollable grid panel.
/// Toggle with Tab key. Includes a Sell All button.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Sell")]
    [SerializeField] private Button sellAllButton;
    [SerializeField] private TextMeshProUGUI sellAllText;

    [Header("Slots Info")]
    [SerializeField] private TextMeshProUGUI slotsText;

    private bool isOpen = false;

    private void Start()
    {
        inventoryPanel.SetActive(false);
        sellAllButton?.onClick.AddListener(SellAll);
        GameManager.Instance.Inventory.OnInventoryChanged.AddListener(RefreshUI);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        if (isOpen) RefreshUI();
    }

    private void RefreshUI()
    {
        if (!isOpen) return;

        // Clear existing items
        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        var items = GameManager.Instance.Inventory.GetAllItems();
        int totalValue = 0;

        foreach (var kvp in items)
        {
            var item = kvp.Value;
            totalValue += item.crop.SellValue * item.quantity;

            if (inventoryItemPrefab != null)
            {
                GameObject slot = Instantiate(inventoryItemPrefab, itemGrid);
                // Set icon
                var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
                if (icon && item.crop.Icon) icon.sprite = item.crop.Icon;

                // Set name
                var nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText) nameText.text = item.crop.CropName;

                // Set quantity
                var qtyText = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
                if (qtyText) qtyText.text = $"x{item.quantity}";

                // Set value
                var valText = slot.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
                if (valText) valText.text = $"{item.crop.SellValue * item.quantity}🪙";
            }
        }

        // Update sell button text
        if (sellAllText) sellAllText.text = $"Sell All ({totalValue}🪙)";

        // Update slots text
        var inv = GameManager.Instance.Inventory;
        if (slotsText) slotsText.text = $"Slots: {inv.UsedSlots}/{inv.MaxSlots}";
    }

    private void SellAll()
    {
        int earned = GameManager.Instance.Inventory.SellAll();
        HUDManager.Instance?.ShowNotification($"Sold everything for {earned} 🪙!");
        RefreshUI();
    }
}
