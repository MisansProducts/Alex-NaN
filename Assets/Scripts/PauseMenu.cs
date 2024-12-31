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
        SoundEffects.Instance.PlaySound(SoundEffects.Instance.select);
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
    }

    public void ResumeGame()
    {
        SoundEffects.Instance.PlaySound(SoundEffects.Instance.select);
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
            SoundEffects.Instance.PlaySound(SoundEffects.Instance.select);
        }
        countdownText.gameObject.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void HomeButton()
    {
        SoundEffects.Instance.PlaySound(SoundEffects.Instance.select);
        SceneManager.LoadSceneAsync(0);
        Time.timeScale = 1;
    }

    public void OpenSettings()
    {
        SoundEffects.Instance.PlaySound(SoundEffects.Instance.select);
        settingsMenu.SetActive(true);
    }
}
