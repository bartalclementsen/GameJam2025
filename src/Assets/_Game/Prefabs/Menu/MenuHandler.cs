using System;
using _Game;
using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _finshedMenu;
    [SerializeField] private GameHandler _gameHandler;
    //[SerializeField] private InputFieldEditor _nameTextField;
    [SerializeField] private HighScoreHandler _highScoreHandler;
    [SerializeField] private TMP_InputField _nameTextField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var escapeClicked = Keyboard.current.escapeKey.wasPressedThisFrame;
        if(escapeClicked)
        {
            ShowPauseMenu();
        }
    }

    public void ShowPauseMenu()
    {
        Time.timeScale = 0;
        _gameHandler.Pause();
        _pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        Time.timeScale = 1;
        _gameHandler.Resume();
        _pauseMenu.SetActive(false);
    }

    public void ContinuePressed()
    {
        Time.timeScale = 1;
        _gameHandler.Resume();

        HidePauseMenu();
    }

    public void RetryClicked()
    {
        Time.timeScale = 1;
        _gameHandler.Resume();
        SceneManager.LoadScene(1);
    }

    public void QuitClicked()
    {
        Time.timeScale = 1;
        _gameHandler.Resume();
        SceneManager.LoadScene(0);
    }

    public void SaveHighScore()
    {
        var name = _nameTextField.text;
        if(string.IsNullOrWhiteSpace(name))
        {
            name = "Unknown";
        }

        Game.Container.Resolve<IHighScoreService>().AddHighScore(name, _highScoreHandler.GetScore(), 0);

        Time.timeScale = 1;
        _gameHandler.Resume();
        SceneManager.LoadScene(0);
    }

    internal void ShowFinishedMenu()
    {
        Time.timeScale = 0;
        _finshedMenu.SetActive(true);
    }
}
