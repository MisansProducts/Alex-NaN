using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class SoundEffects : MonoBehaviour
{
    private float volumeSettings;

    void Start()
    {
    }

    public void setVolume(float volume)
    {
        // need to use array , replace with findobjectsbytype
        volumeSettings = volume;
        var sounds = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var sound in sounds) 
        {
            if(sound.CompareTag("SoundEffects"))
            {
                sound.volume = volumeSettings;
            }
        }
    }

    public float getVolume()
    {
        return volumeSettings;
    }

    public void StartGame()
    {
       
    }
}
