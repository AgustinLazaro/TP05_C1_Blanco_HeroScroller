using System.Collections;
using UnityEngine;

/// <summary>
/// Objetos recolectables: monedas y power-ups.
/// </summary>
public class Pickable : MonoBehaviour
{
    public enum PickableType { Coin, HealthPowerUp, InvincibilityPowerUp, DoubleJumpPowerUp }

    [Header("Tipo y Valores")]
    [SerializeField] private PickableType type;
    [SerializeField] private float value = 10f;
    [SerializeField] private float respawnTime = 5f;

    [Header("Área de Reaparición (power-ups)")]
    [SerializeField] private Vector2 spawnAreaMin;
    [SerializeField] private Vector2 spawnAreaMax;

    private SpriteRenderer spriteRenderer;
    private new Collider2D collider;
    private Animator animator;

    private static readonly int HashCollect = Animator.StringToHash("Collect");

    // ========== CICLO DE VIDA ==========
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (type != PickableType.Coin) Respawn();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var playerController = col.GetComponent<PlayerController>();
        if (playerController == null) return;

        if (animator != null) animator.SetTrigger(HashCollect);

        switch (type)
        {
            case PickableType.Coin:
                GameManager.Instance?.AddCoin();
                break;
            case PickableType.HealthPowerUp:
                var ph = col.GetComponent<PlayerHealth>();
                if (ph != null) ph.RestoreHealth(Mathf.RoundToInt(value));
                break;
            case PickableType.InvincibilityPowerUp:
                var ph2 = col.GetComponent<PlayerHealth>();
                if (ph2 != null) ph2.ActivateInvincibility(value);
                break;
            case PickableType.DoubleJumpPowerUp:
                playerController.ActivateDoubleJump(value);
                break;
        }

        spriteRenderer.enabled = false;
        if (collider != null) collider.enabled = false;

        if (type != PickableType.Coin) StartCoroutine(RespawnCoroutine());
        else Destroy(gameObject);
    }

    // ========== RESPAWN ==========
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        Respawn();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (collider != null) collider.enabled = true;
    }

    private void Respawn()
    {
        if (type == PickableType.Coin) return;
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        transform.position = new Vector2(randomX, randomY);
    }

    // ========== GIZMOS ==========
    private void OnDrawGizmosSelected()
    {
        if (type == PickableType.Coin) return;
        Gizmos.color = Color.green;
        Vector2 center = (spawnAreaMin + spawnAreaMax) / 2f;
        Vector2 size = spawnAreaMax - spawnAreaMin;
        Gizmos.DrawWireCube(transform.position + (Vector3)center, size);
    }
}