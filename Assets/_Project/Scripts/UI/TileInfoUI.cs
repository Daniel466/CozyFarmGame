using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows a small tooltip panel when the player hovers over a farm tile.
/// Displays: crop name, growth stage, watered status, time remaining.
/// </summary>
public class TileInfoUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Slider growthSlider;

    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float hoverUpdateRate = 0.1f;

    private Camera mainCamera;
    private FarmGrid grid;
    private float nextUpdateTime;

    private readonly string[] stageNames = { "Planted 🌱", "Sprouting 🌿", "Growing 🌾", "Ready to Harvest! ✨" };

    private void Start()
    {
        mainCamera = Camera.main;
        grid = GameManager.Instance.FarmGrid;
        panel.SetActive(false);
    }

    private void Update()
    {
        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + hoverUpdateRate;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            FarmTile tile = grid.GetTileAtWorldPos(hit.point);
            if (tile != null && tile.IsPlanted)
            {
                ShowTileInfo(tile);
                return;
            }
        }

        panel.SetActive(false);
    }

    private void ShowTileInfo(FarmTile tile)
    {
        panel.SetActive(true);

        if (titleText) titleText.text = tile.PlantedCrop.CropName;

        int stage = tile.GetGrowthStage();
        string stageName = stage >= 0 && stage < stageNames.Length ? stageNames[stage] : "";
        string wateredStr = tile.IsWatered ? "💧 Watered" : "🏜️ Needs water";

        if (statusText) statusText.text = $"{stageName}\n{wateredStr}";
        if (growthSlider) growthSlider.value = tile.GrowthProgress;
    }
}
