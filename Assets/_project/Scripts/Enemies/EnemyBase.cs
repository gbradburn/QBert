using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] protected Animator _animator;
    [SerializeField] protected float _jumpDelay = 1f;
    
    protected float _delay;
    protected bool _jumping = false;
    protected bool _dead = false;
    protected Transform _transform;
    protected static readonly int Jumping = Animator.StringToHash("Jumping");
    protected static readonly int Landed = Animator.StringToHash("Landed");
    protected List<Vector3> _legalJumpLocations;
    public UnityEvent<EnemyBase> EnemyDied;

    protected virtual void Awake()
    {
        _transform = transform;
        EnemyDied = new UnityEvent<EnemyBase>();
        _legalJumpLocations = new List<Vector3>();
    }

    protected virtual void OnEnable()
    {
        GameManager.Instance.GameStateChanged.AddListener(OnGameStateChanged);
        _delay = _jumpDelay;
    }

    protected virtual void OnDisable()
    {
        GameManager.Instance.GameStateChanged.RemoveListener(OnGameStateChanged);
    }


    protected virtual void Update()
    {
        if (_jumping || _dead) return;
        _delay -= Time.deltaTime;
        if (_delay <= 0f)
        {
            Jump();
        }
    }

    protected virtual void OnGameStateChanged()
    {
        if (GameManager.Instance.GameState is not (GameManager.GameStates.LevelComplete
            or GameManager.GameStates.QBertDied)) return;
        DOTween.KillAll();
        gameObject.SetActive(false);
    }

    protected virtual void Jump()
    {
        _jumping = true;
        _animator.SetBool(Jumping, true);
        Vector3 location = GetRandomJumpLocation();
        if (BoardManager.LandingOutOfBounds(location))
        {
            _dead = true;
            ScoreManager.Instance.AddScore(500);
            location = LocationOffEdge(location);
            transform.DOJump(location, 25, 1, 1).SetEase(Ease.InSine).SetAutoKill();
            EnemyDied.Invoke(this);
            return;
        }
        transform.DOJump(location, 4, 1, 0.5f).SetEase(Ease.Linear).SetAutoKill();
    }

    protected virtual Vector3 LocationOffEdge(Vector3 location)
    {
        Vector3 offEdgeLocation = location;
        offEdgeLocation.y = -10;
        if (location.z > 20)
        {
            offEdgeLocation.z = 30;
        }
        else if (location.z < 0)
        {
            offEdgeLocation.z = -10;
        }
        else if (location.x < 0)
        {
            offEdgeLocation.x = -10;
        }
        else if (location.x > 18)
        {
            offEdgeLocation.x = 28;
        }
        else
        {
            if (_transform.eulerAngles == BoardManager.SouthEast)
            {
                offEdgeLocation.x += 10;
            }
            else
            {
                offEdgeLocation.z -= 10;
            }
        }

        return offEdgeLocation;
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (_jumping)
        {
            _jumping = false;
            _animator.SetBool(Jumping, false);
            _animator.SetTrigger(Landed);
            _delay = _jumpDelay;
        }

        if (!other.collider.TryGetComponent<QBert>(out var qBert)) return;
        _dead = true;
        qBert.KillQBert();
        EnemyDied.Invoke(this);
    }
    
    protected virtual Vector3 GetRandomJumpLocation()
    {
        Vector3 position = _transform.position;
        Vector3 facing = _transform.eulerAngles;


        int legalLocations = GameManager.Instance.BoardManager.LegalJumpLocations(_legalJumpLocations, position, position.y > 6);
        position = _legalJumpLocations[Random.Range(0, legalLocations)];
        
        return position;
    }
}
