using UnityEngine;
using DG.Tweening;

public class CropGrowthVisual : MonoBehaviour
{
    private FarmTile tile;
    private int currentStage = -1;
    private GameObject currentModel;

    private static readonly float[] StageScales = { 0.7f, 0.85f, 0.95f, 1.0f };

    private static readonly System.Collections.Generic.Dictionary<string, Color> CropColours =
        new System.Collections.Generic.Dictionary<string, Color>
    {
        { "carrot",     new Color(1.0f, 0.55f, 0.1f)  },
        { "sunflower",  new Color(1.0f, 0.85f, 0.1f)  },
        { "tomato",     new Color(0.9f, 0.2f,  0.1f)  },
        { "potato",     new Color(0.7f, 0.55f, 0.2f)  },
        { "strawberry", new Color(0.9f, 0.15f, 0.25f) },
        { "corn",       new Color(1.0f, 0.9f,  0.2f)  },
        { "pumpkin",    new Color(0.9f, 0.45f, 0.05f) },
        { "grapes",     new Color(0.5f, 0.1f,  0.7f)  },
        { "chilli",     new Color(0.9f, 0.1f,  0.05f) },
        { "lavender",   new Color(0.7f, 0.5f,  0.9f)  },
    };

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

        float stageScale = StageScales[Mathf.Clamp(stage, 0, StageScales.Length - 1)];
        float baseScale  = tile.PlantedCrop != null ? tile.PlantedCrop.ModelBaseScale : 1f;
        float finalScale = stageScale * baseScale;
        Vector3 rotOffset = tile.PlantedCrop != null ? tile.PlantedCrop.ModelRotationOffset : Vector3.zero;

        var prefabs = tile.PlantedCrop?.GrowthStagePrefabs;
        if (prefabs != null && stage >= 0 && stage < prefabs.Length && prefabs[stage] != null)
        {
            currentModel = Instantiate(prefabs[stage], transform.position, Quaternion.Euler(rotOffset), transform);
            SetLayerRecursively(currentModel, LayerMask.NameToLayer("Ignore Raycast"));
        }
        else
        {
            Color cropColour = Color.green;
            if (tile.PlantedCrop != null)
                CropColours.TryGetValue(tile.PlantedCrop.CropId, out cropColour);
            currentModel = PlaceholderAssetGenerator.CreatePlaceholderCrop(stage, cropColour, transform.position);
            currentModel.transform.SetParent(transform, true);
        }

        currentModel.transform.localScale = Vector3.zero;
        currentModel.transform.DOScale(Vector3.one * finalScale, 0.35f).SetEase(Ease.OutBack);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer == -1) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public void PlayWaterBounce()
    {
        if (currentModel == null) return;
        currentModel.transform.DOKill();
        currentModel.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 4, 0.5f);
    }

    public void PopOutAndDestroy(System.Action onComplete)
    {
        if (currentModel == null) { onComplete?.Invoke(); return; }
        currentModel.transform.DOKill();
        Vector3 baseScale = currentModel.transform.localScale;
        DOTween.Sequence()
            .Append(currentModel.transform.DOScale(baseScale * 1.3f, 0.08f).SetEase(Ease.OutQuad))
            .Append(currentModel.transform.DOScale(Vector3.zero, 0.18f).SetEase(Ease.InBack))
            .OnComplete(() => onComplete?.Invoke());
    }
}