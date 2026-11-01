using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.4f;

    [Header("Sound Effects")]
    public AudioClip wallHitClip;
    public AudioClip winClip;
    public AudioClip playerDeathClip;
    public AudioClip pickupClip;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null) musicSource.volume = volume;
    }

    public void SetMusicEnabled(bool enabled)
    {
        if (musicSource != null) musicSource.mute = !enabled;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayWallHit()     => PlaySFX(wallHitClip);
    public void PlayWin()         => PlaySFX(winClip);
    public void PlayPlayerDeath() => PlaySFX(playerDeathClip);
    public void PlayPickup()      => PlaySFX(pickupClip);
}
