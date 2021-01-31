using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{

    public List<ParticleSystem> particles;
    public List<RandomSound> sounds;

    public void Play () {
        foreach (ParticleSystem particleSystem in particles) {
            if (particleSystem != null) {
                particleSystem.Play();
            }
        }
        foreach (RandomSound randomSound in sounds) {
            if (randomSound != null) {
                randomSound.Play();
            }
        }
    }

    public void Pause () {
        foreach (ParticleSystem particleSystem in particles) {
            if (particleSystem != null) {
                particleSystem.Pause();
            }
        }
        foreach (RandomSound randomSound in sounds) {
            if (randomSound != null) {
                randomSound.Pause();
            }
        }
    }

    public void Stop () {
        foreach (ParticleSystem particleSystem in particles) {
            if (particleSystem != null) {
                particleSystem.Stop();
            }
        }
        foreach (RandomSound randomSound in sounds) {
            if (randomSound != null) {
                randomSound.Stop();
            }
        }
    }
}
