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
        
        if (PlayerPrefs.HasKey("BGMVolume") && PlayerPrefs.HasKey("EffectsVolume"))
        {
            LoadVolume();
        }
        else
        {
            PlayerPrefs.SetFloat("BGMVolume", 0.5f);
            PlayerPrefs.SetFloat("EffectsVolume", 0.5f);
            LoadVolume();
        }
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
        PlayerPrefs.SetFloat("BGMVolume", sliderValue);
    }

    public void AdjustEffectsVolume(float sliderValue)
    {
        soundEffects.setVolume(sliderValue);
        PlayerPrefs.SetFloat("EffectsVolume", sliderValue);
    }

    public void LoadVolume()
    {
        volumeSliderBGM.value = PlayerPrefs.GetFloat("BGMVolume");
        volumeSliderSoundEffects.value = PlayerPrefs.GetFloat("EffectsVolume");
    }

    public void BackToPauseMenu()
    {
        settingMenu.SetActive(false);
    }
}
