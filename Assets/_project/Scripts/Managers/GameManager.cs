using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _qBertPrefab;
    [SerializeField] GameObject _squarePrefab;
    [SerializeField] AudioClip _victorySound, _gameOverSound;
    public static GameManager Instance;

    public enum GameStates
    {
        Ready,
        GameStarted,
        RoundStarted,
        LevelComplete,
        GameOver
    }

    GameStates _gameState;

    public GameStates GameState
    {
        get => _gameState;
        private set
        {
            _gameState = value;
            GameStateChanged.Invoke();
        }
    }
    
    List<IPlatform> _platforms;
    Transform _transform;
    QBert _qBert;
    int _lives;

    public int UnFlippedPlatforms => _platforms.Count(p => !p.Flipped);

    public int Lives
    {
        get => _lives;
        private set
        {
            _lives = value;
            LivesChanged.Invoke();
        } 
    }

    public UnityEvent LivesChanged = new UnityEvent();
    public UnityEvent GameStateChanged = new UnityEvent(); 

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
        _transform = transform;
    }

    void Start()
    {
        ReadyToPlay();
    }

    void Update()
    {
        if (GameState != GameStates.Ready) return;
        if (Input.anyKeyDown)
        {
            StartGame(); 
        }
    }

    void StartGame()
    {
        GameState = GameStates.GameStarted;
        StartRound();
    }
    
    void StartRound()
    {
        GameState = GameStates.RoundStarted;
        SpawnQBert();
        MusicManager.Instance.PlayGameMusic();
    }

    void SetUpBoard()
    {
        if (_platforms?.Count > 0)
        {
            ResetPlatforms();
            return;
        }
        _platforms = new List<IPlatform>();
        int blocksPerRow = 7;
        int y = 0;
        int startZ = 0;
        while (blocksPerRow > 0)
        {
            for (int i = 0; i < blocksPerRow; ++i)
            {
                GameObject square = Instantiate(_squarePrefab, _transform);
                square.transform.localPosition = new Vector3(i * 3, y, startZ + i * 3);
                _platforms.Add(square.GetComponentInChildren<IPlatform>());
            }

            --blocksPerRow;
            startZ += 3;
            y += 3;
        }
    }

    void ResetPlatforms()
    {
        if (UnFlippedPlatforms == _platforms.Count) return;
        foreach (var platform in _platforms.Where(platform => platform.Flipped))
        {
            platform.SetFlippedState(false);
        }
    }
    
    void SpawnQBert()
    {
        if (!_qBert)
        {
            _qBert = Instantiate(_qBertPrefab, _transform).GetComponent<QBert>();
        }

        _qBert.ResetQBert();
            
        --Lives;
    }

    public void PlatformFlipped()
    {
        if (UnFlippedPlatforms >= 1) return;
        _qBert.gameObject.SetActive(false);
        GameState = GameStates.LevelComplete;
        MusicManager.Instance.Stop();
        SoundManager.Instance.PlayAudioClip(_victorySound);
        ScoreManager.Instance.AddLevel();
        Invoke(nameof(StartRound), 9f);
    }

    public void QBertDied()
    {
        _qBert.gameObject.SetActive(false);
        if (Lives > 0)
        {
            StartRound();
        }
        else
        {
            GameOver();
        }
    }

    void GameOver()
    {
        SoundManager.Instance.PlayAudioClip(_gameOverSound);
        GameState = GameStates.GameOver;
        Invoke(nameof(ReadyToPlay), 3f);
    }

    void ReadyToPlay()
    {
        MusicManager.Instance.PlayIntroMusic();
        Lives = 3;
        ScoreManager.Instance.ResetScore();
        SetUpBoard();
        GameState = GameStates.Ready;
    }
}
