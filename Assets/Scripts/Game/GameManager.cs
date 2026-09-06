using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TMP_Text _finalScoreText;

    private int _score;
    private bool _isGameOver;

    public int Score => _score;
    public bool IsGameOver => _isGameOver;

    private void Awake()
    {
        _score = 0;
        _isGameOver = false;

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);

        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        _score += amount;
        UpdateScoreText();
    }

    public void GameOver()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;

        if (_finalScoreText != null)
            _finalScoreText.text = $"SCORE: {_score}";

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(true);
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateScoreText()
    {
        if (_scoreText == null)
            return;

        _scoreText.text = $"SCORE: {_score}";
    }
}
