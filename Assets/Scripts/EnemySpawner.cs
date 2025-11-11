using UnityEngine;

/// <summary>
/// Genera enemigos automáticamente en intervalos de tiempo
/// dentro de un área definida
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ========== CONFIGURACIÓN DE SPAWN ==========
    [Header("Generación de Enemigos")]
    [SerializeField] private GameObject enemyPrefab;        // Prefab del enemigo a crear
    [SerializeField] private float spawnInterval = 2f;      // Cada cuántos segundos aparece un enemigo
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 5f); // Tamaño del área de spawn

    [Header("Configuración de Enemigos")]
    [SerializeField] private float minMoveSpeed = 1f;       // Velocidad mínima
    [SerializeField] private float maxMoveSpeed = 3f;       // Velocidad máxima

    // ========== ESTADO ==========
    private float timer; // Temporizador interno

    // ========== UPDATE (CADA FRAME) ==========
    private void Update()
    {
        // Incrementar temporizador
        timer += Time.deltaTime;

        // Cuando el temporizador llega al intervalo, generar enemigo
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f; // Resetear temporizador
        }
    }

    /// <summary>
    /// Crea un enemigo en una posición aleatoria del área
    /// </summary>
    private void SpawnEnemy()
    {
        // Calcular posición aleatoria dentro del área
        float randomX = Random.Range(-spawnAreaSize.x, spawnAreaSize.x) * 0.5f;
        float randomY = Random.Range(-spawnAreaSize.y, spawnAreaSize.y) * 0.5f;
        Vector2 randomOffset = new Vector2(randomX, randomY);

        // Posición final: centro del spawner + offset aleatorio
        Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

        // Crear el enemigo
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Configurar velocidad aleatoria
        EnemyAI ai = enemyObject.GetComponent<EnemyAI>();
        if (ai != null)
        {
            float randomSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
            ai.SetMoveSpeed(randomSpeed);
        }
    }

    // ========== GIZMOS (VISUALIZACIÓN EN EDITOR) ==========
    private void OnDrawGizmos()
    {
        // Dibujar el área de spawn en rojo
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}