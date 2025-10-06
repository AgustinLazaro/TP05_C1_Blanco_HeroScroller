using UnityEngine;

public class Pickable : MonoBehaviour
{
    public enum PickableType
    {
        Coin,
        HealthPowerUp
    }

    [SerializeField] private PickableType type;
    [SerializeField] private int value = 10;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            switch (type)
            {
                case PickableType.Coin:
                    GameManager.Instance.AddScore(value);
                    Debug.Log($"Recogiste {value} moneda(s).");
                    break;

                case PickableType.HealthPowerUp:
                    collision.gameObject.GetComponent<PlayerHealth>().RestoreHealth(value);
                    Debug.Log($"Recogiste un power-up que restauró {value}% de tu salud.");
                    break;
            }

            Destroy(gameObject);
        }
    }
}