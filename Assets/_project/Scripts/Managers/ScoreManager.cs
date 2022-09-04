using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int Score { get; private set; }
    public int HiScore { get; private set; }
    public int Level { get; private set; }
    public int BonusPoints { get; private set; }
    
    public UnityEvent ScoreChanged = new UnityEvent();

    int _unusedTransportDiscs;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    void OnEnable()
    {
        HiScore = PlayerPrefs.GetInt("HighScore", 0);
        Score = 0;
        Level = 1;
        ScoreChanged.Invoke();
    }

    public void AddScore(int points)
    {
        Score += points;
        ScoreChanged.Invoke();
        if (Score <= HiScore) return;
        HiScore = Score;
        PlayerPrefs.SetInt("HighScore", HiScore);
    }

    public void ResetScore()
    {
        Score = 0;
        _unusedTransportDiscs = 2;
        ScoreChanged.Invoke();
    }

    public void AddLevel()
    {
        CalculateBonusPoints();
        Score += BonusPoints;
        Level += 1;
        _unusedTransportDiscs = 2;
        ScoreChanged.Invoke();
    }

    public void UseTransportDisc()
    {
        _unusedTransportDiscs--;
    }

    int LevelBonus => (Level - 1) * 250;
    int TransportDiscBonus => _unusedTransportDiscs * 50;
    void CalculateBonusPoints()
    {
        BonusPoints = 1000 + LevelBonus + TransportDiscBonus;
    }
}
