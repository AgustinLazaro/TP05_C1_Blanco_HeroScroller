using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;           // si no lo tienes, calculamos velocidad por delta de posición

    [Header("Ataque")]
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private Vector2 hitBoxSize = new Vector2(0.6f, 0.5f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private bool autoAttack = true;

    [Header("Locomoción (opcional)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.08f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debug")]
    [SerializeField] private bool debugGizmos = true;

    private float nextAttackTime;

    // Hashes
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private static readonly int HashDie = Animator.StringToHash("Die");

    private Vector3 _lastPos;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        _lastPos = transform.position;
    }

    private void Update()
    {
        // 1) Locomoción: Speed (+ opcional grounded)
        float speed = GetHorizontalSpeed();
        animator.SetFloat(HashSpeed, speed);

        if (groundCheck != null)
            animator.SetBool(HashIsGrounded, IsGrounded());

        // 2) Auto-ataque si el jugador entra en rango (opcional)
        if (autoAttack && Time.time >= nextAttackTime && PlayerInHitbox())
        {
            animator.SetTrigger(HashAttack);
            nextAttackTime = Time.time + attackCooldown;
        }

        _lastPos = transform.position;
    }

    private float GetHorizontalSpeed()
    {
        if (rb != null)
            return Mathf.Abs(rb.velocity.x);

        // Si no hay Rigidbody2D, calcula por delta de posición
        float dx = transform.position.x - _lastPos.x;
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        return Mathf.Abs(dx / dt);
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return true;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private bool PlayerInHitbox()
    {
        if (!hitOrigin) return false;
        var hits = Physics2D.OverlapBoxAll(hitOrigin.position, hitBoxSize, 0f, playerLayer);
        return hits.Length > 0;
    }

    // Animation Event en el frame de impacto del clip "attack"
    public void AnimEvent_DoDamage()
    {
        if (!hitOrigin) return;
        var hits = Physics2D.OverlapBoxAll(hitOrigin.position, hitBoxSize, 0f, playerLayer);
        foreach (var h in hits)
            if (h.TryGetComponent<PlayerHealth>(out var ph))
                ph.TakeDamage(damage);
    }

    // Llamables desde IA
    public void TriggerAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            animator.SetTrigger(HashAttack);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void TriggerHit() => animator.SetTrigger(HashHit);
    public void TriggerDie() => animator.SetTrigger(HashDie);

    private void OnDrawGizmosSelected()
    {
        if (debugGizmos && hitOrigin)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hitOrigin.position, hitBoxSize);
        }
        if (debugGizmos && groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}