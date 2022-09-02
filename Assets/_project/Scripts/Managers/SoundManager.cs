using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    AudioSource _audioSource;

    void Awake()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioClip(AudioClip clip, float volume = 1f)
    {
        //_audioSource.PlayOneShot(clip, volume);
    }
}
