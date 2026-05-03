using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Music")]
    public AudioSource musicSource;

    [Header("SFX")]
    public AudioSource sfxPrefab;
    public int poolSize = 10;

    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int poolIndex = 0;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource sfx = Instantiate(sfxPrefab, transform);
            sfx.playOnAwake = false;
            sfxPool.Add(sfx);
        }
    }

    AudioSource GetAvailableSFX()
    {
        AudioSource source = sfxPool[poolIndex];
        poolIndex = (poolIndex + 1) % poolSize;
        return source;
    }

    // 🔊 Play Sound Effect
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        AudioSource source = GetAvailableSFX();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }

    // 🎵 Play Music
    public void PlayMusic(AudioClip music, float volume = 1f, bool loop = true)
    {
        musicSource.clip = music;
        musicSource.volume = volume;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // ⏹ Stop Music
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // 🔇 Volume Controls
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        foreach (var sfx in sfxPool)
        {
            sfx.volume = volume;
        }
    }
}
