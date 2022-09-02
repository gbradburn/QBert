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
        SetPlatformMaterial();
        if (flipped)
        {
            SoundManager.Instance.PlayAudioClip(_flipSound, 0.25f);
            ScoreManager.Instance.AddScore(25);
        }
        GameManager.Instance.PlatformFlipped();
    }

    public void SetPlatformColor(Color color)
    {
        _platformRenderer.material.color = color;
    }

    void SetPlatformMaterial()
    {
        _platformRenderer.material = Flipped ? _platformFlipped : _platformNormal;
    }
}
