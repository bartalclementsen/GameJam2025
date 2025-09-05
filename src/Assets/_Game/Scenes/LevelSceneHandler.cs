using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject _gamePanel;

    [SerializeField]
    private GameObject _winPanel;

    [SerializeField]
    private GameObject _losePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WinButtonClicked()
    {
        _gamePanel.SetActive(false);
        _winPanel.SetActive(true);
    }

    public void LoseButtonClicked()
    {
        _gamePanel.SetActive(false);
        _losePanel.SetActive(true);
    }

    public void TryAgainButtonClicked()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitButtonClicked()
    {
        SceneManager.LoadScene(0);
    }
}
