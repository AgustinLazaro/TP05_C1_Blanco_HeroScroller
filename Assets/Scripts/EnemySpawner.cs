using UnityEngine;

/// <summary>
/// Genera enemigos automáticamente dentro de un área definida.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Generación")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 5f);

    [Header("Enemigos")]
    [SerializeField] private float minMoveSpeed = 1f;
    [SerializeField] private float maxMoveSpeed = 3f;

    [Header("Ajuste")]
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundSnapDistance = 10f;

    private float timer;

    private void Update()
    {
        if (enemyPrefab == null) return;
        timer += Time.deltaTime;
        if (timer >= spawnInterval) { SpawnEnemy(); timer = 0f; }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        float randomX = Random.Range(-spawnAreaSize.x, spawnAreaSize.x) * 0.5f;
        float randomY = Random.Range(-spawnAreaSize.y, spawnAreaSize.y) * 0.5f;
        Vector2 spawnPosition = (Vector2)transform.position + new Vector2(randomX, randomY);

        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        if (snapToGround) SnapToGround(enemyObject);

        float randomSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        enemyObject.SendMessage("SetMoveSpeed", randomSpeed, SendMessageOptions.DontRequireReceiver);
    }

    private void SnapToGround(GameObject go)
    {
        if (go == null) return;
        var col = go.GetComponent<Collider2D>();
        var tr = go.transform;
        Vector2 origin = col != null ? new Vector2(col.bounds.center.x, col.bounds.max.y) : (Vector2)tr.position;
        float castDist = groundSnapDistance + (col != null ? col.bounds.extents.y : 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, castDist, groundLayers);
        if (hit.collider != null && !hit.collider.isTrigger && hit.collider.gameObject != go)
        {
            float extentsY = col != null ? col.bounds.extents.y : 0.5f;
            float targetY = hit.point.y + extentsY;
            tr.position = new Vector3(tr.position.x, targetY, tr.position.z);
        }
    }

    private void OnDrawGizmos() { Gizmos.color = Color.red; Gizmos.DrawWireCube(transform.position, spawnAreaSize); }
}