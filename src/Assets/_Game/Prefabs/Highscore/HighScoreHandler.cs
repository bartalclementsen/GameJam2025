using TMPro;
using UnityEngine;

public class HighScoreHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    private TextMeshProUGUI _textMeshPro;

    private int _increments = 0;
    private int _score = 0;

    public void AddScore()
    {
        _increments++;
        _score += 1;
        _textMeshPro.SetText($"Score: {GetScore()}%");
    }

    public void RemoveScore()
    {
        _increments++;
        _score -= 1;

        if (_score < 0)
        {
            _score = 0;
        }

        _textMeshPro.SetText($"Score: {GetScore()}%");
    }

    public int GetScore()
    {
        if (_increments == 0)
        {
            return 0;
        }

        return (int)((Mathf.Min(Mathf.Max(0f, (float)_score / (float)_increments), 1f) * 100) + 0.5f);
    }
}