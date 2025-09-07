using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneHandler : MonoBehaviour
{
    [SerializeField] GameObject _mainMenu;
    [SerializeField] GameObject _highScoreMenu;
    [SerializeField] TextMeshProUGUI _highScoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this).Log("StartSceneHandler started");
        var _highScoreService = Game.Container.Resolve<_Game.IHighScoreService>();

        var highScores = _highScoreService.GetHighScores();
        StringBuilder sb = new StringBuilder();
        if (highScores.Any() == false)
        {
            sb.AppendLine("No high scores");
        }
        else
        {
            for (int i = 0; i < highScores.Count(); i++)
            {
                var highScore = highScores.ElementAt(i);
                sb.AppendLine($"{(i + 1).ToString("00")}. {highScore.Name} Percent {highScore.Percent}, Score {highScore.Score} ({highScore.Date}) ");
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
}
