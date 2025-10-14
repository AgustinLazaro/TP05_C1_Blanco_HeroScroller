using System.Collections; // Agregar esta línea
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private PlayerHealth playerHealth; // Referencia al script PlayerHealth

    private Rigidbody2D rb;
    private bool isGrounded;
    private new Collider2D collider; // Usar 'new' para ocultar el miembro heredado

    private bool canDoubleJump = false;
    private bool hasDoubleJumped = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
      
    }

    void Update()
    {
        Move();
        Jump();
        Shoot();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        Vector2 movement = new Vector2(moveX * moveSpeed, rb.velocity.y);
        rb.velocity = movement;
    }

    public void ActivateDoubleJump(float duration)
    {
        StartCoroutine(DoubleJumpCoroutine(duration));
    }

    private IEnumerator DoubleJumpCoroutine(float duration)
    {
        canDoubleJump = true;
        Debug.Log("Doble salto activado.");
        yield return new WaitForSeconds(duration);
        canDoubleJump = false;
        Debug.Log("Doble salto desactivado.");
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                isGrounded = false;
                hasDoubleJumped = false; // Resetear doble salto
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                hasDoubleJumped = true; // Usar doble salto
            }
        }
    }

    void Shoot()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            Vector2 shootDirection = (mousePosition - bulletSpawnPoint.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().SetDirection(shootDirection);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0)
        {
            isGrounded = true;
        }

        // Delegar el daño al script PlayerHealth si colisiona con un enemigo
        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerHealth.TakeDamage(collision.gameObject.GetComponent<Enemy>().damage);
        }
    }
}