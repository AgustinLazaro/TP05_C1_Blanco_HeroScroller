using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controla la pantalla de victoria
/// </summary>
public class VictoryUI : MonoBehaviour
{
    // ========== CONFIGURACIÓN ==========
    [Header("Pantalla de Victoria")]
    [SerializeField] private GameObject panelVictory;       // Panel principal
    [SerializeField] private TextMeshProUGUI textVictory;   // Texto de resumen
    [SerializeField] private Button buttonRetry;            // Botón "Reintentar"
    [SerializeField] private Button buttonMenu;             // Botón "Menú"

    [Header("Audio")]
    [SerializeField] private AudioClip victorySfx;          // Sonido de victoria

    // ========== INICIALIZACIÓN ==========
    private void Awake()
    {
        // Buscar referencias automáticamente
        AutoWire();
        
        // Conectar botones
        if (buttonRetry != null)
            buttonRetry.onClick.AddListener(OnRetry);
        
        if (buttonMenu != null)
            buttonMenu.onClick.AddListener(OnMenu);
        
        // Ocultar panel al inicio
        if (panelVictory != null)
            panelVictory.SetActive(false);
    }

    /// <summary>
    /// Busca componentes automáticamente
    /// </summary>
    private void AutoWire()
    {
        if (panelVictory == null)
            panelVictory = GameObject.Find("PanelVictory");
        
        if (textVictory == null && panelVictory != null)
            textVictory = panelVictory.GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonRetry == null)
            buttonRetry = FindButton("ButtonRetry");
        
        if (buttonMenu == null)
            buttonMenu = FindButton("ButtonMenu");
    }

    private Button FindButton(string name)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.name == name)
                return btn;
        }
        return null;
    }

    // ========== MOSTRAR PANTALLA ==========

    /// <summary>
    /// Muestra la pantalla de victoria con un mensaje
    /// </summary>
    public void Show(string message)
    {
        // Actualizar texto
        if (textVictory != null)
            textVictory.text = message;
        
        // Mostrar panel
        if (panelVictory != null)
            panelVictory.SetActive(true);
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Detener música
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
        
        // Reproducir sonido de victoria
        if (victorySfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(victorySfx);
    }

    // ========== BOTONES ==========

    /// <summary>
    /// Botón "Reintentar" - Recarga la escena actual
    /// </summary>
    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Botón "Menú" - Vuelve al menú principal
    /// </summary>
    private void OnMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SceneMenu");
    }
}