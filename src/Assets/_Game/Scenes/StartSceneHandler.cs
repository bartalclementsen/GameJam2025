using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneHandler : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _highScoreMenu;
    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private TextMeshProUGUI _highScoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this).Log("StartSceneHandler started");
        _Game.IHighScoreService _highScoreService = Game.Container.Resolve<_Game.IHighScoreService>();

        System.Collections.Generic.IEnumerable<_Game.HighScore> highScores = _highScoreService.GetHighScores();
        StringBuilder sb = new();
        if (highScores.Any() == false)
        {
            sb.AppendLine("No high scores");
        }
        else
        {
            for (int i = 0; i < highScores.Count(); i++)
            {
                _Game.HighScore highScore = highScores.ElementAt(i);
                sb.AppendLine($"{i + 1:00}. {highScore.Name} Percent {highScore.Percent}, Score {highScore.Score} ({highScore.Date}) ");
            }
        }

        _highScoreText.text = sb.ToString();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void StartButtonClicked()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitButtonClicked()
    {
        Application.Quit();
    }

    public void ShowHighScore()
    {
        _mainMenu.SetActive(false);
        _highScoreMenu.SetActive(true);
    }

    public void BackFromHighScore()
    {
        _mainMenu.SetActive(true);
        _highScoreMenu.SetActive(false);
    }

    public void ShowInfo()
    {
        _mainMenu.SetActive(false);
        _infoPanel.SetActive(true);
    }

    public void BackFromInfo()
    {
        _mainMenu.SetActive(true);
        _infoPanel.SetActive(false);
    }
}
