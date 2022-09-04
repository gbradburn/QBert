using DG.Tweening;
using UnityEngine;

public class QBert : MonoBehaviour
{
    [SerializeField]
    Animator _animator;

    [SerializeField] AudioClip _jumpSound, _landSound, _fallSound, _deathSound;
    [SerializeField] GameObject _speechBubble;

    static readonly int JumpTrigger = Animator.StringToHash("Jump");
    static readonly int LandTrigger = Animator.StringToHash("Land");
    bool _jumping = false, _dead = false;
    Transform _transform;
    Rigidbody _rigidbody;
    Vector3 _startPosition;

    BoardManager BoardManager => GameManager.Instance.BoardManager;
    
    public Transform Body { get; private set; }
    
    void Awake()
    {
        _transform = transform;
        Body = _transform.GetChild(0);
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying || _dead || _jumping) return;

        if (!ReceivedPlayerInput(out var desiredFacing, out var landingPosition)) return;
        
        ChangeFacing(desiredFacing);
        
        if (BoardManager.LandingOutOfBounds(landingPosition))
        {
            if (JumpedToDisc(landingPosition))
            {
                return;
            }
            JumpOffEdge(landingPosition);
            return;
        }
            
        PerformJump(landingPosition);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_jumping) return;
        TriggerLandingAnimation();
        if (!CollidedWithPlatform(collision, out var platform)) return;
        _jumping = false;
        if (!platform.Flipped)
        {
            platform.SetFlippedState(true);
            return;
        }
        SoundManager.Instance.PlayAudioClip(_landSound);
    }

    public void ResetQBert(Vector3 spawnPosition, Vector3 spawnRotation)
    {
        _jumping = _dead = false;
        _speechBubble.SetActive(false);
        _animator.StartPlayback();
        _startPosition = _transform.position = spawnPosition;
        Body.eulerAngles = spawnRotation;
        _rigidbody.velocity = _rigidbody.angularVelocity = Vector3.zero;
        gameObject.SetActive(true);
    }

    bool ReceivedPlayerInput(out Vector3 desiredFacing, out Vector3 landingPosition)
    {
        landingPosition = _transform.position;
        desiredFacing = BoardManager.NoChange;
        
        // up and right (NorthEast)
        if (Input.GetKey(KeyCode.E))
        {
            desiredFacing = BoardManager.NorthEast;
            landingPosition.y += 3;
            landingPosition.z += 3;
        }
        
        // up and left (NorthWest)
        else if (Input.GetKey(KeyCode.Q))
        {
            desiredFacing = BoardManager.NorthWest;
            landingPosition.y += 3;
            landingPosition.x -= 3;
        }
        
        // Down and right (SouthEast)
        else if (Input.GetKey(KeyCode.C))
        {
            desiredFacing = BoardManager.SouthEast;
            landingPosition.y -= 3;
            landingPosition.x += 3;
        }
        
        // Down and left (SouthWest)
        else if (Input.GetKey(KeyCode.Z))
        {
            desiredFacing = BoardManager.SouthWest;
            landingPosition.y -= 3;
            landingPosition.z -= 3;
        }

        return landingPosition != _transform.position;
    }

    void JumpOffEdge(Vector3 targetPosition)
    {
        MusicManager.Instance.Stop();
        SoundManager.Instance.PlayAudioClip(_fallSound, 0.25f);
        Vector3 fallPosition = targetPosition;
        fallPosition.y = -10;
        if (targetPosition.z > 20)
        {
            fallPosition.z = 30;
        }
        else if (targetPosition.z < 0)
        {
            fallPosition.z = -10;
        }
        else if (targetPosition.x < 0)
        {
            fallPosition.x = -10;
        }
        else if (targetPosition.x > 18)
        {
            fallPosition.x = 28;
        }
        else
        {
            if (Body.eulerAngles == BoardManager.SouthEast)
            {
                fallPosition.x += 10;
            }
            else
            {
                fallPosition.z -= 10;
            }
        }

        _transform.DOJump(fallPosition, 25, 1, 1).SetEase(Ease.InSine);
        _dead = true;
        GameManager.Instance.QBertDied();
    }
    
    void TriggerLandingAnimation()
    {
        _animator.SetBool(JumpTrigger, false);
        _animator.SetTrigger(LandTrigger);
    }

    bool CollidedWithPlatform(Collision collision, out IPlatform platform)
    {
        return collision.collider.gameObject.TryGetComponent<IPlatform>(out platform);
    }

    void PlayJumpSound()
    {
        SoundManager.Instance.PlayAudioClip(_jumpSound);
    }
    
    bool JumpedToDisc(Vector3 landingPosition)
    {
        Vector3 discPosition = landingPosition;
        discPosition.y -= 3;
        var disc = BoardManager.TransportDiscAtPosition(discPosition);
        if (!disc) return false;
        PerformJump(disc.transform.position);
        return true;
    }
    
    void ChangeFacing(Vector3 desiredFacing)
    {
        if (desiredFacing != BoardManager.NoChange)
        {
            Body.DORotate(desiredFacing, 0.25f).SetAutoKill();
        }
    }
    
    void PerformJump(Vector3 landingPosition)
    {
        _jumping = true;
        _animator.SetBool(JumpTrigger, true);
        PlayJumpSound();
        _transform.DOJump(landingPosition, 4, 1, 0.5f, false).SetEase(Ease.Linear).SetAutoKill();
    }

    public void JumpToPlatform()
    {
        _transform.SetParent(null);
        ChangeFacing(_transform.position.x < 0 ? BoardManager.SouthEast : BoardManager.SouthWest);
        PerformJump(_startPosition);
    }
    

    public void LandedOnDisc(bool left)
    {
        TriggerLandingAnimation();
        ChangeFacing(left ? BoardManager.NorthEast : BoardManager.NorthWest);
    }

    public void KillQBert()
    {
        if (_dead) return;
        _dead = true;
        MusicManager.Instance.Stop();
        SoundManager.Instance.PlayAudioClip(_deathSound);
        _animator.StopPlayback();
        _speechBubble.SetActive(true);
        GameManager.Instance.QBertDied();
    }
}
