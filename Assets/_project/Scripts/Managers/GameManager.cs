using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public enum GameStates
    {
        Ready,
        GameStarted,
        RoundStarted,
        QBertDied,
        LevelComplete,
        GameOver
    }
    
    [SerializeField] GameObject _qBertPrefab;
    [SerializeField] Vector3 _qBertStartPosition, _qBertStartRotation;
    [SerializeField] AudioClip _victorySound, _gameOverSound;
    GameStates _gameState;
    BoardManager _boardManager;
    Transform _transform;
    QBert _qBert;
    int _lives;
    Vector3 _qBertSpawnPosition, _qBertSpawnRotation;

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

    public bool IsPlaying => GameState == GameStates.RoundStarted;
    public BoardManager BoardManager => _boardManager;
    public Transform Qbert => _qBert.transform;

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
        Lives = 3;
        StartRound();
    }
    
    void StartRound()
    {
        SpawnQBert();
        GameState = GameStates.RoundStarted;
        MusicManager.Instance.PlayGameMusic();
    }

    void SpawnQBert()
    {
        if (!_qBert)
        {
            _qBert = Instantiate(_qBertPrefab, _transform).GetComponent<QBert>();
        }

        _qBert.ResetQBert(_qBertSpawnPosition, _qBertSpawnRotation);
            
        --Lives;
    }

    public void PlatformFlipped()
    {
        if (_boardManager.UnFlippedPlatforms >= 1) return;
        StartCoroutine(NextLevel());
    }

    IEnumerator NextLevel()
    {
        _qBert.gameObject.SetActive(false);
        GameState = GameStates.LevelComplete;
        MusicManager.Instance.Stop();
        SoundManager.Instance.PlayAudioClip(_victorySound);
        _boardManager.ShowVictoryEffect();
        yield return new WaitForSeconds(9f);
        ScoreManager.Instance.AddLevel();
        _boardManager.SetUpBoard();
        StartRound();
    }

    public void QBertDied()
    {
        GameState = GameStates.QBertDied;
        StartCoroutine(HandleQBertDeath());
    }

    IEnumerator HandleQBertDeath()
    {
        yield return new WaitForSeconds(3f);
        _qBertSpawnPosition = _qBert.transform.position;
        _qBertSpawnRotation = _qBert.Body.eulerAngles;
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
        SoundManager.Instance.PlayAudioClip(_gameOverSound, 0.25f);
        GameState = GameStates.GameOver;
        Invoke(nameof(ReadyToPlay), 3f);
    }

    void ReadyToPlay()
    {
        MusicManager.Instance.PlayIntroMusic();
        Lives = 3;
        _qBertSpawnPosition = _qBertStartPosition;
        _qBertSpawnRotation = _qBertStartRotation;
        ScoreManager.Instance.ResetScore();
        _boardManager.SetUpBoard();
        GameState = GameStates.Ready;
    }
}
