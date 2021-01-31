using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleSounds : MonoBehaviour
{
    private ParticleSystem particles;

    [Tooltip("Should be an empty GameObject with a single AudioSource component. ParticleSounds will use this for playing sound if set")]
    public AudioSource prefabAudioSource;

    [Tooltip("Mark AudioSources as 3D or not. Overriden by Prefab Audio Source if set.")]
    public bool use3DSound;

    [Min(0)]
    [Tooltip("Limit the number of audio source objects that can be spawned (0 for no limit)")]
    public int audioSourceLimit = 0;

    // List of controlled audio sources + are they one time i.e. can they be interrupted?
    private Dictionary<AudioSource, bool> audioSources;

    [Serializable]
    public struct ParticleSoundVolume {
        [Tooltip("The curve will evaluate according to the duration mode.")]
        public ParticleSystem.MinMaxCurve volume;
        public enum SoundDurationMode {
            AUDIO_CLIP_DURATION, PARTICLE_LIFETIME
        }
        [Tooltip("The duration to use when evaluating the volume.")]
        public SoundDurationMode durationMode;
    }

    [Serializable]
    public struct ParticleSoundEvent {
        [Tooltip("A random sound from this list will play on each event.")]
        public List<AudioClip> sounds;
        public ParticleSoundVolume volume;
        [Tooltip("If checked, a new sound will be played for each particle on the trigerring frame. Otherwise, one sound will play for each frame the event is triggered.")]
        public bool soundPerParticle;
    }

    [Serializable]
    public struct ParticleOneTimeSoundEvent {
        [Tooltip("A random sound from this list will play on each event.")]
        public List<AudioClip> sounds;
        [Tooltip("The curve will evaluate over the audio clip's duration.")]
        public ParticleSystem.MinMaxCurve volume;
        [Tooltip("If checked, the sound will loop until Stop is called.")]
        public bool loop;
        public bool ignoreInterrupt;
    }

    [Header("Per particle sound events")]

    public List<ParticleSoundEvent> onBirth;
    public List<ParticleSoundEvent> onDeath;
    
    [Header("One time sound events")]
    
    [Tooltip("Note: will not trigger from editor 'restart'.")]
    public List<ParticleOneTimeSoundEvent> onPlay;
    public List<ParticleOneTimeSoundEvent> onPause;
    public List<ParticleOneTimeSoundEvent> onStop;

    [Space]

    [Header("Auto-destruct settings")]

    [Tooltip("Will destroy this GameObject once there are no particles left.")]
    public bool destroyOnceEmpty = false;
    [Tooltip("A time delay after the last particle has died to wait before destroying.")]
    public float destroyDelay = 0;
    [Tooltip("If new particles are born while the destroy delay is ticking, the object will not be destroyed and the timer reset.")]
    public bool newParticlesStopDestroy = true;

    private int particleCount = 0;
    private bool playedLastFrame = false;
    private bool pausedLastFrame = false;
    private bool stoppedLastFrame = false;
    private bool emptyLastFrame = false;
    private bool setToDestroy = false;
    private float timeEmpty = 0;
    private float startTime = 0;

    void OnEnable () {
        foreach (AudioSource source in GetComponentsInChildren<AudioSource>()) {
            source.Stop();
            RemoveAudioSource(source);
        }
        audioSources = new Dictionary<AudioSource, bool>();
        // Start with 1 source
        if (Application.isPlaying) {
            AddAudioSource(false);
        }
        particles = GetComponent<ParticleSystem>();
    }

#if UNITY_EDITOR
    void OnValidate () {
        if (!UnityEditor.EditorApplication.isPlaying) {
            if (audioSources != null)
                Stop();

            bool hasSoundPerParticleAndVolumeCurveCombo = false;

            foreach (ParticleSoundEvent soundEvent in onBirth) {
                if (soundEvent.soundPerParticle &&
                    (soundEvent.volume.volume.mode != ParticleSystemCurveMode.Constant &&
                    soundEvent.volume.volume.mode != ParticleSystemCurveMode.TwoConstants)) {

                    hasSoundPerParticleAndVolumeCurveCombo = true;
                    break;
                }
            }
            foreach (ParticleSoundEvent soundEvent in onDeath) {
                if (soundEvent.soundPerParticle &&
                    (soundEvent.volume.volume.mode != ParticleSystemCurveMode.Constant &&
                    soundEvent.volume.volume.mode != ParticleSystemCurveMode.TwoConstants)) {
                    
                    hasSoundPerParticleAndVolumeCurveCombo = true;
                    break;
                }
            }
            
            // TODO: Alerts every time a variable is changed when the condition is met, for any of them.
            // Create custom inspector to allow for custom warnings.
            if (hasSoundPerParticleAndVolumeCurveCombo) {
                /*UnityEditor.EditorUtility.DisplayDialog("Warning",
                    "Using a curve for volume will require ParticleSounds to add a new AudioSource per sound. Combined with Sound per Particle, this could cause a big performance snag and/or memory issue. Limiting the number of audio objects with Audio Source Limit is recommended.",
                    "I understand the risk");*/
            }
        }
    }
#endif

    void Start () {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        List<AudioSource> sourcesToRemove = new List<AudioSource>();
        int audioSourceCount = audioSources.Count;
        foreach (AudioSource audioSource in audioSources.Keys) {
            if (audioSource == null || AboveAudioSourceLimit(audioSourceCount)) {
                sourcesToRemove.Add(audioSource);
                audioSourceCount --;
            }
        }
        foreach (AudioSource source in sourcesToRemove) {
            RemoveAudioSource(source);
        }
        
        if (particles.isPlaying && !playedLastFrame) {
            // Particle system played
            // Stop all other one time events
            CleanupInterruptableAudioSources();
            foreach (ParticleOneTimeSoundEvent soundEvent in onPlay) {
                PlaySoundEvent(soundEvent);
            }
            playedLastFrame = true;
        } else if (!particles.isPlaying)
            playedLastFrame = false;

        if (particles.isPaused && !pausedLastFrame) {
            // Particle system paused
            // Stop all other one time events
            CleanupInterruptableAudioSources();
            foreach (ParticleOneTimeSoundEvent soundEvent in onPause) {
                PlaySoundEvent(soundEvent);
            }
            pausedLastFrame = true;
        } else if (!particles.isPaused)
            pausedLastFrame = false;

        if (particles.isStopped && !stoppedLastFrame) {
            // Particle system stopped
            // Stop all other one time events
            CleanupInterruptableAudioSources();
            foreach (ParticleOneTimeSoundEvent soundEvent in onStop) {
                PlaySoundEvent(soundEvent);
            }
            stoppedLastFrame = true;
        } else if (!particles.isStopped)
            stoppedLastFrame = false;

        if (particles.particleCount > particleCount) {
            // Particle born
            foreach (ParticleSoundEvent soundEvent in onBirth) {
                PlaySoundEvent(soundEvent);
            }
        } else if (particles.particleCount < particleCount) {
            // Particle dead
            foreach (ParticleSoundEvent soundEvent in onDeath) {
                PlaySoundEvent(soundEvent);
            }
        }
        particleCount = particles.particleCount;

        if (Application.isPlaying) {
            // Empty particle auto-destruct
            if (particleCount == 0) {
                // First frame being empty?
                if (!emptyLastFrame) {
                    emptyLastFrame = true;
                    timeEmpty = Time.time;
                    setToDestroy = true;
                }
            } else {
                emptyLastFrame = false;
                if (newParticlesStopDestroy) {
                    setToDestroy = false;
                }
            }
            if (Time.time - timeEmpty >= destroyDelay && destroyOnceEmpty && setToDestroy && Time.time - startTime > 0.05f) {
                Destroy(gameObject);
            }
        }

    }

    void PlaySoundEvent (ParticleSoundEvent soundEvent) {
        if (soundEvent.sounds == null)
            return;
        if (soundEvent.sounds.Count == 0)
            return;
        int soundCount = soundEvent.soundPerParticle ? Mathf.Abs(particles.particleCount - particleCount) : 1;
        for (int i = 0; i < soundCount; i ++) {
            int index = UnityEngine.Random.Range(0, soundEvent.sounds.Count - 1);
            if (soundEvent.sounds[index] == null)
                continue;
            GetAudioSourceAndPlay(soundEvent.sounds[index], soundEvent.volume);
        }
    }

    void PlaySoundEvent (ParticleOneTimeSoundEvent soundEvent) {
        if (soundEvent.sounds == null)
            return;
        if (soundEvent.sounds.Count == 0)
            return;
        int index = UnityEngine.Random.Range(0, soundEvent.sounds.Count - 1);
        if (soundEvent.sounds[index] == null)
            return;
        GetAudioSourceAndPlay(soundEvent.sounds[index], soundEvent.volume, soundEvent.ignoreInterrupt, soundEvent.loop);
    }

    // Play method for per particle events
    void GetAudioSourceAndPlay (AudioClip clip, ParticleSoundVolume volume) {
        if (volume.volume.mode == ParticleSystemCurveMode.Constant || volume.volume.mode == ParticleSystemCurveMode.TwoConstants) {
            audioSources.Keys.First().PlayOneShot(clip, volume.volume.Evaluate(0));
        } else {
            AudioSource chosenSource = null;
            foreach (AudioSource audioSource in audioSources.Keys) {
                if (!audioSource.isPlaying) {
                    chosenSource = audioSource;
                    audioSources[audioSource] = false;
                    break;
                }
            }
            if (chosenSource == null) {
                chosenSource = AddAudioSource(false);
            }
            // If adding the audio source failed, cancel the play
            if (chosenSource == null)
                return;
            chosenSource.clip = clip;
            chosenSource.Play();
            StartCoroutine(ApplyVolumeCurve(chosenSource, volume));
        }
    }

    // Play method for one time events
    void GetAudioSourceAndPlay (AudioClip clip, ParticleSystem.MinMaxCurve volume, bool ignoreInterrupt, bool loop) {
        AudioSource chosenSource = null;
        foreach (AudioSource audioSource in audioSources.Keys) {
            if (!audioSource.isPlaying) {
                chosenSource = audioSource;
                audioSources[audioSource] = !ignoreInterrupt;
                break;
            }
        }
        if (chosenSource == null) {
            chosenSource = AddAudioSource(!ignoreInterrupt);
        }
        // If adding the audio source failed, cancel the play
        if (chosenSource == null)
            return;
        chosenSource.clip = clip;
        chosenSource.loop = loop;
        chosenSource.Play();
        StartCoroutine(ApplyVolumeCurve(chosenSource, volume));
    }

    IEnumerator ApplyVolumeCurve (AudioSource audioSource, ParticleSoundVolume volume) {
        AudioClip originalClip = audioSource.clip;
        while (audioSource != null && audioSource.isPlaying && audioSource.clip == originalClip) {
            if (volume.durationMode == ParticleSoundVolume.SoundDurationMode.AUDIO_CLIP_DURATION) {
                audioSource.volume = volume.volume.Evaluate(Mathf.Clamp(audioSource.time / audioSource.clip.length, 0, 1));
            } else if (volume.durationMode == ParticleSoundVolume.SoundDurationMode.PARTICLE_LIFETIME) {
                float particleLifetime = particles.main.startLifetime.Evaluate(particles.time);
                audioSource.volume = volume.volume.Evaluate(Mathf.Clamp(audioSource.time / particleLifetime, 0, 1));
            }
            yield return null;
        }
    }

    IEnumerator ApplyVolumeCurve (AudioSource audioSource, ParticleSystem.MinMaxCurve volume) {
        AudioClip originalClip = audioSource.clip;
        while (audioSource != null && audioSource.isPlaying && audioSource.clip == originalClip) {
            audioSource.volume = volume.Evaluate(Mathf.Clamp(audioSource.time / audioSource.clip.length, 0, 1));
            yield return null;
        }
    }

    public void Stop () {
        foreach (AudioSource audioSource in audioSources.Keys) {
            audioSource.Stop();
        }
    }

    void CleanupInterruptableAudioSources () {
        List<AudioSource> sourcesToRemove = new List<AudioSource>();
        int sourceCount = audioSources.Count;
        foreach (AudioSource audioSource in audioSources.Keys) {
            if (audioSources[audioSource] || !audioSource.isPlaying) {
                audioSource.Stop();
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying) {
                    if (sourceCount > 1) {
                        sourcesToRemove.Add(audioSource);
                        sourceCount --;
                    }
                } else {
                    sourcesToRemove.Add(audioSource);
                }
#else
                if (sourceCount > 1) {
                    sourcesToRemove.Add(audioSource);
                    sourceCount --;
                }
#endif
            }
        }
        foreach (AudioSource source in sourcesToRemove) {
            RemoveAudioSource(source);
        }
    }

    AudioSource AddAudioSource (bool interrputable) {
        if (AboveAudioSourceLimit(audioSources.Count))
            return null;
        AudioSource audioSource = null;
        if (prefabAudioSource == null) {
            GameObject obj = new GameObject("Default ParticleSounds AudioSource");
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            audioSource = obj.AddComponent<AudioSource>();
            audioSource.spatialBlend = use3DSound ? 1 : 0;
        } else {
            audioSource = Instantiate(prefabAudioSource.gameObject, transform).GetComponent<AudioSource>();
        }
        audioSources.Add(audioSource, interrputable);
        return audioSource;
    }

    void RemoveAudioSource (AudioSource source) {
        if (source != null) {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                DestroyImmediate(source.gameObject);
            else
                Destroy(source.gameObject);
#else
            Destroy(source.gameObject);
#endif
        }
        if (audioSources != null)
            audioSources.Remove(source);
    }

    bool AboveAudioSourceLimit (int count) {
        return count > audioSourceLimit && audioSourceLimit > 0;
    }
}
