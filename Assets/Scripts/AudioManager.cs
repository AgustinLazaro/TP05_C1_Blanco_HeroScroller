using UnityEngine;

/// <summary>
/// Gestiona todo el audio del juego:
/// - Música de fondo
/// - Efectos de sonido
/// - Control de volumen
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ========== SINGLETON ==========
    public static AudioManager Instance { get; private set; }

    // ========== COMPONENTES DE AUDIO ==========
    [Header("Fuentes de Audio")]
    [SerializeField] private AudioSource musicSource;   // Para música
    [SerializeField] private AudioSource sfxSource;     // Para efectos de sonido

    // ========== CLAVES DE PLAYERPREFS ==========
    private const string KEY_MASTER = "vol_master";
    private const string KEY_MUSIC = "vol_music";
    private const string KEY_SFX = "vol_sfx";

    // ========== PROPIEDADES DE VOLUMEN ==========

    /// <summary>
    /// Volumen maestro (afecta a todo el audio del juego)
    /// </summary>
    public float MasterVolume
    {
        get => AudioListener.volume;
        set
        {
            AudioListener.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_MASTER, AudioListener.volume);
        }
    }

    /// <summary>
    /// Volumen de la música
    /// </summary>
    public float MusicVolume
    {
        get => musicSource != null ? musicSource.volume : 1f;
        set
        {
            if (musicSource != null)
                musicSource.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_MUSIC, Mathf.Clamp01(value));
        }
    }

    /// <summary>
    /// Volumen de efectos de sonido
    /// </summary>
    public float SfxVolume
    {
        get => sfxSource != null ? sfxSource.volume : 1f;
        set
        {
            if (sfxSource != null)
                sfxSource.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_SFX, Mathf.Clamp01(value));
        }
    }

    // ========== INICIALIZACIÓN ==========
    private void Awake()
    {
        // Configurar Singleton (solo una instancia)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre escenas

        // Asegurar que tenemos las fuentes de audio
        EnsureSources();
        
        // Cargar volúmenes guardados
        LoadVolumes();
    }

    /// <summary>
    /// Crea las fuentes de audio si no existen
    /// </summary>
    private void EnsureSources()
    {
        // Crear fuente de música si no existe
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        // Crear fuente de SFX si no existe
        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFXSource");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Carga los volúmenes guardados en PlayerPrefs
    /// </summary>
    private void LoadVolumes()
    {
        MasterVolume = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, 1f);
        SfxVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);
    }

    // ========== MÉTODOS PÚBLICOS ==========

    /// <summary>
    /// Reproduce música de fondo
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null)
            return;
        
        musicSource.loop = loop;
        musicSource.clip = clip;
        musicSource.Play();
    }

    /// <summary>
    /// Detiene la música
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    /// <summary>
    /// Reproduce un efecto de sonido
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;
        
        sfxSource.PlayOneShot(clip);
    }
}