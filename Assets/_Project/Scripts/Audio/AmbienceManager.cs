using UnityEngine;

/// <summary>
/// Plays looping ambient nature sounds (birds, breeze, etc.)
/// Separate from music so volumes can be controlled independently.
/// </summary>
public class AmbienceManager : MonoBehaviour
{
    public static AmbienceManager Instance { get; private set; }

    [Header("Ambience Clips")]
    [SerializeField] private AudioClip[] ambienceClips; // Birds, wind, nature sounds
    [SerializeField] private float ambienceVolume = 0.25f;

    private AudioSource[] sources;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (ambienceClips == null || ambienceClips.Length == 0)
        {
            Debug.Log("[AmbienceManager] No ambience clips assigned. Add AudioClips to Ambience Clips in Inspector.");
            return;
        }

        sources = new AudioSource[ambienceClips.Length];
        for (int i = 0; i < ambienceClips.Length; i++)
        {
            if (ambienceClips[i] == null) continue;
            GameObject go = new GameObject($"Ambience_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.clip = ambienceClips[i];
            src.loop = true;
            src.volume = ambienceVolume;
            src.spatialBlend = 0f;
            src.Play();
            sources[i] = src;
        }
    }

    public void SetVolume(float volume)
    {
        ambienceVolume = Mathf.Clamp01(volume);
        if (sources == null) return;
        foreach (var src in sources)
            if (src != null) src.volume = ambienceVolume;
    }
}
