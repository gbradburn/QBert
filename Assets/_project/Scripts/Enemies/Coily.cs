using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Coily : MonoBehaviour
{
    [SerializeField] Animator _animator;
    [SerializeField] GameObject _egg, _snake;
    [SerializeField] float _jumpDelay = 0.5f;
    float _delay;
    bool _jumping = false;
    bool _dead = false;
    Transform _transform;
    static readonly int Jumping = Animator.StringToHash("Jumping");
    static readonly int Landed = Animator.StringToHash("Landed");
    List<Vector3> _legalJumpLocations;
    public UnityEvent CoilyDied;

    void Awake()
    {
        _transform = transform;
        CoilyDied = new UnityEvent();
        _legalJumpLocations = new List<Vector3>();
    }

    void OnEnable()
    {
        GameManager.Instance.GameStateChanged.AddListener(OnGameStateChanged);
        _delay = _jumpDelay;
        _egg.SetActive(true);
        _snake.SetActive(false);
    }

    void OnDisable()
    {
        GameManager.Instance.GameStateChanged.RemoveListener(OnGameStateChanged);
    }


    void Update()
    {
        if (_jumping || _dead) return;
        _delay -= Time.deltaTime;
        if (_delay <= 0f)
        {
            Jump();
        }
    }

    void OnGameStateChanged()
    {
        if (GameManager.Instance.GameState is not (GameManager.GameStates.LevelComplete
            or GameManager.GameStates.QBertDied)) return;
        DOTween.KillAll();
        gameObject.SetActive(false);
    }

    void Jump()
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
            CoilyDied.Invoke();
            return;
        }
        transform.DOJump(location, 4, 1, 0.5f).SetEase(Ease.Linear).SetAutoKill();
    }

    Vector3 LocationOffEdge(Vector3 location)
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

    void OnCollisionEnter(Collision other)
    {
        if (_jumping)
        {
            _jumping = false;
            _animator.SetBool(Jumping, false);
            _animator.SetTrigger(Landed);
            _delay = _jumpDelay;
        }

        if (other.collider.TryGetComponent<QBert>(out var qBert))
        {
            _dead = true;
            qBert.KillQBert();
            CoilyDied.Invoke();
            return;
        }

        if (!(_transform.position.y < 4) || !_egg.activeSelf) return;
        _egg.SetActive(false);
        _snake.SetActive(true);
    }
    
    Vector3 GetRandomJumpLocation()
    {
        Vector3 position = _transform.position;
        Vector3 facing = _transform.eulerAngles;
        
        // Egg always jumps down
        if (_egg.activeSelf)
        {
            int locations = GameManager.Instance.BoardManager.LegalJumpLocations(_legalJumpLocations, _transform.position);
            return _legalJumpLocations[Random.Range(0, locations)];
        }
        
        // Snake always jumps toward player (unless disc is activated)
        int yOffset = GameManager.Instance.Qbert.position.y < position.y ? -3 : 3;
        position.y += yOffset;
        switch (yOffset)
        {
            // Q*Bert is below and to the left of snake
            case < 0 when GameManager.Instance.Qbert.position.z < position.z:
                position.z -= 3;
                facing = BoardManager.SouthWest;
                break;
            // Q*Bert is below and to the right 
            case < 0:
                position.x += 3;
                facing = BoardManager.SouthEast;
                break;
            // Q*Bert if above and to the right of snake
            case > 0 when GameManager.Instance.Qbert.position.z > position.z:
                position.z += 3;
                facing = BoardManager.NorthEast;
                break;
            // Q*Bert is above and to the left of snake
            case > 0:
                position.x -= 3;
                facing = BoardManager.NorthWest;
                break;
            case 0:
                int legalLocations = GameManager.Instance.BoardManager.LegalJumpLocations(_legalJumpLocations,
                    _transform.position, _transform.position.y > 6);
                position = _legalJumpLocations[Random.Range(0, legalLocations)];
                break;
        }
        transform.DORotate(facing, 0.25f).SetEase(Ease.Linear).SetAutoKill();
        
        return position;
    }

 
}
