using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    AudioSource audioSource;
    AudioClip clipMusic;
    float volume = 0.02f;

    public void ChangeVolume(float changeVolume)
    {
        volume = changeVolume;
    }

    void Start()
    {
        clipMusic = Resources.Load<AudioClip> ("Audio/Chopin Fsharp Adagio Strings");
        audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.PlayOneShot(clipMusic, volume);
    }
}
