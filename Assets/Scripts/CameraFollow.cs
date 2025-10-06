using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float offsetY = 2f;
    [SerializeField] private float offsetZ = -10f;

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, offsetY, offsetZ);
        }
    }
}