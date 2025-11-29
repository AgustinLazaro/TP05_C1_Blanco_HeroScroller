using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Combate")]
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private bool debugGroundCheck = false;

    [Header("Daño por contacto")]
    [SerializeField] private float contactDamageCooldown = 0.35f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Camera mainCamera;

    private float nextAttackTime;
    private bool isDead;
    private bool canDoubleJump;
    private bool hasDoubleJumped;
    private float lastContactDamageTime = -999f;

    // Animator Hashes
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int HashCrouch = Animator.StringToHash("Crouch");
    private static readonly int HashJump = Animator.StringToHash("Jump");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashDie = Animator.StringToHash("Die");
    private static readonly int HashLand = Animator.StringToHash("Land");
    private static readonly int HashVerticalSpeed = Animator.StringToHash("VerticalSpeed");
    private static readonly int HashIsFalling = Animator.StringToHash("IsFalling");

    private bool wasGrounded;

    // Cache de parámetros del animator (hashes) para evitar warnings
    private HashSet<int> animatorParamHashes;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        CacheAnimatorParameters();
    }

    private void CacheAnimatorParameters()
    {
        animatorParamHashes = new HashSet<int>();
        if (animator == null) return;
        foreach (var p in animator.parameters)
            animatorParamHashes.Add(Animator.StringToHash(p.name));
    }

    private bool HasParam(int hash) => animatorParamHashes != null && animatorParamHashes.Contains(hash);

    private void Update()
    {
        if (isDead) return;
        HandleInput();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDead) { rb.velocity = Vector2.zero; return; }
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private void HandleInput()
    {
        float moveInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(moveInput) > 0.01f)
            spriteRenderer.flipX = moveInput < 0f;

        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded())
            {
                PerformJump();
                hasDoubleJumped = false;
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                PerformJump();
                hasDoubleJumped = true;
            }
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            if (HasParam(HashAttack)) animator?.SetTrigger(HashAttack);
            Shoot();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float moveInput = Input.GetAxis("Horizontal");
        bool grounded = IsGrounded();
        float vy = rb.velocity.y;

        if (HasParam(HashSpeed)) animator.SetFloat(HashSpeed, Mathf.Abs(moveInput));
        if (HasParam(HashIsGrounded)) animator.SetBool(HashIsGrounded, grounded);
        if (HasParam(HashCrouch)) animator.SetBool(HashCrouch, Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
        if (HasParam(HashVerticalSpeed)) animator.SetFloat(HashVerticalSpeed, vy);
        if (HasParam(HashIsFalling)) animator.SetBool(HashIsFalling, !grounded && vy < -0.1f);

        if (grounded && !wasGrounded && HasParam(HashLand))
            animator.SetTrigger(HashLand);

        wasGrounded = grounded;
    }

    private void PerformJump()
    {
        if (isDead) return;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        if (HasParam(HashJump)) animator?.SetTrigger(HashJump);
    }

    private void Shoot()
    {
        if (isDead || bulletPrefab == null || bulletSpawnPoint == null) return;
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector2 dir = (mousePosition - bulletSpawnPoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>()?.SetDirection(dir);
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return Mathf.Abs(rb.velocity.y) < 0.01f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        if (debugGroundCheck)
        {
#if UNITY_EDITOR
            // imprime solo si has activado explícitamente el flag en Inspector
            Debug.Log($"IsGrounded: {hits.Length} hits");
#endif
        }
        foreach (var c in hits)
        {
            if (c == null || c.gameObject == gameObject || c.isTrigger || c.transform.IsChildOf(transform)) continue;
            if (debugGroundCheck)
            {
#if UNITY_EDITOR
                Debug.Log($"Ground contact: {c.name}");
#endif
            }
            return true;
        }
        return false;
    }

    public void ActivateDoubleJump(float duration) => StartCoroutine(DoubleJumpCoroutine(duration));
    private IEnumerator DoubleJumpCoroutine(float duration)
    {
        canDoubleJump = true;
        yield return new WaitForSeconds(duration);
        canDoubleJump = false;
        hasDoubleJumped = false;
    }

    public void PlayDie()
    {
        if (isDead) return;
        isDead = true;
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        if (HasParam(HashDie)) animator?.SetTrigger(HashDie);
    }

    private void OnCollisionEnter2D(Collision2D c) => TryContactDamage(c.collider);
    private void OnCollisionStay2D(Collision2D c) => TryContactDamage(c.collider);
    private void OnTriggerEnter2D(Collider2D o) => TryContactDamage(o);
    private void OnTriggerStay2D(Collider2D o) => TryContactDamage(o);

    private void TryContactDamage(Collider2D col)
    {
        if (isDead || playerHealth == null || col == null) return;
        if (Time.time - lastContactDamageTime < contactDamageCooldown) return;

        var enemy = col.GetComponent<Enemy>() ?? col.GetComponentInParent<Enemy>() ?? col.GetComponentInChildren<Enemy>();
        if (enemy == null || enemy.IsDead) return;

        playerHealth.TakeDamage(enemy.Damage);
        lastContactDamageTime = Time.time;
    }
}