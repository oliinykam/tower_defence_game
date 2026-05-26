using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Pool Settings")]
    public int initialPoolSize = 15;
    public int maxPoolSize = 30;
    private List<AudioSource> audioPool = new List<AudioSource>();
    
    [Header("Global Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0.1f, 1f)]
    public float maxVolumeLimit = 0.5f; 

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float bgmVolume = 0.3f; 
    private AudioSource bgmSource;

    [Header("Anti-Glitch Settings")]
    public float clipCooldown = 0.05f;
    private Dictionary<AudioClip, float> lastPlayTime = new Dictionary<AudioClip, float>();

    private void Update()
    {
        if (bgmSource != null && bgmSource.volume != bgmVolume)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    private void Awake()
    {
        Instance = this;
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            audioPool.Add(source);
        }

        if (backgroundMusic != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume; 
            bgmSource.Play();
        }
    }

    public void SetMasterVolume(float vol)
    {
        if (vol > 1f) vol /= 100f;
        masterVolume = Mathf.Clamp01(vol);
        AudioListener.volume = masterVolume * maxVolumeLimit;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        if (lastPlayTime.TryGetValue(clip, out float lastTime))
        {
            if (Time.unscaledTime - lastTime < clipCooldown)
            {
                return; 
            }
        }

        lastPlayTime[clip] = Time.unscaledTime;

        AudioSource source = GetAvailableSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume; 
            source.Play();
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in audioPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        if (audioPool.Count < maxPoolSize)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            audioPool.Add(newSource);
            return newSource;
        }

        return null; 
    }
}