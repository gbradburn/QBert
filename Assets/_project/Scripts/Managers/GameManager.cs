using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public enum GameStates
    {
        Ready,
        GameStarted,
        RoundStarted,
        LevelComplete,
        GameOver
    }
    
    [SerializeField] GameObject _qBertPrefab;
    [SerializeField] AudioClip _victorySound, _gameOverSound;
    GameStates _gameState;
    BoardManager _boardManager;
    Transform _transform;
    QBert _qBert;
    int _lives;

    public static GameManager Instance;
    public GameStates GameState
    {
        get => _gameState;
        private set
        {
            _gameState = value;
            GameStateChanged.Invoke();
        }
    }
    public BoardManager BoardManager => _boardManager;


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
        _boardManager = GetComponent<BoardManager>();
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
        if (_boardManager.UnFlippedPlatforms >= 1) return;
        NextLevel();
    }

    void NextLevel()
    {
        _qBert.gameObject.SetActive(false);
        GameState = GameStates.LevelComplete;
        MusicManager.Instance.Stop();
        SoundManager.Instance.PlayAudioClip(_victorySound);
        ScoreManager.Instance.AddLevel();
        _boardManager.SetUpBoard();
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
        _boardManager.SetUpBoard();
        GameState = GameStates.Ready;
    }
}
