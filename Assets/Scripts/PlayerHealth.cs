using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        UpdateHealthBarColor();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;
        UpdateHealthBarColor();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void RestoreHealth(int percentage)
    {
        int healthToRestore = Mathf.RoundToInt(maxHealth * (percentage / 100f));
        currentHealth = Mathf.Min(currentHealth + healthToRestore, maxHealth);
        healthBar.value = currentHealth;
        UpdateHealthBarColor();
    }

    void UpdateHealthBarColor()
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        healthBarFill.color = Color.Lerp(Color.red, Color.green, healthPercentage);
    }

    void Die()
    {
        Debug.Log("Jugador ha muerto");
    }
}