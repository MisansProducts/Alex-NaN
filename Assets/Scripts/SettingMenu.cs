using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{

    private BackgroundMusic backgroundMusic;
    private SoundEffects soundEffects;
    [SerializeField] private Slider volumeSliderBGM;
    [SerializeField] private Slider volumeSliderSoundEffects;
    [SerializeField] private GameObject settingMenu;


    void Start()
    {
        backgroundMusic = FindObjectOfType<BackgroundMusic>();
        soundEffects = FindObjectOfType<SoundEffects>();
        volumeSliderBGM.value = backgroundMusic.getVolume();
        volumeSliderSoundEffects.value = soundEffects.getVolume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToPauseMenu();
        }
    } 

    public void AdjustBGMVolume(float sliderValue)
    {
        backgroundMusic.setVolume(sliderValue);
    }

    public void AdjustEffectsVolume(float sliderValue)
    {
        soundEffects.setVolume(sliderValue);
    }

    public void BackToPauseMenu()
    {
        settingMenu.SetActive(false);
    }
}
