using UnityEngine;

/// <summary>
/// Singleton that owns the dog companion lifecycle.
/// BuildingManager calls SpawnDog / DespawnDog when the doghouse is placed or removed.
/// The dog prefab (ShibaInu_Dog) is assigned in the Inspector on the GameManager GameObject
/// or any persistent scene object.
/// </summary>
public class DogManager : MonoBehaviour
{
    public static DogManager Instance { get; private set; }

    [Header("Dog Prefab")]
    [Tooltip("Assign the ShibaInu_Dog prefab from Assets/_Project/Prefabs/Animals/.")]
    [SerializeField] private GameObject dogPrefab;

    [Tooltip("Offset from the doghouse position where the dog spawns.")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(1.5f, 0f, 0f);

    /// <summary>Currently live dog instance. Null when no doghouse is placed.</summary>
    public DogController ActiveDog { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Instantiates the dog next to the doghouse world position.
    /// No-op if a dog already exists (only one dog at a time).
    /// </summary>
    /// <param name="doghouseWorldPos">World position of the placed doghouse building.</param>
    public void SpawnDog(Vector3 doghouseWorldPos)
    {
        if (ActiveDog != null)
        {
            Debug.Log("[DogManager] Dog already exists — skipping spawn.");
            return;
        }

        if (dogPrefab == null)
        {
            Debug.LogError("[DogManager] dogPrefab is not assigned. Assign ShibaInu_Dog in the Inspector.");
            return;
        }

        Vector3 desired  = doghouseWorldPos + spawnOffset;
        Vector3 spawnPos = desired;
        if (UnityEngine.AI.NavMesh.SamplePosition(desired, out UnityEngine.AI.NavMeshHit hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
            spawnPos = hit.position;
        else
            Debug.LogWarning("[DogManager] Spawn position not on NavMesh — placing at doghouse origin.");

        var go = Instantiate(dogPrefab, spawnPos, Quaternion.identity);
        go.name = "ShibaInu_Dog";

        ActiveDog = go.GetComponent<DogController>();
        if (ActiveDog == null)
        {
            Debug.LogError("[DogManager] Spawned dog prefab has no DogController component.");
            return;
        }

        ActiveDog.SetHome(doghouseWorldPos);
        DogHappinessHUD.Instance?.SetDogPanelVisible(true);
        Debug.Log($"[DogManager] Dog spawned at {spawnPos}.");
    }

    /// <summary>
    /// Destroys the active dog instance.
    /// Called when the doghouse building is removed.
    /// </summary>
    public void DespawnDog()
    {
        if (ActiveDog == null) return;

        Destroy(ActiveDog.gameObject);
        ActiveDog = null;
        DogHappinessHUD.Instance?.SetDogPanelVisible(false);
        Debug.Log("[DogManager] Dog despawned.");
    }

    /// <summary>True when a dog is currently alive in the scene.</summary>
    public bool HasDog => ActiveDog != null;
}
