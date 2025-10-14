using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panelGameOver; // "PanelGameOver"
    [SerializeField] private Button buttonRetry;       // "ButtonRetry"
    [SerializeField] private Button buttonMenu;        // "ButtonMenu"

    [Header("Audio")]
    [SerializeField] private AudioClip defeatSfx;

    [SerializeField] private string menuSceneName = "SceneMenu";

    private void Awake()
    {
        AutoWireInChildren();

        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (buttonRetry != null) buttonRetry.onClick.AddListener(Retry);
        if (buttonMenu != null) buttonMenu.onClick.AddListener(LoadMenu);
    }

    public void Show()
    {
        if (panelGameOver == null)
        {
            Debug.LogError("[GameOverUI] Falta 'PanelGameOver' como hijo del HUD.");
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            if (defeatSfx != null) AudioManager.Instance.PlaySFX(defeatSfx);
        }

        panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Retry()
    {
        Time.timeScale = 1f;
        var current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    private void LoadMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrWhiteSpace(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
        else
            Debug.LogError("[GameOverUI] 'menuSceneName' no configurado.");
    }

    private void AutoWireInChildren()
    {
        if (panelGameOver == null) panelGameOver = FindChildGO("PanelGameOver");
        if (buttonRetry == null) buttonRetry = FindChild<Button>("ButtonRetry");
        if (buttonMenu == null) buttonMenu = FindChild<Button>("ButtonMenu");
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
}