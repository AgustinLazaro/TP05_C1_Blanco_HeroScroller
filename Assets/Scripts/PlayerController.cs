using UnityEngine;
using System.Collections;

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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Camera mainCamera;

    private float nextAttackTime;
    private bool isDead;
    private bool canDoubleJump;
    private bool hasDoubleJumped;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

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
        if (Mathf.Abs(moveInput) > 0.01f) spriteRenderer.flipX = (moveInput < 0f);

        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded()) { PerformJump(); hasDoubleJumped = false; }
            else if (canDoubleJump && !hasDoubleJumped) { PerformJump(); hasDoubleJumped = true; }
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            animator?.SetTrigger(Animator.StringToHash("Attack"));
            Shoot();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;
        float moveInput = Input.GetAxis("Horizontal");
        animator.SetFloat(Animator.StringToHash("Speed"), Mathf.Abs(moveInput));
        animator.SetBool(Animator.StringToHash("IsGrounded"), IsGrounded());
        animator.SetBool(Animator.StringToHash("Crouch"), Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
    }

    private void PerformJump()
    {
        if (isDead) return;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        animator?.SetTrigger(Animator.StringToHash("Jump"));
    }

    private void Shoot()
    {
        if (isDead || bulletPrefab == null || bulletSpawnPoint == null) return;
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition); mousePosition.z = 0;
        Vector2 direction = (mousePosition - bulletSpawnPoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>()?.SetDirection(direction);
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return Mathf.Abs(rb.velocity.y) < 0.01f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        if (debugGroundCheck) Debug.Log($"IsGrounded: {hits.Length} hits");
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (c.gameObject == gameObject) continue;
            if (c.isTrigger) continue;
            if (c.transform.IsChildOf(transform)) continue;
            if (debugGroundCheck) Debug.Log($"Ground contact: {c.name}");
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
        animator?.SetTrigger(Animator.StringToHash("Die"));
    }
}