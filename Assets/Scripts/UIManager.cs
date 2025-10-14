using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string gameSceneName = "SceneGame";
    [SerializeField] private string menuSceneName = "SceneMenu";

    [Header("Audio")]
    [SerializeField] private AudioClip menuMusic;

    [Header("Paneles (menú)")]
    [SerializeField] private GameObject panelTitle;
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private GameObject panelCredits;

    [Header("Botones (menú)")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonOptions;
    [SerializeField] private Button buttonCredits;

    [Header("Botones Back")]
    [SerializeField] private Button buttonBackOptions;  // ButtonBackOptions o "ButtonBack" dentro de PanelOptions
    [SerializeField] private Button buttonBackCredits;  // ButtonBackCredits o "ButtonBack" dentro de PanelCredits

    [Header("Opciones")]
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Slider sliderSFX;

    [Header("Pausa (juego)")]
    [SerializeField] private GameObject panelPause;
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonPauseOptions;
    [SerializeField] private KeyCode pauseKey = KeyCode.P;

    private const string KEY_VOL_MASTER = "vol_master";
    private const string KEY_VOL_MUSIC = "vol_music";
    private const string KEY_VOL_SFX = "vol_sfx";

    private bool isPaused;
    private bool isMenuScene;
    private GameObject returnPanelFromOptions;

    private void Awake()
    {
        AutoWireInChildren();
        isMenuScene = SceneManager.GetActiveScene().name == menuSceneName;

        WireMenuButtons();
        WirePauseButtons();
        WireBackButtons();
        WireSliders();

        if (isMenuScene)
        {
            ShowPanel(panelTitle);
            if (menuMusic != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(menuMusic, loop: true);
            if (panelPause != null) panelPause.SetActive(false);
            Time.timeScale = 1f;
        }

        SyncSlidersFromAudio();
    }

    private void Update()
    {
        if (isMenuScene) return;
        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelPause == null) return;
            if (!isPaused) Pause();
            else
            {
                if (panelOptions != null && panelOptions.activeSelf && returnPanelFromOptions == panelPause)
                    ShowPanel(panelPause);
                else
                    Resume();
            }
        }
    }

    private void OnDisable()
    {
        if (isPaused) { Time.timeScale = 1f; isPaused = false; }
    }

    // Acciones principales
    public void OnPlay()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrWhiteSpace(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            Debug.LogError("[UIManager] No se pudo cargar la escena destino.");
    }

    public void OnOptions()
    {
        if (panelOptions == null) return;
        bool show = !panelOptions.activeSelf;
        if (show) SyncSlidersFromAudio();
        ShowPanel(show ? panelOptions : panelTitle);
        if (!show) returnPanelFromOptions = null;
    }

    public void OnCredits()
    {
        if (panelCredits == null) return;
        bool show = !panelCredits.activeSelf;
        ShowPanel(show ? panelCredits : panelTitle);
    }

    public void OnOptionsFromPause()
    {
        if (panelOptions == null) return;
        SyncSlidersFromAudio();
        returnPanelFromOptions = panelPause;
        ShowPanel(panelOptions);
    }

    public void OnBackFromOptions()
    {
        if (returnPanelFromOptions == panelPause && panelPause != null) ShowPanel(panelPause);
        else ShowPanel(panelTitle);
    }

    public void OnBackToTitle() => ShowPanel(panelTitle);

    public void Pause()
    {
        if (panelPause == null) return;
        isPaused = true;
        Time.timeScale = 0f;
        returnPanelFromOptions = panelPause;
        ShowPanel(panelPause);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (panelPause != null) panelPause.SetActive(false);
        if (panelOptions != null && returnPanelFromOptions == panelPause) panelOptions.SetActive(false);
        returnPanelFromOptions = null;
    }

    // Volúmenes
    private void OnMasterVolumeChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.MasterVolume = v;
        PlayerPrefs.SetFloat(KEY_VOL_MASTER, Mathf.Clamp01(v));
    }

    private void OnMusicVolumeChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.MusicVolume = v;
        PlayerPrefs.SetFloat(KEY_VOL_MUSIC, Mathf.Clamp01(v));
    }

    private void OnSFXVolumeChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SfxVolume = v;
        PlayerPrefs.SetFloat(KEY_VOL_SFX, Mathf.Clamp01(v));
    }

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

        if (sliderMaster != null) sliderMaster.SetValueWithoutNotify(master);
        if (sliderMusic != null) sliderMusic.SetValueWithoutNotify(music);
        if (sliderSFX != null) sliderSFX.SetValueWithoutNotify(sfx);
    }

    // Utilidades
    private void ShowPanel(GameObject panelToShow)
    {
        if (panelTitle != null) panelTitle.SetActive(panelToShow == panelTitle && panelToShow != null);
        if (panelOptions != null) panelOptions.SetActive(panelToShow == panelOptions && panelToShow != null);
        if (panelCredits != null) panelCredits.SetActive(panelToShow == panelCredits && panelToShow != null);
        if (panelPause != null) panelPause.SetActive(panelToShow == panelPause && panelToShow != null);
    }

    private void WireMenuButtons()
    {
        if (buttonStart != null) buttonStart.onClick.AddListener(OnPlay);
        if (buttonOptions != null) buttonOptions.onClick.AddListener(OnOptions);
        if (buttonCredits != null) buttonCredits.onClick.AddListener(OnCredits);
    }

    private void WirePauseButtons()
    {
        if (buttonResume != null) buttonResume.onClick.AddListener(Resume);
        if (buttonPauseOptions != null) buttonPauseOptions.onClick.AddListener(OnOptionsFromPause);
    }

    private void WireBackButtons()
    {
        if (buttonBackOptions != null) buttonBackOptions.onClick.AddListener(OnBackFromOptions);
        if (buttonBackCredits != null) buttonBackCredits.onClick.AddListener(OnBackToTitle);
    }

    private void WireSliders()
    {
        if (sliderMaster != null) sliderMaster.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (sliderMusic != null) sliderMusic.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sliderSFX != null) sliderSFX.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void AutoWireInChildren()
    {
        if (panelTitle == null) panelTitle = FindChildGO("PanelTitle");
        if (panelOptions == null) panelOptions = FindChildGO("PanelOptions");
        if (panelCredits == null) panelCredits = FindChildGO("PanelCredits");
        if (panelPause == null) panelPause = FindChildGO("PanelPause");

        if (buttonStart == null) buttonStart = FindChild<Button>("ButtonStart");
        if (buttonOptions == null) buttonOptions = FindChild<Button>("ButtonOptions");
        if (buttonCredits == null) buttonCredits = FindChild<Button>("ButtonCredits");

        if (buttonBackOptions == null) buttonBackOptions = FindChild<Button>("ButtonBackOptions");
        if (buttonBackOptions == null && panelOptions != null) buttonBackOptions = FindIn<Button>(panelOptions, "ButtonBack");

        if (buttonBackCredits == null) buttonBackCredits = FindChild<Button>("ButtonBackCredits");
        if (buttonBackCredits == null && panelCredits != null) buttonBackCredits = FindIn<Button>(panelCredits, "ButtonBack");

        if (buttonResume == null) buttonResume = FindChild<Button>("ButtonResume");
        if (buttonPauseOptions == null) buttonPauseOptions = FindChild<Button>("ButtonPauseOptions");

        if (sliderMaster == null) sliderMaster = FindChild<Slider>("SliderMaster");
        if (sliderMusic == null) sliderMusic = FindChild<Slider>("SliderMusic");
        if (sliderSFX == null) sliderSFX = FindChild<Slider>("SliderSFX");
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