using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class SoundEffects : MonoBehaviour
{
    public static SoundEffects Instance;
    public AudioSource audioSource;
    [SerializeField] public AudioClip death; // plays but gets overrided by StopSound()
    [SerializeField] public AudioClip error;
    [SerializeField] public AudioClip flash;
    [SerializeField] public AudioClip floor;
    [SerializeField] public AudioClip jumpHold; // not able to play
    [SerializeField] public AudioClip jumpMid;
    [SerializeField] public AudioClip jumpStart;
    [SerializeField] public AudioClip pickup;
    [SerializeField] public AudioClip select; // not used right now
    [SerializeField] public AudioClip shieldPop;
    private float volumeSettings;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySound(AudioClip clip) {
        audioSource.PlayOneShot(clip);
    }
    public void StopSound() {
        audioSource.Stop();
    }

    public void setVolume(float volume)
    {
        // kaitlyn wtf is this
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
}
