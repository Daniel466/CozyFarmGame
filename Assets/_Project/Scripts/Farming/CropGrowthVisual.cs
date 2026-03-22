using UnityEngine;

/// <summary>
/// Handles the visual representation of a crop on a tile.
/// Uses assigned prefabs if available, otherwise falls back to placeholder primitives.
/// </summary>
public class CropGrowthVisual : MonoBehaviour
{
    private FarmTile tile;
    private int currentStage = -1;
    private GameObject currentModel;

    // Scale per growth stage — starts small, grows to full size
    // Adjust these to match your tile size and polyperfect model sizes
    private static readonly float[] StageScales = { 0.5f, 0.8f, 1.2f, 1.8f };


    // Placeholder colours per crop type
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

        float scale = StageScales[Mathf.Clamp(stage, 0, StageScales.Length - 1)];

        // Try assigned prefabs first
        var prefabs = tile.PlantedCrop?.GrowthStagePrefabs;
        if (prefabs != null && stage >= 0 && stage < prefabs.Length && prefabs[stage] != null)
        {
            currentModel = Instantiate(prefabs[stage], transform.position, Quaternion.identity, transform);
            currentModel.transform.localScale = Vector3.one * scale;

            // Make sure all child renderers ignore raycasts
            SetLayerRecursively(currentModel, LayerMask.NameToLayer("Ignore Raycast"));
        }
        else
        {
            // Fall back to placeholder primitives
            Color cropColour = Color.green;
            if (tile.PlantedCrop != null)
                CropColours.TryGetValue(tile.PlantedCrop.CropId, out cropColour);

            currentModel = PlaceholderAssetGenerator.CreatePlaceholderCrop(stage, cropColour, transform.position);
            currentModel.transform.SetParent(transform, true);
        }

        // Add a gentle bounce animation when the model changes
        StartCoroutine(BounceIn(currentModel.transform));
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer == -1) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
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