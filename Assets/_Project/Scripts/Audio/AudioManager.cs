using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Central audio manager for music and SFX.
/// Attach to a persistent GameObject in the scene.
/// Supports: background music looping, SFX one-shots, volume control.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip[] musicTracks; // Drag in lo-fi/cozy tracks
    [SerializeField] private float musicVolume = 0.4f;

    [Header("SFX")]
    [SerializeField] private AudioClip[] tillSFXClips;    // Randomised per dig action
    [SerializeField] private AudioClip[] plantSFXClips;   // Randomised per plant action
    [SerializeField] private AudioClip[] waterSFXClips;   // Randomised per water action
    [SerializeField] private AudioClip[] harvestSFXClips; // Randomised per harvest action
    [SerializeField] private AudioClip sellSFX;
    [SerializeField] private AudioClip buildPlaceSFX;
    [SerializeField] private AudioClip buildRemoveSFX;
    [SerializeField] private AudioClip levelUpSFX;
    [SerializeField] private AudioClip uiClickSFX;
    [SerializeField] private AudioClip collectSFX;
    [SerializeField] private AudioClip[] dogBarkClips;    // ANIMAL_Dog_Bark_03 RR1-4
    [SerializeField] private AudioClip petSFX;            // Optional soft pet sound
    [SerializeField] private float sfxVolume = 0.8f;

    private int currentTrackIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        // Create audio sources if not assigned
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("MusicSource");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
            musicSource.spatialBlend = 0f; // 2D
        }

        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
            sfxSource.spatialBlend = 0f; // 2D
        }
    }

    private void Start()
    {
        PlayMusic();
    }

    // --- Music ---

    public void PlayMusic()
    {
        if (musicTracks == null || musicTracks.Length == 0)
        {
            Debug.Log("[AudioManager] No music tracks assigned. Add AudioClips to Music Tracks in Inspector.");
            return;
        }

        var track = musicTracks[currentTrackIndex % musicTracks.Length];
        if (track == null) return;

        musicSource.clip = track;
        musicSource.Play();
    }

    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % Mathf.Max(1, musicTracks.Length);
        PlayMusic();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource) musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource) sfxSource.volume = sfxVolume;
    }

    public void ToggleMusic()
    {
        if (musicSource.isPlaying) musicSource.Pause();
        else musicSource.Play();
    }

    // --- SFX ---

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // Convenience methods for each farming action
    public void PlayTill()     => PlaySFX(RandomClip(tillSFXClips));
    public void PlayPlant()    => PlaySFX(RandomClip(plantSFXClips));
    public void PlayWater()    => PlaySFX(RandomClip(waterSFXClips));
    public void PlayHarvest()  => PlaySFX(RandomClip(harvestSFXClips));
    public void PlaySell()     => PlaySFX(sellSFX);
    public void PlayBuild()    => PlaySFX(buildPlaceSFX);
    public void PlayRemove()   => PlaySFX(buildRemoveSFX);
    public void PlayLevelUp()  => PlaySFX(levelUpSFX, 1.2f);
    public void PlayUIClick()  => PlaySFX(uiClickSFX, 0.6f);
    public void PlayCollect()  => PlaySFX(collectSFX ?? sellSFX, 1.1f);
    public void PlayDogBark()  => PlaySFX(RandomClip(dogBarkClips), 0.9f);
    public void PlayPet()      => PlaySFX(petSFX, 0.7f);

    private AudioClip RandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
