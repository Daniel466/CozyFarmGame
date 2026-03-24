using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns collectible items at designated world positions.
/// Each slot respawns independently after its cooldown expires.
///
/// Setup:
///   1. Add this component to a GameObject in the farm scene.
///   2. Create empty child GameObjects named "SpawnPoint" and scatter them around the map.
///   3. Assign them to the Spawn Points array in the Inspector (or use
///      Tools > CozyFarm > Setup Collectible Spawner to auto-find children).
///
/// Drop table (weighted):
///   Coins (small)  — 60% — 5-15 coins
///   Seed           — 25% — coins equal to a random unlocked crop's seed cost
///   Lucky Find     — 15% — 25-50 coins
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    public static CollectibleSpawner Instance { get; private set; }

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Settings")]
    [SerializeField] private float respawnSeconds = 300f; // 5 minutes
    [SerializeField] private CropDatabase cropDatabase;

    // Per-slot state
    private CollectibleItem[] activeItems;
    private float[]           respawnTimers; // > 0 means on cooldown

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[CollectibleSpawner] No spawn points assigned.");
            return;
        }

        activeItems    = new CollectibleItem[spawnPoints.Length];
        respawnTimers  = new float[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
            SpawnAt(i);
    }

    private void Update()
    {
        for (int i = 0; i < respawnTimers.Length; i++)
        {
            if (respawnTimers[i] <= 0f) continue;
            respawnTimers[i] -= Time.deltaTime;
            if (respawnTimers[i] <= 0f)
                SpawnAt(i);
        }
    }

    private void SpawnAt(int slotIndex)
    {
        if (spawnPoints[slotIndex] == null) return;

        Vector3 pos = spawnPoints[slotIndex].position + Vector3.up * 0.5f;
        GameObject go = CreateCollectibleObject(pos, out CollectibleItem item, out Color color);

        // Pick drop
        var (type, coins, lbl) = RollDrop();
        item.Initialise(type, coins, lbl);
        SetCollectibleColor(go, color, type);

        int captured = slotIndex;
        item.OnCollected += () =>
        {
            activeItems[captured]   = null;
            respawnTimers[captured] = respawnSeconds;
        };

        activeItems[slotIndex] = item;
    }

    // ── Drop table ────────────────────────────────────────────────────────────

    private (CollectibleItem.CollectibleType type, int coins, string label) RollDrop()
    {
        float roll = Random.value;

        if (roll < 0.60f)
        {
            // Small coin pouch
            return (CollectibleItem.CollectibleType.Coins, Random.Range(5, 16), "");
        }
        else if (roll < 0.85f)
        {
            // Seed — grant coins equal to seed cost
            CropData crop = PickUnlockedCrop();
            if (crop != null)
                return (CollectibleItem.CollectibleType.Seed, crop.SeedCost, crop.CropName);
            // Fallback if no database / no crops
            return (CollectibleItem.CollectibleType.Coins, 10, "");
        }
        else
        {
            // Lucky find
            return (CollectibleItem.CollectibleType.LuckyFind, Random.Range(25, 51), "");
        }
    }

    private CropData PickUnlockedCrop()
    {
        if (cropDatabase == null) return null;
        int playerLevel = GameManager.Instance?.Progression?.CurrentLevel ?? 1;
        List<CropData> unlocked = new List<CropData>();
        foreach (var crop in cropDatabase.GetAllCrops())
            if (crop != null && crop.UnlockLevel <= playerLevel)
                unlocked.Add(crop);
        return unlocked.Count > 0 ? unlocked[Random.Range(0, unlocked.Count)] : null;
    }

    // ── Visual creation ───────────────────────────────────────────────────────

    private GameObject CreateCollectibleObject(Vector3 pos, out CollectibleItem item, out Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Collectible";
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.35f;

        // Remove collider — pickup is distance-based in CollectibleItem.Update
        var col = go.GetComponent<SphereCollider>();
        if (col != null) Object.Destroy(col);

        // Sparkle particles
        AddSparkleParticles(go);

        item  = go.AddComponent<CollectibleItem>();
        color = Color.white; // placeholder — set after drop roll
        return go;
    }

    private static void SetCollectibleColor(GameObject go, Color _, CollectibleItem.CollectibleType type)
    {
        Color c = type switch
        {
            CollectibleItem.CollectibleType.Seed      => new Color(0.35f, 0.85f, 0.35f),
            CollectibleItem.CollectibleType.LuckyFind => new Color(0.75f, 0.4f,  1.0f),
            _                                         => new Color(1.0f,  0.82f, 0.15f)
        };

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat    = new Material(shader);
        mat.SetColor("_BaseColor", c);
        mat.color = c;
        // Metallic/smooth for a gem-like look
        mat.SetFloat("_Metallic",   0.6f);
        mat.SetFloat("_Smoothness", 0.8f);
        go.GetComponent<Renderer>().material = mat;
    }

    private static void AddSparkleParticles(GameObject parent)
    {
        var psGO = new GameObject("Sparkle");
        psGO.transform.SetParent(parent.transform, false);
        psGO.transform.localPosition = Vector3.zero;

        var ps   = psGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop           = true;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.04f, 0.10f);
        main.startColor     = new ParticleSystem.MinMaxGradient(
                                  new Color(1f, 0.95f, 0.4f, 1f),
                                  new Color(1f, 1f,    1f,   0.6f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction      = ParticleSystemStopAction.None;

        var emission = ps.emission;
        emission.rateOverTime = 6f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.5f;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size    = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.2f, 1f), new Keyframe(1f, 0f)));

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                  ?? Shader.Find("Particles/Standard Unlit")
                  ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", new Color(1f, 0.95f, 0.5f, 1f));
        psGO.GetComponent<ParticleSystemRenderer>().material = mat;

        ps.Play();
    }
}
