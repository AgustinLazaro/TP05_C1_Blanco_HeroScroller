using UnityEngine;

/// <summary>
/// Reproduce la música asignada al iniciar la escena.
/// </summary>
public class GameMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip gameMusic;

    private void Start()
    {
        if (gameMusic != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(gameMusic, true);
    }
}