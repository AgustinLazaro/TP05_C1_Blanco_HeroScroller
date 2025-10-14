using UnityEngine;
using System.Collections;

public class Pickable : MonoBehaviour
{
    public enum PickableType
    {
        Coin,
        HealthPowerUp,
        InvincibilityPowerUp,
        DoubleJumpPowerUp
    }

    [SerializeField] private PickableType type;
    [SerializeField] private int value = 10;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaMin;
    [SerializeField] private Vector2 spawnAreaMax;
    [SerializeField] private Color gizmoColor = Color.green;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 5f;

    private SpriteRenderer spriteRenderer;
    private new Collider2D collider;

    public bool IsCoin => type == PickableType.Coin;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (type != PickableType.Coin) Respawn();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        switch (type)
        {
            case PickableType.Coin:
                if (GameManager.Instance != null) GameManager.Instance.AddCoin();
                break;

            case PickableType.HealthPowerUp:
                col.GetComponent<PlayerHealth>()?.RestoreHealth(value);
                break;

            case PickableType.InvincibilityPowerUp:
                col.GetComponent<PlayerHealth>()?.ActivateInvincibility(5f);
                break;

            case PickableType.DoubleJumpPowerUp:
                col.GetComponent<PlayerController>()?.ActivateDoubleJump(10f);
                break;
        }

        spriteRenderer.enabled = false;
        collider.enabled = false;

        if (type == PickableType.HealthPowerUp || type == PickableType.InvincibilityPowerUp || type == PickableType.DoubleJumpPowerUp)
            StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);
        Respawn();
        spriteRenderer.enabled = true;
        collider.enabled = true;
    }

    private void Respawn()
    {
        if (type == PickableType.Coin) return;

        Vector2 pos = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );
        transform.position = pos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube((spawnAreaMin + spawnAreaMax) / 2f, spawnAreaMax - spawnAreaMin);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}