using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryUI : MonoBehaviour
{
    [SerializeField] private GameObject panelVictory;     // "PanelVictory"
    [SerializeField] private Button buttonRetry;          // "ButtonRetry"
    [SerializeField] private Button buttonMenu;           // "ButtonMenu"
    [SerializeField] private TextMeshProUGUI messageTMP;  // "TextVictory" (TMP)

    [Header("Audio")]
    [SerializeField] private AudioClip victorySfx;

    [SerializeField] private string menuSceneName = "SceneMenu";

    private void Awake()
    {
        AutoWireInChildren();

        if (panelVictory != null) panelVictory.SetActive(false);
        if (buttonRetry != null) buttonRetry.onClick.AddListener(Retry);
        if (buttonMenu != null) buttonMenu.onClick.AddListener(LoadMenu);
    }

    public void Show(string message)
    {
        if (panelVictory == null)
        {
            Debug.LogError("[VictoryUI] Falta 'PanelVictory' como hijo del HUD.");
            return;
        }

        if (messageTMP != null) messageTMP.text = message;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            if (victorySfx != null) AudioManager.Instance.PlaySFX(victorySfx);
        }

        panelVictory.SetActive(true);
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
            Debug.LogError("[VictoryUI] 'menuSceneName' no configurado.");
    }

    private void AutoWireInChildren()
    {
        if (panelVictory == null) panelVictory = FindChildGO("PanelVictory");
        if (buttonRetry == null) buttonRetry = FindChild<Button>("ButtonRetry");
        if (buttonMenu == null) buttonMenu = FindChild<Button>("ButtonMenu");
        if (messageTMP == null)
        {
            var go = FindChildGO("TextVictory");
            if (go != null) messageTMP = go.GetComponent<TextMeshProUGUI>();
        }
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