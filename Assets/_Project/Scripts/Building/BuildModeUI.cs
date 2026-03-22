using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Build mode UI panel — shows a catalogue of buildings to place.
/// Built at runtime by HUDBootstrapper. Toggle with F5.
/// </summary>
public class BuildModeUI : MonoBehaviour
{
    public static BuildModeUI Instance { get; private set; }

    private GameObject buildPanel;
    private Transform itemGrid;
    private BuildingDatabase database;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Setup(GameObject panel, Transform grid, BuildingDatabase db)
    {
        buildPanel = panel;
        itemGrid = grid;
        database = db;
        buildPanel.SetActive(false);
    }

    public void ToggleBuildPanel()
    {
        isOpen = !isOpen;
        buildPanel?.SetActive(isOpen);
        if (isOpen)
        {
            buildPanel?.transform.SetAsLastSibling();
            RefreshCatalogue();
        }
    }

    public void CloseBuildPanel()
    {
        isOpen = false;
        buildPanel?.SetActive(false);
    }

    private void RefreshCatalogue()
    {
        if (itemGrid == null || database == null) return;

        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        int playerLevel = GameManager.Instance.Progression.CurrentLevel;
        List<BuildingData> all = database.GetAll();

        foreach (var building in all)
        {
            bool unlocked = building.UnlockLevel <= playerLevel;

            // Create item button
            GameObject btn = new GameObject($"Btn_{building.BuildingId}");
            btn.transform.SetParent(itemGrid, false);

            var img = btn.AddComponent<Image>();
            img.color = unlocked ? new Color(0.25f, 0.2f, 0.13f) : new Color(0.15f, 0.13f, 0.1f);

            var rect = btn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 120f);

            var vlg = btn.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 4f;
            vlg.childForceExpandWidth = true;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            // Colour swatch
            GameObject swatch = new GameObject("Swatch");
            swatch.transform.SetParent(btn.transform, false);
            var swatchImg = swatch.AddComponent<Image>();
            swatchImg.color = unlocked ? building.PlaceholderColor : new Color(0.3f, 0.3f, 0.3f);
            var swatchRect = swatch.GetComponent<RectTransform>();
            swatchRect.sizeDelta = new Vector2(60f, 50f);
            var swatchLE = swatch.AddComponent<LayoutElement>();
            swatchLE.preferredHeight = 50f;

            // Name text
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(btn.transform, false);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text = building.BuildingName;
            nameTMP.fontSize = 14f;
            nameTMP.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.fontStyle = FontStyles.Bold;
            var nameLE = nameGO.AddComponent<LayoutElement>();
            nameLE.preferredHeight = 20f;

            // Cost / lock text
            GameObject costGO = new GameObject("Cost");
            costGO.transform.SetParent(btn.transform, false);
            var costTMP = costGO.AddComponent<TextMeshProUGUI>();
            costTMP.text = unlocked ? $"{building.Cost} coins" : $"Lv.{building.UnlockLevel}";
            costTMP.fontSize = 13f;
            costTMP.color = unlocked ? new Color(1f, 0.85f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
            costTMP.alignment = TextAlignmentOptions.Center;
            var costLE = costGO.AddComponent<LayoutElement>();
            costLE.preferredHeight = 18f;

            // Button click
            var button = btn.AddComponent<Button>();
            button.targetGraphic = img;
            button.interactable = unlocked;

            if (unlocked)
            {
                BuildingData buildingRef = building;
                button.onClick.AddListener(() =>
                {
                    BuildingManager.Instance.EnterBuildMode(buildingRef);
                    CloseBuildPanel();
                });
            }

            // Dim locked items
            var cg = btn.AddComponent<CanvasGroup>();
            cg.alpha = unlocked ? 1f : 0.5f;
        }
    }
}
