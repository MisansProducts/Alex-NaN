using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class SettingMenu : MonoBehaviour
{

    void Start()
    {
    }

    public void EnableKeyboard()
    {

    }

    public void EnableMouse()
    {

    }

    public void AdjustVolume(float sliderValue)
    {
        AudioSource audio = (AudioSource)GameObject.FindObjectOfType(typeof(AudioSource));
        audio.volume = sliderValue;
    }
}
