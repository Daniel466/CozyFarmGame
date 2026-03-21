using UnityEngine;

/// <summary>
/// Handles the visual representation of a crop on a tile.
/// Swaps prefabs/models as the crop passes through growth stages.
/// </summary>
public class CropGrowthVisual : MonoBehaviour
{
    private FarmTile tile;
    private int currentStage = -1;
    private GameObject currentModel;

    public void Initialise(FarmTile farmTile)
    {
        tile = farmTile;
    }

    private void Update()
    {
        if (tile == null || !tile.IsPlanted) return;

        int stage = tile.GetGrowthStage();
        if (stage != currentStage)
        {
            UpdateVisual(stage);
            currentStage = stage;
        }
    }

    private void UpdateVisual(int stage)
    {
        if (currentModel != null)
            Destroy(currentModel);

        var prefabs = tile.PlantedCrop?.GrowthStagePrefabs;
        if (prefabs == null || stage < 0 || stage >= prefabs.Length || prefabs[stage] == null)
            return;

        currentModel = Instantiate(prefabs[stage], transform.position, Quaternion.identity, transform);

        // Add a gentle bounce animation when the model changes
        StartCoroutine(BounceIn(currentModel.transform));
    }

    private System.Collections.IEnumerator BounceIn(Transform t)
    {
        Vector3 originalScale = t.localScale;
        t.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            t.localScale = originalScale * scale;
            yield return null;
        }
        t.localScale = originalScale;
    }
}
