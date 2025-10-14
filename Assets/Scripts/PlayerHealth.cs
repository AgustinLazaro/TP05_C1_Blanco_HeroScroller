using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;

    private int currentHealth;
    private bool isInvincible;

    private void Awake()
    {
        if (healthBar == null)
        {
            var go = GameObject.Find("HP player bar");
            if (go != null) healthBar = go.GetComponent<Slider>();
        }
        if (healthBarFill == null && healthBar != null && healthBar.fillRect != null)
            healthBarFill = healthBar.fillRect.GetComponent<Image>();

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        UpdateHealthBarColor();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (healthBar != null) healthBar.value = currentHealth;
        UpdateHealthBarColor();

        if (currentHealth <= 0) Die();
    }

    public void RestoreHealth(int percentage)
    {
        float pct = Mathf.Clamp01(percentage / 100f);
        int toRestore = Mathf.RoundToInt(maxHealth * pct);
        currentHealth = Mathf.Min(currentHealth + toRestore, maxHealth);
        if (healthBar != null) healthBar.value = currentHealth;
        UpdateHealthBarColor();
    }

    private void UpdateHealthBarColor()
    {
        if (healthBarFill == null) return;
        float pct = (float)currentHealth / Mathf.Max(1, maxHealth);
        healthBarFill.color = Color.Lerp(Color.red, Color.green, pct);
    }

    private void Die()
    {
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        var gameOver = FindObjectOfType<GameOverUI>(true);
        if (gameOver != null) gameOver.Show();
        else Debug.LogWarning("[PlayerHealth] GameOverUI no encontrado.");
    }

    public void ActivateInvincibility(float duration)
    {
        if (duration > 0f) StartCoroutine(InvincibilityCoroutine(duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }
}