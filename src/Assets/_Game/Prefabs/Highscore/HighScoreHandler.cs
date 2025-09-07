using TMPro;
using UnityEngine;

public class HighScoreHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    private TextMeshProUGUI _textMeshPro;

    private int _increments = 0;
    private int _score = 0;

    private void Update()
    {
        _textMeshPro.SetText($"Score: {GetScore()}%");
    }

    public void AddScore()
    {
        _increments++;
        _score += 1;
    }

    public void RemoveScore()
    {
        _increments++;
        _score -= 1;

        if (_score < 0)
        {
            _score = 0;
        }
    }

    public int GetScore()
    {
        return (int)((Mathf.Min(Mathf.Max(0f, (float)_score / (float)_increments), 1f) * 100) + 0.5f);
    }
}