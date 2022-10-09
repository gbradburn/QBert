using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] GameObject _coilyPrefab;
    [SerializeField] GameObject[] _enemyPrefabs;
    [SerializeField] float _spawnDelay = 3f;

    List<GameObject> _enemies;
    Coily _coily;
    float _delay;

    bool ShouldSpawnMonster
    {
        get
        {
            if (!GameManager.Instance.IsPlaying) return false;
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
            _coily.EnemyDied.AddListener(OnCoilyDied);
        }
        else
        {
            var prefab = GetRandomEnemyPrefab();
            var enemy = Instantiate(prefab, transform);
            enemy.GetComponent<EnemyBase>().EnemyDied.AddListener(OnEnemyDied);
            _enemies.Add(enemy);
        }

        _delay = _spawnDelay;
    }

    GameObject GetRandomEnemyPrefab()
    {
        return _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
    }

    void OnEnemyDied(EnemyBase enemy)
    {
        _enemies.Remove(enemy.gameObject);
        enemy.EnemyDied.RemoveListener(OnEnemyDied);
        Destroy(enemy.gameObject, 3f);
    }

    void OnCoilyDied(EnemyBase enemy)
    {
        DestroyAllEnemies();
    }

    void DestroyCoily()
    {
        if (!_coily) return;
        _coily.EnemyDied.RemoveListener(OnCoilyDied);
        Destroy(_coily.gameObject, 2f);
        _coily = null;
        _delay = _spawnDelay;
    }
}
