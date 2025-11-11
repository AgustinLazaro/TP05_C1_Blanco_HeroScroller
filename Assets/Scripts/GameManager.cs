using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // ========== SINGLETON ==========
    // Permite acceder a este script desde cualquier lugar con GameManager.Instance
    public static GameManager Instance { get; private set; }
    
    // ========== ESTADO DEL JUEGO ==========
    public bool IsGameOver { get; private set; }

    // ========== OBJETIVOS DEL NIVEL ==========
    [Header("Objetivos del Nivel")]
    [SerializeField] private int targetCoins = 0;  // Monedas necesarias para ganar (0 = sin objetivo)
    [SerializeField] private int targetKills = 0;  // Enemigos a eliminar (0 = sin objetivo)

    // ========== TEXTOS DEL HUD ==========
    [Header("Textos del HUD")]
    [SerializeField] private TextMeshProUGUI coinsTMP;  // Texto que muestra las monedas
    [SerializeField] private TextMeshProUGUI killsTMP;  // Texto que muestra los kills

    // ========== PANTALLAS DE VICTORIA/DERROTA ==========
    [Header("Pantallas de Fin de Juego")]
    [SerializeField] private VictoryUI victoryUI;      // Pantalla de victoria
    [SerializeField] private GameOverUI gameOverUI;    // Pantalla de game over

    // ========== PROGRESO ACTUAL ==========
    private int coinsCollected;   // Monedas recolectadas
    private int enemiesKilled;    // Enemigos eliminados

    // ========== INICIALIZACIÓN ==========
    private void Awake()
    {
        // Configurar el Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Buscar referencias automáticamente si no están asignadas
        AutoWire();
        
        // Mostrar el HUD inicial
        UpdateHUD();
    }

    /// <summary>
    /// Busca automáticamente los componentes necesarios en la escena
    /// </summary>
    private void AutoWire()
    {
        // Buscar textos del HUD si no están asignados
        if (coinsTMP == null)
            coinsTMP = FindTMPByNames("TextCoin", "TextCoins");
        
        if (killsTMP == null)
            killsTMP = FindTMPByNames("TextEnemy", "TextKills");
        
        // Buscar pantallas de victoria/derrota
        if (victoryUI == null)
            victoryUI = FindObjectOfType<VictoryUI>(true);
        
        if (gameOverUI == null)
            gameOverUI = FindObjectOfType<GameOverUI>(true);
    }

    /// <summary>
    /// Busca un TextMeshProUGUI por varios nombres posibles
    /// </summary>
    private TextMeshProUGUI FindTMPByNames(params string[] names)
    {
        // Obtener todos los objetos raíz de la escena
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        
        // Buscar en cada objeto raíz
        foreach (GameObject root in roots)
        {
            // Obtener todos los TextMeshProUGUI (incluso desactivados)
            TextMeshProUGUI[] tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            
            // Verificar cada texto
            foreach (TextMeshProUGUI tmp in tmps)
            {
                // Verificar si coincide con alguno de los nombres
                for (int i = 0; i < names.Length; i++)
                {
                    if (tmp.name == names[i])
                        return tmp;
                }
            }
        }
        
        return null;
    }

    // ========== ACTUALIZACIÓN DEL HUD ==========
    
    /// <summary>
    /// Actualiza los textos del HUD con el progreso actual
    /// </summary>
    private void UpdateHUD()
    {
        // Actualizar texto de monedas
        if (coinsTMP != null)
        {
            if (targetCoins > 0)
                coinsTMP.text = $"Monedas: {coinsCollected}/{targetCoins}";
            else
                coinsTMP.text = $"Monedas: {coinsCollected}";
        }

        // Actualizar texto de kills
        if (killsTMP != null)
        {
            if (targetKills > 0)
                killsTMP.text = $"Kills: {enemiesKilled}/{targetKills}";
            else
                killsTMP.text = $"Kills: {enemiesKilled}";
        }
    }

    // ========== VERIFICACIÓN DE VICTORIA ==========
    
    /// <summary>
    /// Verifica si se cumplieron los objetivos para ganar
    /// </summary>
    private void CheckVictory()
    {
        // Verificar objetivo de monedas
        bool coinsOK = (targetCoins <= 0) || (coinsCollected >= targetCoins);
        
        // Verificar objetivo de kills
        bool killsOK = (targetKills <= 0) || (enemiesKilled >= targetKills);

        // Si se cumplieron ambos objetivos, el jugador gana
        if (coinsOK && killsOK)
        {
            // Crear mensaje de victoria
            string resumen = $"¡Objetivo logrado!\nMonedas: {coinsCollected}\nEnemigos: {enemiesKilled}";
            
            // Mostrar pantalla de victoria
            if (victoryUI != null)
                victoryUI.Show(resumen);
            
            // Marcar el juego como terminado
            SetGameOver();
        }
    }

    // ========== GAME OVER ==========
    
    /// <summary>
    /// Marca el juego como terminado y muestra la pantalla de Game Over
    /// </summary>
    public void SetGameOver()
    {
        // Si ya terminó, no hacer nada
        if (IsGameOver)
            return;
        
        // Marcar como terminado
        IsGameOver = true;
        
        // Iniciar secuencia de Game Over
        StartCoroutine(GameOverSequence());
    }

    /// <summary>
    /// Corrutina que maneja la secuencia de Game Over
    /// </summary>
    private IEnumerator GameOverSequence()
    {
        // Esperar 1 segundo (para que se vea la animación de muerte)
        yield return new WaitForSeconds(1f);

        // Pausar el juego
        Time.timeScale = 0f;

        // Mostrar pantalla de Game Over
        if (gameOverUI != null)
            gameOverUI.Show();
    }

    // ========== MÉTODOS PÚBLICOS ==========
    
    /// <summary>
    /// Llamar cuando el jugador recolecte una moneda
    /// </summary>
    public void AddCoin()
    {
        coinsCollected++;
        UpdateHUD();
        CheckVictory();
    }

    /// <summary>
    /// Llamar cuando el jugador mate un enemigo
    /// </summary>
    public void EnemyKilled()
    {
        enemiesKilled++;
        UpdateHUD();
        CheckVictory();
    }
}