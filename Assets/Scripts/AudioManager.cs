using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEntry
    {
        public string soundName;
        public AudioClip clip;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public float spatialBlend = 1f;
        public float minDistance = 1f;
        public float maxDistance = 50f;

        [HideInInspector] public AudioSource source;
    }

    [Header("Audio Settings")]
    public AudioMixer masterMixer;
    public List<SoundEntry> sounds = new List<SoundEntry>();

    [Header("Weapon Sounds")]
    public AudioClip[] assaultRifleClips;
    public AudioClip[] sniperRifleClips;
    public AudioClip[] rpgLaunchClips;
    public AudioClip[] explosionClips;

    [Header("Environmental Audio")]
    public AudioClip ambienceDay;
    public AudioClip ambienceNight;
    public AudioClip tunnelAmbience;
    public AudioClip windSound;

    [Header("Voice")]
    public AudioClip[] missionBriefings;
    public AudioClip[] combatTaunts;
    public AudioClip[] victoryCries;

    private Dictionary<string, SoundEntry> soundDictionary;
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<AudioManager>();
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        soundDictionary = new Dictionary<string, SoundEntry>();

        foreach (SoundEntry s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.mixerGroup;
            s.source.loop = s.loop;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.spatialBlend = s.spatialBlend;
            s.source.minDistance = s.minDistance;
            s.source.maxDistance = s.maxDistance;

            if (!soundDictionary.ContainsKey(s.soundName))
                soundDictionary.Add(s.soundName, s);
        }
    }

    public void Play(string soundName)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in AudioManager!");
            return;
        }

        SoundEntry s = soundDictionary[soundName];
        s.source.Play();
    }

    public void PlayOneShot(string soundName)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in AudioManager!");
            return;
        }

        SoundEntry s = soundDictionary[soundName];
        s.source.PlayOneShot(s.clip);
    }

    public void PlayAtPosition(string soundName, Vector3 position)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in AudioManager!");
            return;
        }

        SoundEntry s = soundDictionary[soundName];
        AudioSource.PlayClipAtPoint(s.clip, position, s.volume);
    }

    public void Stop(string soundName)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in AudioManager!");
            return;
        }

        SoundEntry s = soundDictionary[soundName];
        s.source.Stop();
    }

    public void SetVolume(string parameterName, float value)
    {
        if (masterMixer != null)
            masterMixer.SetFloat(parameterName, value);
    }

    public void SetMasterVolume(float volume)
    {
        SetVolume("MasterVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume("SFXVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
    }

    public void SetMusicVolume(float volume)
    {
        SetVolume("MusicVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
    }

    public void PlayRandomWeaponSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.7f);
    }

    public void PlayExplosionSound(Vector3 position)
    {
        if (explosionClips == null || explosionClips.Length == 0) return;
        AudioClip clip = explosionClips[Random.Range(0, explosionClips.Length)];
        AudioSource.PlayClipAtPoint(clip, position, 1f);
    }
}
