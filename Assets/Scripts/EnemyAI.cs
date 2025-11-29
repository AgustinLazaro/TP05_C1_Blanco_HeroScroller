using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Comportamiento")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float visionRange = 5f;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float attackCooldown = 0.6f;

    [Header("Detección de Suelo (opcional)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Referencias")]
    [SerializeField] private GameObject playerPrefab;

    private Transform player;
    private Enemy enemy;
    private Rigidbody2D rb;
    private Collider2D col;

    private float nextAttackTime;
    private bool canMove = true;
    private State currentState;

    private float targetVelX;
    private float desiredVelX;

    private enum State { Idle, Walking, Attacking }

    // ========== CICLO DE VIDA ==========
    private void Start()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = Mathf.Max(0.5f, rb.gravityScale);
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform, false);
            float yOffset = (col != null) ? -col.bounds.extents.y : -0.5f;
            go.transform.localPosition = new Vector3(0f, yOffset, 0f);
            groundCheck = go.transform;
        }

        var pc = FindObjectOfType<PlayerController>();
        if (pc != null) player = pc.transform;
        else if (playerPrefab != null)
        {
            GameObject p = Instantiate(playerPrefab);
            player = p.transform;
        }

        currentState = State.Idle;
    }

    private void Update()
    {
        if (player == null || !canMove || enemy.IsDead)
        {
            targetVelX = 0f;
            enemy.SetMovementState(0f);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > visionRange) currentState = State.Idle;
        else if (distance <= attackDistance) currentState = State.Attacking;
        else currentState = State.Walking;

        switch (currentState)
        {
            case State.Idle: targetVelX = 0f; break;
            case State.Walking:
                float dir = Mathf.Sign(player.position.x - transform.position.x);
                targetVelX = dir * moveSpeed;
                break;
            case State.Attacking:
                targetVelX = 0f;
                if (Time.time >= nextAttackTime)
                {
                    enemy.Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }
                break;
        }

        UpdateOrientation();
    }

    private void FixedUpdate()
    {
        if (!canMove || enemy.IsDead || rb == null) return;
        desiredVelX = Mathf.MoveTowards(rb.velocity.x, targetVelX, acceleration * Time.fixedDeltaTime);
        rb.velocity = new Vector2(desiredVelX, rb.velocity.y);
        enemy.SetMovementState(Mathf.Abs(desiredVelX));
    }

    // ========== UTILIDADES ==========
    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        foreach (var h in hits)
        {
            if (h == null || h.gameObject == gameObject || h.isTrigger || h.transform.IsChildOf(transform)) continue;
            return true;
        }
        return false;
    }

    private void UpdateOrientation()
    {
        if (player == null) return;
        float dir = player.position.x - transform.position.x;
        Vector3 s = transform.localScale;
        s.x = dir < 0 ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
        transform.localScale = s;
    }

    public void StopMovement()
    {
        canMove = false;
        targetVelX = 0f;
        if (rb != null) rb.velocity = Vector2.zero;
        enemy.SetMovementState(0);
    }
}