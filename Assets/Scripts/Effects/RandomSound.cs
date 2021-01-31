using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class RandomSound : MonoBehaviour {

    public List<AudioClip> sounds;

    [HideInInspector]
    public AudioSource audioSource;
    
    void Start () {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOneShot () {
        int playSound = UnityEngine.Random.Range(0, sounds.Count - 1);
        audioSource.PlayOneShot(sounds[playSound]);
    }

    public void PlayOneShot (float volume) {
        int playSound = UnityEngine.Random.Range(0, sounds.Count - 1);
        audioSource.PlayOneShot(sounds[playSound], volume);
    }

    public void Play () {
        int playSound = UnityEngine.Random.Range(0, sounds.Count - 1);
        audioSource.clip = sounds[playSound];
        audioSource.Play();
    }

    public void Pause () {
        audioSource.Pause();
    }

    public void Stop () {
        audioSource.Stop();
    }

}