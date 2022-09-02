using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] AudioClip _introMusic, _gameMusic;
    
    AudioSource _audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    public void PlayIntroMusic()
    {
        _audioSource.Stop();
        _audioSource.PlayOneShot(_introMusic, .25f);
    }

    public void PlayGameMusic()
    {
        _audioSource.clip = _gameMusic;
        _audioSource.volume = 1f;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void Stop()
    {
        _audioSource.Stop();
    }
}
