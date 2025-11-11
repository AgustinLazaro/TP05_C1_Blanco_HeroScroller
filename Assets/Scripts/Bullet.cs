    using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask enemyLayer;

    private Vector2 direction;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        col = GetComponent<Collider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    private void FixedUpdate()
    {
        if (direction.sqrMagnitude <= 0f) return;
        Vector2 next = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyLayer.value != 0 && ((1 << collision.gameObject.layer) & enemyLayer.value) == 0)
            return;

        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null)
            OnTriggerEnter2D(collision.collider);
    }

    private void OnBecameInvisible() => Destroy(gameObject);
}

public class CollisionDebug : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log($"{name} OnCollisionEnter2D con {col.gameObject.name} (Layer: {LayerMask.LayerToName(col.gameObject.layer)})");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"{name} OnTriggerEnter2D con {col.gameObject.name} (Layer: {LayerMask.LayerToName(col.gameObject.layer)})");
    }
}