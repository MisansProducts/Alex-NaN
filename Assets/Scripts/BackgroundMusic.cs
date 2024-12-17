using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class BackgroundMusic : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip start;
    [SerializeField] private AudioClip mode0;
    [SerializeField] private AudioClip lightMode;
    private bool switching = false;
    private float volumeSettings;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void switchBGM()
    {
        audioSource.clip = lightMode;
        audioSource.Play();
    }

    public void setVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public float getVolume()
    {
        return audioSource.volume;
    }

    public void StartGame()
    {
        // audioSource.loop = false;
        // audioSource.clip = start;
        // audioSource.Play();
        // yield return new WaitForSeconds(audioSource.clip.length);
        audioSource.clip = mode0;
        audioSource.Play();
        audioSource.loop = true;
    }
}
