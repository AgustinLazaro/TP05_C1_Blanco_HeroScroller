using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Enemigo: vida, ataques, animaciones y barra de vida.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Combate")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Vector2 attackRange = new Vector2(1f, 0.8f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float deathDelay = 0.9f;
    [SerializeField] private float attackDuration = 0.3f;

    [Header("Barra de Vida")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private float healthBarOffset = 1.5f;

    private Animator animator;
    private Camera mainCamera;
    private Slider healthBar;
    private Image healthBarFill;
    private int currentHealth;
    public bool IsDead { get; private set; }
    public int Damage => damageAmount;

    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHurt = Animator.StringToHash("Hit");
    private static readonly int HashDie = Animator.StringToHash("Die");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        currentHealth = maxHealth;
        SetupHealthBar();
    }

    private void SetupHealthBar()
    {
        if (healthBarPrefab == null) return;
        Canvas canvas = null;
        foreach (var c in FindObjectsOfType<Canvas>())
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
        if (canvas == null) return;

        GameObject barra = Instantiate(healthBarPrefab, canvas.transform);
        healthBar = barra.GetComponentInChildren<Slider>();
        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = 1;
            healthBar.value = 1;
            healthBarFill = healthBar.fillRect?.GetComponent<Image>();
            UpdateHealthBarPosition();
        }
    }

    private void Update() => UpdateHealthBarPosition();

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthBar();
        animator?.SetTrigger(HashHurt);
        if (currentHealth <= 0) StartCoroutine(DieRoutine());
    }

    public void SetMovementState(float speed)
    {
        if (animator == null) return;
        animator.SetFloat(HashSpeed, Mathf.Abs(speed));
        animator.SetBool(HashIsMoving, Mathf.Abs(speed) > 0.1f);
    }

    public void Attack()
    {
        if (IsDead) return;
        animator?.SetTrigger(HashAttack);
        StartCoroutine(DealDamageAfterDelay());
    }

    private IEnumerator DealDamageAfterDelay()
    {
        yield return new WaitForSeconds(attackDuration);
        DealDamageToPlayer();
    }

    public void DealDamageToPlayer()
    {
        if (IsDead) return;

        Vector3 center = attackOrigin != null ? attackOrigin.position : transform.position + transform.right * (attackRange.x * 0.5f);
        Collider2D[] hits = playerLayer.value != 0
            ? Physics2D.OverlapBoxAll(center, attackRange, 0f, playerLayer)
            : Physics2D.OverlapBoxAll(center, attackRange, 0f);

        if (hits == null || hits.Length == 0) return;

        foreach (var h in hits)
        {
            if (h == null) continue;
            var ph = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>() ?? h.GetComponentInChildren<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damageAmount);
                return;
            }
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;
        float pct = (float)currentHealth / maxHealth;
        healthBar.value = pct;
        if (healthBarFill != null) healthBarFill.color = Color.Lerp(Color.red, Color.green, pct);
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBar == null || mainCamera == null) return;
        Vector3 worldPos = transform.position + Vector3.up * healthBarOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        healthBar.transform.position = screenPos;
    }

    private IEnumerator DieRoutine()
    {
        IsDead = true;
        var ai = GetComponent<EnemyAI>();
        if (ai != null) { ai.StopMovement(); ai.enabled = false; }
        foreach (var col in GetComponents<Collider2D>()) col.enabled = false;
        animator?.SetTrigger(HashDie);
        yield return new WaitForSeconds(deathDelay);
        if (GameManager.Instance != null) GameManager.Instance.EnemyKilled();
        if (healthBar != null) Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (healthBar != null) Destroy(healthBar.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(origin, attackRange);
    }
}