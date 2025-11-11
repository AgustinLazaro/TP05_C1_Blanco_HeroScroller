using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona todos los menús y la interfaz del juego:
/// - Menú principal
/// - Pausa
/// - Opciones
/// </summary>
public class UIManager : MonoBehaviour
{
    // ========== CONFIGURACIÓN DE ESCENAS ==========
    [Header("Nombres de Escenas")]
    [SerializeField] private string gameSceneName = "SceneGame";    // Escena del juego
    [SerializeField] private string menuSceneName = "SceneMenu";    // Escena del menú

    // ========== AUDIO ==========
    [Header("Música")]
    [SerializeField] private AudioClip menuMusic; // Música del menú

    // ========== PANELES ==========
    [Header("Paneles del Menú")]
    [SerializeField] private GameObject panelTitle;     // Panel principal
    [SerializeField] private GameObject panelOptions;   // Panel de opciones
    [SerializeField] private GameObject panelCredits;   // Panel de créditos

    [Header("Botones del Menú")]
    [SerializeField] private Button buttonStart;        // Botón "Jugar"
    [SerializeField] private Button buttonOptions;      // Botón "Opciones"
    [SerializeField] private Button buttonCredits;      // Botón "Créditos"

    [Header("Botones de Retroceso")]
    [SerializeField] private Button buttonBackOptions;  // Volver desde opciones
    [SerializeField] private Button buttonBackCredits;  // Volver desde créditos

    // ========== OPCIONES DE AUDIO ==========
    [Header("Controles de Volumen")]
    [SerializeField] private Slider sliderMaster;   // Volumen principal
    [SerializeField] private Slider sliderMusic;    // Volumen de música
    [SerializeField] private Slider sliderSFX;      // Volumen de efectos

    // ========== PAUSA ==========
    [Header("Sistema de Pausa")]
    [SerializeField] private GameObject panelPause;         // Panel de pausa
    [SerializeField] private Button buttonResume;           // Botón "Continuar"
    [SerializeField] private Button buttonPauseOptions;     // Botón "Opciones" en pausa
    [SerializeField] private KeyCode pauseKey = KeyCode.P;  // Tecla de pausa

    // ========== CLAVES DE PLAYERPREFS ==========
    private const string KEY_VOL_MASTER = "vol_master";
    private const string KEY_VOL_MUSIC = "vol_music";
    private const string KEY_VOL_SFX = "vol_sfx";

    // ========== ESTADO ==========
    private bool isPaused;                          // ¿El juego está pausado?
    private bool isMenuScene;                       // ¿Estamos en el menú?
    private GameObject returnPanelFromOptions;      // Panel al que volver desde opciones

    // Consulta si el juego terminó
    private bool IsGameOver => GameManager.Instance != null && GameManager.Instance.IsGameOver;

