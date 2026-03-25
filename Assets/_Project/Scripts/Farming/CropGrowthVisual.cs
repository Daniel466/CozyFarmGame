using UnityEngine;
using DG.Tweening;

public class CropGrowthVisual : MonoBehaviour
{
    private FarmTile tile;
    private int currentStage = -1;
    private GameObject currentModel;

    private GameObject readyFXPrefab;
    private GameObject readyFXInstance;
    private bool wasReady = false;

    [SerializeField] private float readyFXYOffset = 0.7f;

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
        { "watermelon", new Color(0.25f, 0.75f, 0.2f)  },
        { "leek",       new Color(0.4f,  0.75f, 0.3f)  },
        { "wheat",      new Color(0.85f, 0.72f, 0.15f) },
    };

    public void Initialise(FarmTile farmTile, GameObject readyFX = null)
    {
        tile          = farmTile;
        readyFXPrefab = readyFX;
        // Run immediately so the correct stage shows on spawn
        Refresh(farmTile);
    }

    /// <summary>Called by RealTimeManager every second to update stage and ready FX.</summary>
    public void Refresh(FarmTile farmTile)
    {
        tile = farmTile;
        if (tile == null || !tile.IsPlanted) return;

        int stage = tile.GetGrowthStage();
        if (stage != currentStage)
        {
            UpdateVisual(stage);
            currentStage = stage;
        }

        UpdateReadyFX();
    }

    private void UpdateReadyFX()
    {
        bool isReady = tile.IsReadyToHarvest;
        if (isReady == wasReady) return;
        wasReady = isReady;

        if (isReady)
        {
            if (readyFXPrefab != null && readyFXInstance == null)
            {
                readyFXInstance = Instantiate(readyFXPrefab,
                    transform.position + Vector3.up * readyFXYOffset,
                    Quaternion.identity,
                    transform);
            }
        }
        else
        {
            if (readyFXInstance != null)
            {
                Destroy(readyFXInstance);
                readyFXInstance = null;
            }
        }
    }

    private void UpdateVisual(int stage)
    {
        if (currentModel != null)
        {
            currentModel.transform.DOKill(); // kill active tweens before destroying
            Destroy(currentModel);
        }

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