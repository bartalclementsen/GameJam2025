using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this).Log("StartSceneHandler started");
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
}
