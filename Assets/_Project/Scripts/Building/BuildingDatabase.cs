using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry of all BuildingData ScriptableObjects.
/// Assign all building assets in the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "BuildingDatabase", menuName = "CozyFarm/Building Database")]
public class BuildingDatabase : ScriptableObject
{
    [SerializeField] private List<BuildingData> buildings = new List<BuildingData>();

    private Dictionary<string, BuildingData> lookup;

    public void Initialise()
    {
        lookup = new Dictionary<string, BuildingData>();
        foreach (var b in buildings)
            if (b != null && !string.IsNullOrEmpty(b.BuildingId))
                lookup[b.BuildingId] = b;
    }

    public BuildingData GetById(string id)
    {
        if (lookup == null) Initialise();
        lookup.TryGetValue(id, out BuildingData b);
        return b;
    }

    public List<BuildingData> GetAll() => buildings;

    public List<BuildingData> GetUnlocked(int playerLevel)
        => buildings.FindAll(b => b.UnlockLevel <= playerLevel);
}
