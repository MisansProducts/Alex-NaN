using UnityEngine;
using System;

public class BackgroundMusic : MonoBehaviour
{
    private GameScript gameScript;
    private AudioSource audioSource;
    [SerializeField] private AudioClip mode1;
    [SerializeField] private AudioClip mode2Intro;
    [SerializeField] private AudioClip mode2;
    // private bool switching = false;
    private float volumeSettings;
    private bool mode2Lock; // Prevents from activating mode2 AudioClip more than once after mode2Intro ends

    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (mode2Lock && gameScript.mode2 && !audioSource.isPlaying && audioSource.time >= mode2Intro.length) {
            mode2Lock = false;
            audioSource.loop = true;
            audioSource.clip = mode2;
            audioSource.Play();
        }
    }

    public void switchBGM()
    {
        audioSource.clip = mode2Intro;
        audioSource.loop = false;
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
        mode2Lock = true;
        audioSource.Stop();
        audioSource.clip = mode1;
        audioSource.loop = true;
        audioSource.Play();
    }
    
    public void StopSound() {
        audioSource.Stop();
    }
}
