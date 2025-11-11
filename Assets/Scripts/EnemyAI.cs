using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    [Header("Comportamiento")]
    [SerializeField] private float moveSpeed = 3.5f;          // velocidad base aumentada
    [SerializeField] private float acceleration = 8f;         // aceleración para suavizar cambio de velocidad
    [SerializeField] private float visionRange = 5f;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float attackCooldown = 0.6f;

    [Header("Referencias")]
    [Tooltip("Opcional: prefab del Player (si quieres que se instancie)")]
    [SerializeField] private GameObject playerPrefab;

    private Transform player;
    private Enemy enemy;
    private float nextAttackTime;
    private bool canMove = true;
    private State currentState;
    private Rigidbody2D rb;

    // Movimiento planificado y suavizado
    private Vector2 targetVelocity = Vector2.zero;
    private Vector2 currentVelocity = Vector2.zero;
    private bool wantsToMove = false;

    private enum State { Idle, Walking, Attacking }

    private void Start()
    {
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null) player = pc.transform;
        else if (playerPrefab != null)
        {
            GameObject p = Instantiate(playerPrefab);
            player = p.transform;
        }

        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        currentState = State.Idle;
    }

    public void SetPlayer(Transform t) => player = t;
    public void SetMoveSpeed(float speed) => moveSpeed = Mathf.Max(0, speed);

    private void Update()
    {
        if (player == null || !canMove || enemy.IsDead) { targetVelocity = Vector2.zero; enemy.SetMovementState(0); return; }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > visionRange) currentState = State.Idle;
        else if (distanceToPlayer <= attackDistance) currentState = State.Attacking;
        else currentState = State.Walking;

        UpdateState();
        UpdateOrientation();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                targetVelocity = Vector2.zero;
                wantsToMove = false;
                enemy.SetMovementState(0f);
                break;

            case State.Walking:
                Vector2 direction = (player.position - transform.position).normalized;
                targetVelocity = direction * moveSpeed;
                wantsToMove = true;
                // La animación recibe la velocidad objetivo (se suaviza al aplicar currentVelocity)
                enemy.SetMovementState(targetVelocity.magnitude);
                break;

            case State.Attacking:
                targetVelocity = Vector2.zero;
                wantsToMove = false;
                enemy.SetMovementState(0f);
                if (Time.time >= nextAttackTime)
                {
                    enemy.Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!canMove || enemy.IsDead) return;

        // Suavizamos la velocidad hacia la objetivo usando aceleración
        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (rb != null)
        {
            if (currentVelocity != Vector2.zero)
                rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
        }
        else
        {
            if (currentVelocity != Vector2.zero)
                transform.position += (Vector3)currentVelocity * Time.fixedDeltaTime;
        }
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
        wantsToMove = false;
        targetVelocity = Vector2.zero;
        currentVelocity = Vector2.zero;
        enemy.SetMovementState(0);
        if (rb != null) rb.velocity = Vector2.zero;
    }
}