using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int health = 2;
    [SerializeField] private int damage = 20;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            GameManager.Instance.EnemyKilled();
            Destroy(gameObject);
        }
    }
}