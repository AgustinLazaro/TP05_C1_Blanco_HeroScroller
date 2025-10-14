using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Fuentes")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private const string KEY_MASTER = "vol_master";
    private const string KEY_MUSIC = "vol_music";
    private const string KEY_SFX = "vol_sfx";

    public float MasterVolume
    {
        get => AudioListener.volume;
        set
        {
            AudioListener.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_MASTER, AudioListener.volume);
        }
    }

    public float MusicVolume
    {
        get => musicSource != null ? musicSource.volume : 1f;
        set
        {
            if (musicSource != null) musicSource.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_MUSIC, Mathf.Clamp01(value));
        }
    }

    public float SfxVolume
    {
        get => sfxSource != null ? sfxSource.volume : 1f;
        set
        {
            if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_SFX, Mathf.Clamp01(value));
        }
    }

    private void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        LoadVolumes();
    }

    private void EnsureSources()
    {
        // Crea fuentes si faltan para no depender del editor
        if (musicSource == null)
        {
            var go = new GameObject("MusicSource");
            go.transform.SetParent(transform);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            var go = new GameObject("SFXSource");
            go.transform.SetParent(transform);
            sfxSource = go.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    private void LoadVolumes()
    {
        MasterVolume = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, 1f);
        SfxVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);
    }

    // API sencilla
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        musicSource.loop = loop;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // Compatibilidad con código existente
    public void PlaySound(AudioClip clip) => PlaySFX(clip);
}