using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Objetivos")]
    [SerializeField] private int targetCoins = 0;
    [SerializeField] private int targetKills = 0;

    [Header("Progreso")]
    [SerializeField] private int coinsCollected = 0;
    [SerializeField] private int enemiesKilled = 0;

    [Header("HUD (TMP)")]
    [SerializeField] private TextMeshProUGUI coinsTMP; // "TextCoin" o "TextCoins"
    [SerializeField] private TextMeshProUGUI killsTMP; // "TextEnemy" o "TextKills"

    [Header("UI Victoria")]
    [SerializeField] private VictoryUI victoryUI;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        AutoWire();
        UpdateHUD();
    }

    private void AutoWire()
    {
        if (coinsTMP == null) coinsTMP = FindTMPByNames("TextCoin", "TextCoins");
        if (killsTMP == null) killsTMP = FindTMPByNames("TextEnemy", "TextKills");
        if (victoryUI == null) victoryUI = FindObjectOfType<VictoryUI>(true);
    }

    private TextMeshProUGUI FindTMPByNames(params string[] names)
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
                for (int i = 0; i < names.Length; i++)
                    if (t.name == names[i]) return t;
        }
        return null;
    }

    private void UpdateHUD()
    {
        if (coinsTMP != null)
        {
            coinsTMP.text = targetCoins > 0
                ? $"Monedas: {coinsCollected}/{targetCoins}"
                : $"Monedas: {coinsCollected}";
        }

        if (killsTMP != null)
        {
            killsTMP.text = targetKills > 0
                ? $"Kills: {enemiesKilled}/{targetKills}"
                : $"Kills: {enemiesKilled}";
        }
    }

    private void CheckVictory()
    {
        bool coinsOK = (targetCoins <= 0) || (coinsCollected >= targetCoins);
        bool killsOK = (targetKills <= 0) || (enemiesKilled >= targetKills);

        if (coinsOK && killsOK)
        {
            string resumen =
                $"Objetivo logrado!\nMonedas: {coinsCollected}/{(targetCoins > 0 ? targetCoins : coinsCollected)}\n" +
                $"Enemigos eliminados: {enemiesKilled}/{(targetKills > 0 ? targetKills : enemiesKilled)}";

            if (victoryUI == null) victoryUI = FindObjectOfType<VictoryUI>(true);
            if (victoryUI != null) victoryUI.Show(resumen);
            else Debug.LogWarning("[GameManager] VictoryUI no encontrado. " + resumen);

            Time.timeScale = 0f;
        }
    }

    // API pública
    public void AddCoin()
    {
        coinsCollected++;
        UpdateHUD();
        CheckVictory();
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        UpdateHUD();
        CheckVictory();
    }
}