using System.Collections;
using UnityEngine;

/// <summary>
/// Controla los objetos recolectables:
/// - Monedas
/// - Power-ups de vida
/// - Power-ups de invencibilidad
/// - Power-ups de doble salto
/// </summary>
public class Pickable : MonoBehaviour
{
    public enum PickableType
    {
        Coin,
        HealthPowerUp,
        InvincibilityPowerUp,
        DoubleJumpPowerUp
    }

    [Header("Tipo y Valores")]
    [SerializeField] private PickableType type;
    [SerializeField] private float value = 10f;
    [SerializeField] private float respawnTime = 5f;

    [Header("Área de Reaparición (solo power-ups)")]
    [SerializeField] private Vector2 spawnAreaMin;
    [SerializeField] private Vector2 spawnAreaMax;

    // Componentes cacheados
    private SpriteRenderer spriteRenderer;
    private new Collider2D collider;
    private Animator animator;

    private static readonly int HashCollect = Animator.StringToHash("Collect");

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Los power-ups (no monedas) aparecen en posición aleatoria al inicio
        if (type != PickableType.Coin)
            Respawn();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Detectar jugador por componente (no por tag)
        PlayerController playerController = col.GetComponent<PlayerController>();
        if (playerController == null)
            return;

        Debug.Log($"[Pickable] {name} collected by {playerController.name} (type={type})");

        if (animator != null)
            animator.SetTrigger(HashCollect);

        switch (type)
        {
            case PickableType.Coin:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddCoin();
                    Debug.Log("[Pickable] Coin collected -> GameManager.AddCoin()");
                }
                break;

            case PickableType.HealthPowerUp:
                {
                    PlayerHealth ph = col.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        int healAmount = Mathf.RoundToInt(value);
                        ph.RestoreHealth(healAmount);
                        Debug.Log($"[Pickable] HealthPowerUp applied -> Restore {healAmount} HP to {ph.gameObject.name}");
                    }
                }
                break;

            case PickableType.InvincibilityPowerUp:
                {
                    PlayerHealth ph = col.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.ActivateInvincibility(value);
                        Debug.Log($"[Pickable] InvincibilityPowerUp applied -> {ph.gameObject.name} invincible for {value}s");
                    }
                }
                break;

            case PickableType.DoubleJumpPowerUp:
                if (playerController != null)
                {
                    playerController.ActivateDoubleJump(value);
                    Debug.Log($"[Pickable] DoubleJumpPowerUp applied -> {playerController.name} double jump for {value}s");
                }
                break;
        }

        // Ocultar y manejar reaparecer o destruir
        spriteRenderer.enabled = false;
        if (collider != null) collider.enabled = false;

        if (type != PickableType.Coin)
        {
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            Debug.Log($"[Pickable] Coin destroyed: {name}");
            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        Respawn();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (collider != null) collider.enabled = true;
        Debug.Log($"[Pickable] Respawned {name} at {transform.position}");
    }

    private void Respawn()
    {
        if (type == PickableType.Coin) return;

        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        transform.position = new Vector2(randomX, randomY);
    }

    private void OnDrawGizmosSelected()
    {
        if (type == PickableType.Coin) return;
        Gizmos.color = Color.green;
        Vector2 center = (spawnAreaMin + spawnAreaMax) / 2f;
        Vector2 size = spawnAreaMax - spawnAreaMin;
        Gizmos.DrawWireCube(transform.position + (Vector3)center, size);
    }
}