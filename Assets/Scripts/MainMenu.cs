using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour {
    public void PlayGame() {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void Windowed() {
        if (Application.isMobilePlatform) return; // mobile cannot be windowed
        if (Screen.fullScreen) Screen.SetResolution(1920, 1080, false); // fullscreen -> windowed
        else Screen.SetResolution(1920, 1080, true); // windowed -> fullscreen
    }
}
