using TMPro;
using UnityEngine;

public class HighScoreHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    private TextMeshProUGUI _textMeshPro;

    private int _increments = 0;
    private int _score = 0;

    private int errorsAdded;

    private int errorsFixed;

    private int _initialErros;

    public void SetInitialErrors(int initialErrors)
    {
        _initialErros = initialErrors;
        UpdateUI();
    }

    public void AddScore()
    {
        errorsFixed++;

        _increments++;
        _score += 1;

        UpdateUI();
    }

    public void RemoveScore()
    {
        errorsAdded++;

        _increments++;
        _score -= 1;

        if (_score < 0)
        {
            _score = 0;
        }

        UpdateUI();
    }

    public int GetScore()
    {
        var percent = (float)errorsFixed / (_initialErros + errorsAdded) * 100;

        return (int)Mathf.Round(percent);

        if (_increments == 0)
        {
            return 0;
        }

        return _initialErros;

        //return (int)((Mathf.Min(Mathf.Max(0f, (float)_score / (float)_increments), 1f) * 100) + 0.5f);
    }

    private void UpdateUI()
    {
        _textMeshPro.SetText($"Score: {GetScore()}%");
    }
}