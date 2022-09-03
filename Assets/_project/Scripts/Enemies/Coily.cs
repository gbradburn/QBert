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
        _delay = _jumpDelay;
        _egg.SetActive(true);
        _snake.SetActive(false);
    }

    void Update()
    {
        if (_jumping) return;
        _delay -= Time.deltaTime;
        if (_delay <= 0f)
        {
            Jump();
        }
    }

    void Jump()
    {
        _jumping = true;
        _animator.SetBool(Jumping, true);
        Vector3 location = GetRandomJumpLocation();
        transform.DOJump(location, 4, 1, 0.5f).SetEase(Ease.Linear).SetAutoKill();
    }

    void OnCollisionEnter(Collision other)
    {
        if (!_jumping) return;
        _jumping = false;
        _animator.SetBool(Jumping, false);
        _animator.SetTrigger(Landed);
        if (other.collider.TryGetComponent<QBert>(out var qBert))
        {
            Debug.Log($"QBert collided with Coily!");
            qBert.KillQBert();
            CoilyDied.Invoke();
            return;
        }
        _delay = _jumpDelay;
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
