using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{

    private BackgroundMusic backgroundMusic;
    private Slider volumeSlider;


    void Start()
    {
        backgroundMusic = FindObjectOfType<BackgroundMusic>();
        volumeSlider = FindObjectOfType<Slider>();
        volumeSlider.value = backgroundMusic.getVolume();
    }

    public void EnableKeyboard()
    {

    }

    public void EnableMouse()
    {

    }

    public void AdjustVolume(float sliderValue)
    {
        backgroundMusic.setVolume(sliderValue);
    }
}
