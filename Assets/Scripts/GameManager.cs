using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int totalCoins = 0;
    [SerializeField] private int totalEnemies = 0;

    private int currentScore = 0;
    private int enemiesKilled = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int value)
    {
        currentScore += value;
        CheckVictoryCondition();
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        if (currentScore >= totalCoins && enemiesKilled >= totalEnemies)
        {
            Debug.Log("¡Victoria! Has completado el nivel.");
        }
    }
}