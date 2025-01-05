using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Windowed()
    {
        // if current screen mode is fullscreen, change to windowed
        if (Screen.fullScreen)
        {
            Screen.SetResolution(1920, 1080, false);
        }
        // if current screen mode is windowed, change to fullscreen
        else
        {
            Screen.SetResolution(1920, 1080, true);
        }
        
    }
}
