using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] GameObject[] _playerLives;
    [SerializeField] TMP_Text _levelText, _scoreText, _highScoreText;
    [SerializeField] TMP_Text _levelCompleteText, _gameOverText, _playAgainText;

    bool _waitingForKeyPress;
    
    void OnEnable()
    {
        ScoreManager.Instance.ScoreChanged.AddListener(OnScoreChanged);
        GameManager.Instance.LivesChanged.AddListener(OnLivesChanged);
        GameManager.Instance.GameStateChanged.AddListener(OnGameStateChanged);
        OnScoreChanged();
    }

    void OnDisable()
    {
        ScoreManager.Instance.ScoreChanged.RemoveListener(OnScoreChanged);
        GameManager.Instance.LivesChanged.RemoveListener(OnLivesChanged);
        GameManager.Instance.GameStateChanged.RemoveListener(OnGameStateChanged);
    }

    void OnGameStateChanged()
    {
        switch (GameManager.Instance.GameState)
        {
            case GameManager.GameStates.GameStarted:
                _levelText.gameObject.SetActive(false);
                _levelCompleteText.gameObject.SetActive(false);
                _gameOverText.gameObject.SetActive(false);
                _playAgainText.gameObject.SetActive(false);
                break;
            case GameManager.GameStates.RoundStarted:
                _levelCompleteText.gameObject.SetActive(false);
                _levelText.gameObject.SetActive(true);
                _gameOverText.gameObject.SetActive(false);
                _playAgainText.gameObject.SetActive(false);
                break;
            case GameManager.GameStates.LevelComplete:
                _levelText.gameObject.SetActive(false);
                _levelCompleteText.text = $"Level {ScoreManager.Instance.Level} Complete";
                _levelCompleteText.gameObject.SetActive(true);
                _gameOverText.gameObject.SetActive(false);
                _playAgainText.gameObject.SetActive(false);
                break;
            case GameManager.GameStates.GameOver:
                _levelText.gameObject.SetActive(false);
                _levelCompleteText.gameObject.SetActive(false);
                _gameOverText.gameObject.SetActive(true);
                _playAgainText.gameObject.SetActive(false);
                break;
            case GameManager.GameStates.Ready:
                _levelText.gameObject.SetActive(false);
                _levelCompleteText.gameObject.SetActive(false);
                _gameOverText.gameObject.SetActive(false);
                _playAgainText.gameObject.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OnLivesChanged()
    {
        for (int i = 0; i < _playerLives.Length; ++i)
        {
            _playerLives[i].SetActive(i < GameManager.Instance.Lives);
        }
    }

    void OnScoreChanged()
    {
        _levelText.text = $"Level {ScoreManager.Instance.Level}";
        _scoreText.text = ScoreManager.Instance.Score.ToString();
        _highScoreText.text = ScoreManager.Instance.HiScore.ToString();
    }
    
}
