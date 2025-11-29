using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(Collider2D))]
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

    [Header("Física")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundSnapDistance = 3f;

    private Animator animator;
    private Camera mainCamera;
    private Slider healthBar;
    private Image healthBarFill;
    private int currentHealth;
    public bool IsDead { get; private set; }
    public int Damage => damageAmount;

    private Rigidbody2D rb;
    private Collider2D mainCollider;

    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHurt = Animator.StringToHash("Hit");
    private static readonly int HashDie = Animator.StringToHash("Die");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");

    private HashSet<int> animatorParamHashes;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        CacheAnimatorParameters();
        EnsurePhysicsSetup();
        SnapToGround();
        SetupHealthBar();
    }

    private void CacheAnimatorParameters()
    {
        animatorParamHashes = new HashSet<int>();
        if (animator == null) return;
        foreach (var p in animator.parameters)
            animatorParamHashes.Add(Animator.StringToHash(p.name));
    }

    private bool HasParam(int hash) => animatorParamHashes != null && animatorParamHashes.Contains(hash);

    private void Update() => UpdateHealthBarPosition();

    private void EnsurePhysicsSetup()
    {
        if (animator != null) animator.applyRootMotion = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = Mathf.Max(0.5f, rb.gravityScale);
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var colliders = GetComponents<Collider2D>();
        foreach (var c in colliders) if (c != null && !c.isTrigger) { mainCollider = c; break; }

        if (mainCollider == null)
        {
            if (colliders.Length > 0)
            {
                mainCollider = colliders[0];
                mainCollider.isTrigger = false;
            }
            else
            {
                var capsule = gameObject.AddComponent<CapsuleCollider2D>();
                capsule.direction = CapsuleDirection2D.Vertical;
                capsule.size = new Vector2(0.6f, 1.2f);
                capsule.isTrigger = false;
                mainCollider = capsule;
            }
        }
    }

    private void SnapToGround()
    {
        if (mainCollider == null) return;
        Vector2 origin = new Vector2(mainCollider.bounds.center.x, mainCollider.bounds.max.y);
        float distance = groundSnapDistance + mainCollider.bounds.extents.y;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, groundLayers);
        if (hit.collider != null && !hit.collider.isTrigger && hit.collider.gameObject != gameObject)
        {
            float targetY = hit.point.y + mainCollider.bounds.extents.y;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }
    }

    private void SetupHealthBar()
    {
        if (healthBarPrefab == null) return;
        Canvas canvas = null;
        foreach (var c in FindObjectsOfType<Canvas>()) if (c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
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
        healthBar.transform.position = mainCamera.WorldToScreenPoint(worldPos);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthBar();
        if (HasParam(HashHurt)) animator?.SetTrigger(HashHurt);
        if (currentHealth <= 0) StartCoroutine(DieRoutine());
    }

    public void SetMovementState(float speed)
    {
        if (animator == null) return;
        float abs = Mathf.Abs(speed);
        if (HasParam(HashSpeed)) animator.SetFloat(HashSpeed, abs);
        if (HasParam(HashIsMoving)) animator.SetBool(HashIsMoving, abs > 0.1f);
    }

    public void Attack()
    {
        if (IsDead) return;
        if (HasParam(HashAttack)) animator?.SetTrigger(HashAttack);
        StartCoroutine(AttackDelay());
    }

    private IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(attackDuration);
        DealDamageToPlayer();
    }

    public void DealDamageToPlayer()
    {
        if (IsDead) return;
        Vector3 center = attackOrigin != null ? attackOrigin.position : transform.position + transform.right * (attackRange.x * 0.5f);
        Collider2D[] hits = playerLayer.value != 0 ? Physics2D.OverlapBoxAll(center, attackRange, 0f, playerLayer) : Physics2D.OverlapBoxAll(center, attackRange, 0f);
        foreach (var h in hits)
        {
            if (h == null) continue;
            var ph = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>() ?? h.GetComponentInChildren<PlayerHealth>();
            if (ph != null) { ph.TakeDamage(damageAmount); return; }
        }
    }

    private IEnumerator DieRoutine()
    {
        IsDead = true;
        var ai = GetComponent<EnemyAI>();
        if (ai != null) { ai.StopMovement(); ai.enabled = false; }

        rb.velocity = Vector2.zero;
        rb.simulated = false;
        foreach (var c in GetComponents<Collider2D>()) c.enabled = false;

        if (HasParam(HashDie)) animator?.SetTrigger(HashDie);
        yield return new WaitForSeconds(deathDelay);

        GameManager.Instance?.EnemyKilled();
        if (healthBar != null) Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy() { if (healthBar != null) Destroy(healthBar.gameObject); }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(origin, attackRange);
        var c = GetComponent<Collider2D>();
        if (c != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
        }
    }
}