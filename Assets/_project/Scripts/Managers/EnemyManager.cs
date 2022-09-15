using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] GameObject _coilyPrefab;
    [SerializeField] float _spawnDelay = 3f;

    List<GameObject> _enemies;
    Coily _coily;
    float _delay;

    bool ShouldSpawnMonster
    {
        get
        {
            _delay -= Time.deltaTime;
            return _delay <= 0f;
        }
    }

    bool ShouldSpawnCoily => GameManager.Instance.IsPlaying && !_coily;

    void Awake()
    {
        _enemies = new List<GameObject>();
    }

    void OnEnable()
    {
        GameManager.Instance.GameStateChanged.AddListener(OnGameStateChanged);
    }

    void OnDisable()
    {
        GameManager.Instance.GameStateChanged.RemoveListener(OnGameStateChanged);
    }

    void Update()
    {
        if (!ShouldSpawnMonster) return;
        SpawnMonster();
    }

    void OnGameStateChanged()
    {
        switch (GameManager.Instance.GameState)
        {
            case GameManager.GameStates.RoundStarted:
                _delay = _spawnDelay;
                break;
            case GameManager.GameStates.QBertDied:
            case GameManager.GameStates.LevelComplete:
            case GameManager.GameStates.GameOver: 
                DestroyAllEnemies();
                break;
        }
    }

    void DestroyAllEnemies()
    {
        Debug.Log($"DestroyAllEnemies()");
        if (_coily)
        {
            DestroyCoily();
        }

        foreach (var enemy in _enemies)
        {
            Destroy(enemy);
        }

        _enemies.Clear();
    }

    void SpawnMonster()
    {
        if (ShouldSpawnCoily)
        {
            _coily = Instantiate(_coilyPrefab, transform).GetComponent<Coily>();
            _coily.CoilyDied.AddListener(OnCoilyDied);
        }

        _delay = _spawnDelay;
    }

    void OnCoilyDied()
    {
        DestroyCoily();
    }

    void DestroyCoily()
    {
        Debug.Log("DestroyCoily()");
        if (!_coily) return;
        Debug.Log("removing coily listener and destroying coily gameobject.");
        _coily.CoilyDied.RemoveListener(OnCoilyDied);
        Destroy(_coily.gameObject, 2f);
        _coily = null;
    }
}
