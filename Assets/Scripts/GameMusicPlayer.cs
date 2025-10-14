using UnityEngine;

public class GameMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip gameMusic;

    private void Start()
    {
        if (gameMusic != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(gameMusic, loop: true);
    }
}