    // ========== INICIALIZACIÓN ==========
    private void Awake()
    {
        // Buscar referencias automáticamente
        AutoWireInChildren();
        
        // Determinar si estamos en el menú
        isMenuScene = SceneManager.GetActiveScene().name == menuSceneName;

        // Conectar botones con sus funciones
        WireMenuButtons();
        WirePauseButtons();
        WireBackButtons();
        WireSliders();

        // Configuración inicial del menú
        if (isMenuScene)
        {
            ShowPanel(panelTitle);
            
            // Reproducir música del menú
            if (menuMusic != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(menuMusic, loop: true);
            
            // Ocultar panel de pausa en el menú
            if (panelPause != null)
                panelPause.SetActive(false);
            
            Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        }

        // Sincronizar sliders con los valores guardados
        SyncSlidersFromAudio();
    }

    // ========== UPDATE ==========
    private void Update()
    {
        // Solo manejar pausa en el juego (no en el menú)
        if (isMenuScene)
            return;

        // No permitir pausar si el juego terminó
        if (IsGameOver)
            return;

        // Detectar tecla de pausa (P o ESC)
        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelPause == null)
                return;

            if (!isPaused)
            {
                // Pausar el juego
                Pause();
            }
            else
            {
                // Si está en opciones, volver al panel de pausa
                if (panelOptions != null && panelOptions.activeSelf && returnPanelFromOptions == panelPause)
                    ShowPanel(panelPause);
                else
                    Resume(); // Reanudar el juego
            }
        }
    }

    private void OnDisable()
    {
        // Evitar reanudar el tiempo si el juego terminó
        if (isPaused)
        {
            if (!IsGameOver)
                Time.timeScale = 1f;
            isPaused = false;
        }
    }

    // ========== ACCIONES DEL MENÚ ==========

    /// <summary>
    /// Botón "Jugar" - Carga la escena del juego
    /// </summary>
    public void OnPlay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Botón "Opciones" - Muestra el panel de opciones
    /// </summary>
    public void OnOptions()
    {
        if (panelOptions == null)
            return;
        
        SyncSlidersFromAudio();
        ShowPanel(panelOptions);
    }

    /// <summary>
    /// Botón "Créditos" - Muestra el panel de créditos
    /// </summary>
    public void OnCredits()
    {
        if (panelCredits == null)
            return;
        
        ShowPanel(panelCredits);
    }

    /// <summary>
    /// Botón "Opciones" desde la pausa
    /// </summary>
    public void OnOptionsFromPause()
    {
        if (panelOptions == null)
            return;
        
        SyncSlidersFromAudio();
        returnPanelFromOptions = panelPause;
        ShowPanel(panelOptions);
    }

    /// <summary>
    /// Volver desde opciones
    /// </summary>
    public void OnBackFromOptions()
    {
        if (returnPanelFromOptions == panelPause && panelPause != null)
            ShowPanel(panelPause);
        else
            ShowPanel(panelTitle);
    }

    /// <summary>
    /// Volver al menú principal
    /// </summary>
    public void OnBackToTitle()
    {
        ShowPanel(panelTitle);
    }

    // ========== SISTEMA DE PAUSA ==========

    /// <summary>
    /// Pausa el juego
    /// </summary>
    public void Pause()
    {
        if (panelPause == null || IsGameOver)
            return;
        
        isPaused = true;
        Time.timeScale = 0f;
        returnPanelFromOptions = panelPause;
        ShowPanel(panelPause);
    }

    /// <summary>
    /// Reanuda el juego
    /// </summary>
    public void Resume()
    {
        if (IsGameOver)
            return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        if (panelPause != null)
            panelPause.SetActive(false);
        
        if (panelOptions != null && returnPanelFromOptions == panelPause)
            panelOptions.SetActive(false);
        
        returnPanelFromOptions = null;
    }

    // ========== CONTROL DE VOLUMEN ==========

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.MasterVolume = value;
        PlayerPrefs.SetFloat(KEY_VOL_MASTER, value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.MusicVolume = value;
        PlayerPrefs.SetFloat(KEY_VOL_MUSIC, value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SfxVolume = value;
        PlayerPrefs.SetFloat(KEY_VOL_SFX, value);
    }

    /// <summary>
    /// Sincroniza los sliders con los valores guardados
    /// </summary>
    private void SyncSlidersFromAudio()
    {
        float master = PlayerPrefs.GetFloat(KEY_VOL_MASTER, 1f);
        float music = PlayerPrefs.GetFloat(KEY_VOL_MUSIC, 1f);
        float sfx = PlayerPrefs.GetFloat(KEY_VOL_SFX, 1f);

        if (AudioManager.Instance != null)
        {
            master = AudioManager.Instance.MasterVolume;
            music = AudioManager.Instance.MusicVolume;
            sfx = AudioManager.Instance.SfxVolume;
        }

        if (sliderMaster != null)
            sliderMaster.SetValueWithoutNotify(master);
        
        if (sliderMusic != null)
            sliderMusic.SetValueWithoutNotify(music);
        
        if (sliderSFX != null)
            sliderSFX.SetValueWithoutNotify(sfx);
    }

    // ========== UTILIDADES ==========

    /// <summary>
    /// Muestra un panel y oculta los demás
    /// </summary>
    private void ShowPanel(GameObject panelToShow)
    {
        if (panelTitle != null)
            panelTitle.SetActive(panelToShow == panelTitle);
        
        if (panelOptions != null)
            panelOptions.SetActive(panelToShow == panelOptions);
        
        if (panelCredits != null)
            panelCredits.SetActive(panelToShow == panelCredits);
        
        if (panelPause != null)
            panelPause.SetActive(panelToShow == panelPause);
    }

    /// <summary>
    /// Conecta los botones del menú con sus funciones
    /// </summary>
    private void WireMenuButtons()
    {
        if (buttonStart != null)
            buttonStart.onClick.AddListener(OnPlay);
        
        if (buttonOptions != null)
            buttonOptions.onClick.AddListener(OnOptions);
        
        if (buttonCredits != null)
            buttonCredits.onClick.AddListener(OnCredits);
    }

    /// <summary>
    /// Conecta los botones de pausa
    /// </summary>
    private void WirePauseButtons()
    {
        if (buttonResume != null)
            buttonResume.onClick.AddListener(Resume);
        
        if (buttonPauseOptions != null)
            buttonPauseOptions.onClick.AddListener(OnOptionsFromPause);
    }

    /// <summary>
    /// Conecta los botones de retroceso
    /// </summary>
    private void WireBackButtons()
    {
        if (buttonBackOptions != null)
            buttonBackOptions.onClick.AddListener(OnBackFromOptions);
        
        if (buttonBackCredits != null)
            buttonBackCredits.onClick.AddListener(OnBackToTitle);
    }

    /// <summary>
    /// Conecta los sliders con sus funciones
    /// </summary>
    private void WireSliders()
    {
        if (sliderMaster != null)
            sliderMaster.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        if (sliderMusic != null)
            sliderMusic.onValueChanged.AddListener(OnMusicVolumeChanged);
        
        if (sliderSFX != null)
            sliderSFX.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    /// <summary>
    /// Busca automáticamente referencias en la jerarquía
    /// </summary>
    private void AutoWireInChildren()
    {
        if (panelTitle == null)
            panelTitle = FindChildGO("PanelTitle");
        
        if (panelOptions == null)
            panelOptions = FindChildGO("PanelOptions");
        
        if (panelCredits == null)
            panelCredits = FindChildGO("PanelCredits");
        
        if (panelPause == null)
            panelPause = FindChildGO("PanelPause");

        if (buttonStart == null)
            buttonStart = FindChild<Button>("ButtonStart");
        
        if (buttonOptions == null)
            buttonOptions = FindChild<Button>("ButtonOptions");
        
        if (buttonCredits == null)
            buttonCredits = FindChild<Button>("ButtonCredits");

        if (buttonBackOptions == null)
        {
            buttonBackOptions = FindChild<Button>("ButtonBackOptions");
            if (buttonBackOptions == null && panelOptions != null)
                buttonBackOptions = FindIn<Button>(panelOptions, "ButtonBack");
        }

        if (buttonBackCredits == null)
        {
            buttonBackCredits = FindChild<Button>("ButtonBackCredits");
            if (buttonBackCredits == null && panelCredits != null)
                buttonBackCredits = FindIn<Button>(panelCredits, "ButtonBack");
        }

        if (buttonResume == null)
            buttonResume = FindChild<Button>("ButtonResume");
        
        if (buttonPauseOptions == null)
            buttonPauseOptions = FindChild<Button>("ButtonPauseOptions");

        if (sliderMaster == null)
            sliderMaster = FindChild<Slider>("SliderMaster");
        
        if (sliderMusic == null)
            sliderMusic = FindChild<Slider>("SliderMusic");
        
        if (sliderSFX == null)
            sliderSFX = FindChild<Slider>("SliderSFX");
    }

    private GameObject FindChildGO(string name)
    {
        var trs = GetComponentsInChildren<Transform>(true);
        foreach (var t in trs) if (t.name == name) return t.gameObject;
        return null;
    }

    private T FindChild<T>(string name) where T : Component
    {
        var comps = GetComponentsInChildren<T>(true);
        foreach (var c in comps) if (c.name == name) return c;
        return null;
    }

    private T FindIn<T>(GameObject root, string name) where T : Component
    {
        if (root == null) return null;
        var comps = root.GetComponentsInChildren<T>(true);
        foreach (var c in comps) if (c.name == name) return c;
        return null;
    }
}