using UnityEngine;

public class Platform : MonoBehaviour, IPlatform
{
    [SerializeField] Material _platformNormal;
    [SerializeField] Material _platformFlipped;
    [SerializeField] Renderer _platformRenderer;
    [SerializeField] AudioClip _flipSound, _platformResetSound;
    
    public bool Flipped { get; private set; }

    public void SetFlippedState(bool flipped)
    {
        Flipped = flipped;
        SetPlatformColor();
        if (flipped)
        {
            SoundManager.Instance.PlayAudioClip(_flipSound, 0.25f);
            ScoreManager.Instance.AddScore(25);
        }
        GameManager.Instance.PlatformFlipped();
    }

    void SetPlatformColor()
    {
        _platformRenderer.material = Flipped ? _platformFlipped : _platformNormal;
    }
}
