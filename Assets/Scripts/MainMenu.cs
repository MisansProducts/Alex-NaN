using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void OpenSettings()
    {
        SceneManager.LoadSceneAsync("SettingMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
