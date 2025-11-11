using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la vida del jugador usando un sistema de corazones visuales
/// Cada corazón representa 2 puntos de vida (lleno=2, medio=1, vacío=0)
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    // ========== CONFIGURACIÓN ==========
    [Header("Configuración de Vida")]
    [SerializeField] private int maxHealth = 6; // 6 vida = 3 corazones × 2
    [SerializeField] private PlayerController playerController;
    
    [Header("Sistema de Corazones Visuales")]
    [SerializeField] private GameObject heartContainer; // Panel que contiene los corazones
    [SerializeField] private Sprite heartFull;          // Sprite: corazón lleno
    [SerializeField] private Sprite heartHalf;          // Sprite: corazón medio
    [SerializeField] private Sprite heartEmpty;         // Sprite: corazón vacío
    
    // ========== VARIABLES PRIVADAS ==========
    private Image[] heartImages;        // Array con las imágenes de los corazones
    private int currentHealth;          // Vida actual del jugador
    private bool isInvincible;          // ¿El jugador es invencible?

    // ========== INICIALIZACIÓN ==========
    private void Awake()
    {
        currentHealth = maxHealth;
        
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        SetupHeartSystem();
    }

    private void SetupHeartSystem()
    {
        if (heartContainer == null) 
            return;
        
        heartImages = heartContainer.GetComponentsInChildren<Image>();
        UpdateHearts();
    }

    // ========== MÉTODOS PÚBLICOS ==========
    
    public void TakeDamage(int damage)
    {
        if (isInvincible) 
            return;

        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;
        
        UpdateHearts();

        if (currentHealth <= 0)
            Die();
    }

    public void RestoreHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        
        UpdateHearts();
    }

    public void ActivateInvincibility(float duration)
    {
        if (duration > 0)
            StartCoroutine(InvincibilityCoroutine(duration));
    }

    // ========== MÉTODOS PRIVADOS ==========

    private void UpdateHearts()
    {
        if (heartImages == null || heartImages.Length == 0) 
            return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            // Cada corazón representa 2 puntos de vida
            int vidaQueRepresenta = (i + 1) * 2; // 2, 4, 6
            
            if (currentHealth >= vidaQueRepresenta)
            {
                // Corazón lleno
                heartImages[i].sprite = heartFull;
                heartImages[i].color = Color.white;
            }
            else if (currentHealth >= vidaQueRepresenta - 1)
            {
                // Corazón medio
                heartImages[i].sprite = heartHalf;
                heartImages[i].color = Color.white; // Cambiado a blanco para usar el sprite real
            }
            else
            {
                // Corazón vacío
                heartImages[i].sprite = heartEmpty;
                heartImages[i].color = Color.white;
            }
            
            heartImages[i].enabled = true;
        }
    }

    private void Die()
    {
        if (playerController != null)
            playerController.PlayDie();
        
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameOver();
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }
}