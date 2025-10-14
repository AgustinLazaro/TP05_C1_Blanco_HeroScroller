using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int health = 20;
    [SerializeField] private int maxHealth = 20;
    [SerializeField] public int damage = 20;

    [Header("UI Salud")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private float worldYOffset = 1.5f;

    private Slider healthBar;
    private Image healthBarFill;
    private Canvas uiCanvas;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        foreach (var c in FindObjectsOfType<Canvas>())
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) { uiCanvas = c; break; }
        }

        if (uiCanvas == null || healthBarPrefab == null)
        {
            Debug.LogWarning("[Enemy] Falta Canvas (Overlay) o 'healthBarPrefab'.");
            return;
        }

        var barGO = Instantiate(healthBarPrefab, uiCanvas.transform);
        barGO.SetActive(true);

        healthBar = barGO.GetComponentInChildren<Slider>(true);
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = (float)health / Mathf.Max(1, maxHealth);

            if (healthBar.fillRect != null)
                healthBarFill = healthBar.fillRect.GetComponent<Image>();
        }

        UpdateHealthBarColor();
        UpdateHealthBarPosition();
    }

    private void Update()
    {
        if (cam == null) cam = Camera.main;
        UpdateHealthBarPosition();
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBar == null || cam == null) return;
        Vector3 worldPos = transform.position + Vector3.up * worldYOffset;
        healthBar.transform.position = cam.WorldToScreenPoint(worldPos);
    }

    public void TakeDamage(int amount)
    {
        health = Mathf.Max(0, health - amount);
        if (healthBar != null) healthBar.value = (float)health / Mathf.Max(1, maxHealth);
        UpdateHealthBarColor();

        if (health <= 0)
        {
            if (healthBar != null) Destroy(healthBar.gameObject);
            if (GameManager.Instance != null) GameManager.Instance.EnemyKilled();
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (healthBar != null) Destroy(healthBar.gameObject);
    }

    private void UpdateHealthBarColor()
    {
        if (healthBarFill == null) return;
        float pct = (float)health / Mathf.Max(1, maxHealth);
        healthBarFill.color = Color.Lerp(Color.red, Color.green, pct);
    }
}