using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Sound class to store audio data
[Serializable]
public class Sound {
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
}

// Main AudioManager class
public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup uiMixerGroup;

    // Organized sound collections by type
    [Header("Background Music")] [SerializeField]
    private Sound[] bgmSounds;

    [Header("Sound Effects")] [SerializeField]
    private Sound[] sfxSounds;

    [Header("UI Sounds")] [SerializeField] private Sound[] uiSounds;

    // Object pooling settings
    [Header("Audio Source Pooling")] [SerializeField]
    private int initialPoolSize = 10;

    [SerializeField] private int maxPoolSize = 30;

    private AudioSource bgmSource;
    private Dictionary<string, Sound> bgmDictionary = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> sfxDictionary = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> uiDictionary = new Dictionary<string, Sound>();

    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    private const string MasterVolumeParam = "MasterVolume";
    private const string SFXVolumeParam = "SFXVolume";
    private const string BGMVolumeParam = "BGMVolume";
    private const string UIVolumeParam = "UIVolume";

    private void Awake() {
        // Singleton pattern
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        } else {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioManager() {
        // Create background music source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.outputAudioMixerGroup = bgmMixerGroup;

        // Initialize sound dictionaries
        InitializeSoundDictionaries();

        // Initialize audio source pool
        InitializeAudioSourcePool();
    }

    private void InitializeSoundDictionaries() {
        // BGM sounds
        foreach (Sound sound in bgmSounds) {
            if (bgmDictionary.ContainsKey(sound.name)) {
                Debug.LogWarning($"BGM Sound with name {sound.name} already exists!");
                continue;
            }

            bgmDictionary.Add(sound.name, sound);
        }

        // SFX sounds
        foreach (Sound sound in sfxSounds) {
            if (sfxDictionary.ContainsKey(sound.name)) {
                Debug.LogWarning($"SFX Sound with name {sound.name} already exists!");
                continue;
            }

            sfxDictionary.Add(sound.name, sound);
        }

        // UI sounds
        foreach (Sound sound in uiSounds) {
            if (uiDictionary.ContainsKey(sound.name)) {
                Debug.LogWarning($"UI Sound with name {sound.name} already exists!");
                continue;
            }

            uiDictionary.Add(sound.name, sound);
        }
    }

    private void InitializeAudioSourcePool() {
        // Create initial pool of audio sources
        for (int i = 0; i < initialPoolSize; i++) {
            CreatePooledAudioSource();
        }
    }

    private AudioSource CreatePooledAudioSource() {
        GameObject audioSourceObj = new GameObject("Pooled_AudioSource");
        audioSourceObj.transform.SetParent(transform);
        AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSourceObj.SetActive(false);
        audioSourcePool.Enqueue(audioSource);
        return audioSource;
    }

    private AudioSource GetAudioSourceFromPool() {
        if (audioSourcePool.Count == 0 && activeAudioSources.Count < maxPoolSize) {
            return CreatePooledAudioSource();
        } else if (audioSourcePool.Count > 0) {
            AudioSource source = audioSourcePool.Dequeue();
            source.gameObject.SetActive(true);
            activeAudioSources.Add(source);
            return source;
        } else {
            // If we've reached max pool size, reuse the oldest active source
            AudioSource oldestSource = activeAudioSources[0];
            activeAudioSources.RemoveAt(0);
            activeAudioSources.Add(oldestSource);
            oldestSource.Stop();
            return oldestSource;
        }
    }

    private void ReturnAudioSourceToPool(AudioSource source) {
        if (source == null) return;

        source.clip = null;
        source.Stop();
        source.gameObject.SetActive(false);
        source.spatialBlend = 0f;
        source.transform.position = transform.position;
        source.transform.SetParent(transform);

        activeAudioSources.Remove(source);
        audioSourcePool.Enqueue(source);
    }

    // Play a BGM sound by name
    public void PlayBGM(string soundName) {
        if (!bgmDictionary.TryGetValue(soundName, out Sound sound)) {
            Debug.LogWarning($"BGM sound {soundName} not found!");
            return;
        }
        Debug.Log($"BGM sound {soundName} is playing!");

        // Stop current BGM if playing
        bgmSource.Stop();

        // Set new BGM properties
        bgmSource.clip = sound.clip;
        bgmSource.volume = sound.volume;
        bgmSource.pitch = sound.pitch;
        bgmSource.outputAudioMixerGroup = bgmMixerGroup;

        // Play new BGM on loop
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(string soundName) {
        PlaySFX("", null);
    }
    
    // Play an SFX sound by name
    public void PlaySFX(string soundName, Vector3? position = null) {
        if (!sfxDictionary.TryGetValue(soundName, out Sound sound)) {
            Debug.LogWarning($"SFX sound {soundName} not found!");
            return;
        }

        Debug.Log($"SFX sound {soundName} is playing!");
        AudioSource source = GetAudioSourceFromPool();

        if (position.HasValue) {
            // Make spatial at position
            source.transform.position = position.Value;
            source.spatialBlend = 1f; // Full 3D
        } else {
            // Use non-spatial audio
            source.transform.position = transform.position;
            source.spatialBlend = 0f; // 2D
        }

        // Configure and play sound
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.loop = false;
        source.outputAudioMixerGroup = sfxMixerGroup;
        source.Play();

        // Return to pool after completion
        StartCoroutine(ReturnToPoolAfterPlay(source, sound.clip.length / sound.pitch));
    }

    // Play a UI sound by name
    public void PlayUI(string soundName) {
        if (!uiDictionary.TryGetValue(soundName, out Sound sound)) {
            Debug.LogWarning($"UI sound {soundName} not found!");
            return;
        }

        Debug.Log($"UI sound {soundName} is playing!");
        AudioSource source = GetAudioSourceFromPool();
        source.spatialBlend = 0f;
        source.transform.position = transform.position;
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.loop = false;
        source.outputAudioMixerGroup = uiMixerGroup;
        source.Play();

        // Return to pool after completion
        StartCoroutine(ReturnToPoolAfterPlay(source, sound.clip.length / sound.pitch));
    }

    // Return AudioSource to pool after playing
    private IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay) {
        yield return new WaitForSeconds(delay);
        ReturnAudioSourceToPool(source);
    }

    // Stop BGM
    public void StopBGM() {
        bgmSource.Stop();
    }

    // Stop specific SFX by name
    public void StopSFX(string soundName) {
        foreach (AudioSource source in activeAudioSources) {
            if (source.outputAudioMixerGroup == sfxMixerGroup &&
                source.clip != null && source.clip.name == soundName) {
                source.Stop();
                ReturnAudioSourceToPool(source);
                break;
            }
        }
    }

    // Stop all SFX
    public void StopAllSFX() {
        List<AudioSource> sourcesToReturn = new List<AudioSource>();
        foreach (AudioSource source in activeAudioSources) {
            if (source.outputAudioMixerGroup == sfxMixerGroup) {
                source.Stop();
                sourcesToReturn.Add(source);
            }
        }

        foreach (AudioSource source in sourcesToReturn) {
            ReturnAudioSourceToPool(source);
        }
    }

    // Stop specific UI sound by name
    public void StopUI(string soundName) {
        foreach (AudioSource source in activeAudioSources) {
            if (source.outputAudioMixerGroup == uiMixerGroup &&
                source.clip != null && source.clip.name == soundName) {
                source.Stop();
                ReturnAudioSourceToPool(source);
                break;
            }
        }
    }

    // Stop all UI sounds
    public void StopAllUI() {
        List<AudioSource> sourcesToReturn = new List<AudioSource>();
        foreach (AudioSource source in activeAudioSources) {
            if (source.outputAudioMixerGroup == uiMixerGroup) {
                source.Stop();
                sourcesToReturn.Add(source);
            }
        }

        foreach (AudioSource source in sourcesToReturn) {
            ReturnAudioSourceToPool(source);
        }
    }

    // Stop all sounds
    public void StopAll() {
        StopBGM();
        StopAllSFX();
        StopAllUI();
    }

    // Runtime sound registration
    public void AddBGM(string name, AudioClip clip, float volume = 1f, float pitch = 1f) {
        Sound sound = new Sound {
            name = name,
            clip = clip,
            volume = volume,
            pitch = pitch
        };

        if (bgmDictionary.ContainsKey(name)) {
            bgmDictionary[name] = sound;
        } else {
            bgmDictionary.Add(name, sound);
        }
    }

    public void AddSFX(string name, AudioClip clip, float volume = 1f, float pitch = 1f) {
        Sound sound = new Sound {
            name = name,
            clip = clip,
            volume = volume,
            pitch = pitch
        };

        if (sfxDictionary.ContainsKey(name)) {
            sfxDictionary[name] = sound;
        } else {
            sfxDictionary.Add(name, sound);
        }
    }

    public void AddUI(string name, AudioClip clip, float volume = 1f, float pitch = 1f) {
        Sound sound = new Sound {
            name = name,
            clip = clip,
            volume = volume,
            pitch = pitch
        };

        if (uiDictionary.ContainsKey(name)) {
            uiDictionary[name] = sound;
        } else {
            uiDictionary.Add(name, sound);
        }
    }

    // Volume control methods
    public void SetMasterVolume(float volume) {
        SetVolumeParam(MasterVolumeParam, volume);
    }

    public void SetSFXVolume(float volume) {
        SetVolumeParam(SFXVolumeParam, volume);
    }

    public void SetBGMVolume(float volume) {
        SetVolumeParam(BGMVolumeParam, volume);
    }

    public void SetUIVolume(float volume) {
        SetVolumeParam(UIVolumeParam, volume);
    }

    // Convert linear volume (0-1) to decibels for the audio mixer
    private void SetVolumeParam(string paramName, float linearVolume) {
        // Convert from linear (0-1) to decibel (-80 to 0)
        float dbVolume = linearVolume > 0.001f ? 20f * Mathf.Log10(linearVolume) : -80f;
        audioMixer.SetFloat(paramName, dbVolume);
    }

    // Get current volume levels
    public float GetMasterVolume() => GetVolumeParam(MasterVolumeParam);
    public float GetSFXVolume() => GetVolumeParam(SFXVolumeParam);
    public float GetBGMVolume() => GetVolumeParam(BGMVolumeParam);
    public float GetUIVolume() => GetVolumeParam(UIVolumeParam);

    // Get volume parameter as linear value (0-1)
    private float GetVolumeParam(string paramName) {
        if (audioMixer.GetFloat(paramName, out float dbVolume)) {
            // Convert from decibel (-80 to 0) to linear (0-1)
            return dbVolume <= -80f ? 0f : Mathf.Pow(10f, dbVolume / 20f);
        }

        return 1f;
    }

    // Helper class to provide methods for volume control
    public class VolumeControl {
        private AudioManager audioManager;

        public VolumeControl(AudioManager manager) {
            audioManager = manager;
        }

        public void SetMasterVolume(float volume) => audioManager.SetMasterVolume(volume);
        public void SetSFXVolume(float volume) => audioManager.SetSFXVolume(volume);
        public void SetBGMVolume(float volume) => audioManager.SetBGMVolume(volume);
        public void SetUIVolume(float volume) => audioManager.SetUIVolume(volume);

        public float GetMasterVolume() => audioManager.GetMasterVolume();
        public float GetSFXVolume() => audioManager.GetSFXVolume();
        public float GetBGMVolume() => audioManager.GetBGMVolume();
        public float GetUIVolume() => audioManager.GetUIVolume();
    }

    // Public property to access volume control functionality
    public VolumeControl VolumeControls => new VolumeControl(this);
}
