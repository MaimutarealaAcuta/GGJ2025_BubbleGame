using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages background music, sound effects, and volumes. 
/// Attach this script to a dedicated GameObject in the scene.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // For continuous background music
    [SerializeField] private AudioSource sfxSource; // For short sound effects

    [Header("Audio Clips")]
    [Tooltip("List of named sound effects and music clips.")]
    [SerializeField] private List<SoundClip> soundClips = new List<SoundClip>();

    // A dictionary for quick lookup by key name
    private Dictionary<string, AudioClip> clipsDictionary = new Dictionary<string, AudioClip>();

    [Header("Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    // Property for changing BGM volume at runtime
    public float BGMVolume
    {
        get => bgmVolume;
        set
        {
            bgmVolume = Mathf.Clamp01(value);
            if (bgmSource != null)
                bgmSource.volume = bgmVolume;
        }
    }

    // Property for changing SFX volume at runtime
    public float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
        }
    }

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: keep this manager across scene loads
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize dictionary for quick lookups
        foreach (var soundClip in soundClips)
        {
            if (!clipsDictionary.ContainsKey(soundClip.key))
            {
                clipsDictionary.Add(soundClip.key, soundClip.clip);
            }
        }

        // Ensure audio sources start with correct volumes
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    #region BGM Methods
    /// <summary>
    /// Plays a background music track by key. Loops indefinitely.
    /// </summary>
    public void PlayBGM(string bgmKey)
    {
        if (bgmSource == null) return;

        // Lookup clip in dictionary
        if (clipsDictionary.TryGetValue(bgmKey, out AudioClip clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"SoundManager: BGM key '{bgmKey}' not found.");
        }
    }

    /// <summary>
    /// Stops the current background music.
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
    #endregion

    #region SFX Methods
    /// <summary>
    /// Plays a one-shot sound effect by key.
    /// </summary>
    public void PlaySFX(string sfxKey)
    {
        if (sfxSource == null) return;

        if (clipsDictionary.TryGetValue(sfxKey, out AudioClip clip))
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: SFX key '{sfxKey}' not found.");
        }
    }

    /// <summary>
    /// Plays a one-shot sound effect by clip, if you want direct references.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
    #endregion
}

/// <summary>
/// Helper class to store a key->clip reference in the Inspector.
/// </summary>
[System.Serializable]
public class SoundClip
{
    public string key;         // e.g. "collectKey", "doorLocked", "monsterChase"
    public AudioClip clip;     // The actual audio clip
}
