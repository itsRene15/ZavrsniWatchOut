using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private bool playOnStart = true;

    [Header("Source")]
    [SerializeField] private AudioSource musicSource;

    // Set up singleton and source
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        ConfigureSource(musicSource);

        if (playOnStart && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // Configure one audio source
    private void ConfigureSource(AudioSource source)
    {
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 1f;
    }

    // Ensure music is playing (keeps the same song)
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null) return;
        if (musicSource.isPlaying) return;

        AudioClip toPlay = musicClip != null ? musicClip : clip;
        if (toPlay == null) return;
        musicSource.clip = toPlay;
        musicSource.Play();
    }

    // Stop current music
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // Play a one-shot sound effect
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        GameObject temp = new GameObject("TempSFX");
        temp.transform.position = Camera.main.transform.position; 
        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume;
        src.Play();
        Destroy(temp, clip.length + 0.1f);
    }
}