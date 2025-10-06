using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip pickableSound;
    [SerializeField] private AudioSource audioSource;

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}