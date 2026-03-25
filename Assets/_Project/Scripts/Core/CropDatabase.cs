using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry of all CropData ScriptableObjects.
/// Allows looking up crops by their ID string (used by the save system).
/// Assign all crop assets in the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "CropDatabase", menuName = "CozyFarm/Crop Database")]
public class CropDatabase : ScriptableObject
{
    [SerializeField] private List<CropData> crops = new List<CropData>();

    private Dictionary<string, CropData> lookup;

    public void Initialise()
    {
        lookup = new Dictionary<string, CropData>();
        foreach (var crop in crops)
        {
            if (crop != null && !string.IsNullOrEmpty(crop.CropId))
                lookup[crop.CropId] = crop;
        }
    }

    public CropData GetCropById(string id)
    {
        if (lookup == null) Initialise();
        lookup.TryGetValue(id, out CropData crop);
        return crop;
    }

    public List<CropData> GetAllCrops() => crops;
}
