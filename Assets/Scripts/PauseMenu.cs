using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject settingsMenu;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        StartCoroutine(WaitAndResume());
    }

    private IEnumerator WaitAndResume()
    {
        countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1);
        }
        countdownText.gameObject.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void HomeButton()
    {
        SceneManager.LoadSceneAsync(0);
        Time.timeScale = 1;
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
    }
}
