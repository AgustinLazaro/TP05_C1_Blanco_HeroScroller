using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla la pantalla de Game Over
/// </summary>
public class GameOverUI : MonoBehaviour
{
    // ========== CONFIGURACIÓN ==========
    [Header("Pantalla de Game Over")]
    [SerializeField] private GameObject panelGameOver;  // Panel principal
    [SerializeField] private Button buttonRetry;        // Botón "Reintentar"
    [SerializeField] private Button buttonMenu;         // Botón "Menú"

    [Header("Audio")]
    [SerializeField] private AudioClip defeatSfx;       // Sonido de derrota

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
        if (panelGameOver != null)
            panelGameOver.SetActive(false);
    }

    /// <summary>
    /// Busca componentes automáticamente
    /// </summary>
    private void AutoWire()
    {
        if (panelGameOver == null)
            panelGameOver = GameObject.Find("PanelGameOver");
        
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
    /// Muestra la pantalla de Game Over
    /// </summary>
    public void Show()
    {
        // Mostrar panel
        if (panelGameOver != null)
            panelGameOver.SetActive(true);
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Detener música
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
        
        // Reproducir sonido de derrota
        if (defeatSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(defeatSfx);
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