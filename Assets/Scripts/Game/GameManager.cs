using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;

    private int _score;

    public int Score => _score;

    private void Awake()
    {
        _score = 0;
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        _score += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (_scoreText == null)
            return;

        _scoreText.text = $"SCORE: {_score}";
    }
}
