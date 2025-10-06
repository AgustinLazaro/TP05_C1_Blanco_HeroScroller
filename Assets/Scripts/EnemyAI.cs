using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 2f;

    private Vector2 direction;
    private Transform player;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        MoveTowardsPlayer();
    }

    public void SetInitialDirection(Vector2 initialDirection)
    {
        direction = initialDirection.normalized;
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector2 moveDirection = (player.position - transform.position).normalized;

            Vector3 moveDirection3D = new Vector3(moveDirection.x, moveDirection.y, 0);

            transform.position += moveDirection3D * moveSpeed * Time.deltaTime;
        }
    }
}