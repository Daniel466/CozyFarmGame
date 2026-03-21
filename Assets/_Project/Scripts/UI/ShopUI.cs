using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shop UI for buying seeds. Toggle with B key.
/// Shows all crops unlocked at the player's current level.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private GameObject shopItemPrefab;

    [Header("Database")]
    [SerializeField] private CropDatabase cropDatabase;

    private bool isOpen = false;

    private void Start()
    {
        shopPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            ToggleShop();
    }

    public void ToggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        if (isOpen) RefreshShop();
    }

    private void RefreshShop()
    {
        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        int playerLevel = GameManager.Instance.Progression.CurrentLevel;
        List<CropData> allCrops = cropDatabase.GetAllCrops();

        foreach (var crop in allCrops)
        {
            if (shopItemPrefab == null) continue;

            GameObject slot = Instantiate(shopItemPrefab, itemGrid);
            bool unlocked = crop.UnlockLevel <= playerLevel;

            // Icon
            var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (icon && crop.Icon) icon.sprite = crop.Icon;

            // Name
            var nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText) nameText.text = crop.CropName;

            // Cost
            var costText = slot.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
            if (costText) costText.text = unlocked ? $"{crop.SeedCost} 🪙" : $"Lv.{crop.UnlockLevel}";

            // Grow time
            var timeText = slot.transform.Find("GrowTime")?.GetComponent<TextMeshProUGUI>();
            if (timeText) timeText.text = unlocked ? $"{crop.GrowTimeSeconds / 60f:0.0} min" : "???";

            // Buy button
            var buyButton = slot.transform.Find("BuyButton")?.GetComponent<Button>();
            if (buyButton)
            {
                buyButton.interactable = unlocked;
                CropData cropRef = crop; // capture for lambda
                buyButton.onClick.AddListener(() => BuySeed(cropRef));
            }

            // Dim locked items
            var canvasGroup = slot.GetComponent<CanvasGroup>();
            if (canvasGroup) canvasGroup.alpha = unlocked ? 1f : 0.5f;
        }
    }

    private void BuySeed(CropData crop)
    {
        // Set as selected crop on PlayerInteraction
        var interaction = FindObjectOfType<PlayerInteraction>();
        if (interaction != null)
        {
            interaction.SetSelectedCrop(crop);
            HUDManager.Instance?.ShowNotification($"{crop.CropName} seed selected! Left-click a tilled tile to plant.");
            HUDManager.Instance?.UpdateToolIndicator($"🌱 {crop.CropName}");
        }

        // Close shop
        ToggleShop();
    }
}
