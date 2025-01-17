using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    [SerializeField] public Button ezMode;
    [SerializeField] public Button normalMode;
    private GameSetting gameSetting;
    
    public void ShowDifficulty() {
        ezMode.gameObject.SetActive(true);
        normalMode.gameObject.SetActive(true);
    }

    public void StartNormalGame() {
        gameSetting.spikeChance = 0.3f;
        gameSetting.spikeCoolDown = 2;
        SceneManager.LoadScene(1);
    }

    public void StartBabyGame() {
        gameSetting.spikeChance = 0.2f;
        gameSetting.spikeCoolDown = 3;
        SceneManager.LoadScene(1);
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void Windowed() {
        if (Application.isMobilePlatform) return; // mobile cannot be windowed
        if (Screen.fullScreen) Screen.SetResolution(1920, 1080, false); // fullscreen -> windowed
        else Screen.SetResolution(1920, 1080, true); // windowed -> fullscreen
    }

    void Awake()
    {
        gameSetting = GameSetting.Instance;
    }
}
