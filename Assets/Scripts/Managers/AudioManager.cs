using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const string VolumeKey = "volume";

    private static AudioManager instance;

    // Dibuat otomatis kalau belum ada, jadi tidak perlu dipasang di scene
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                new GameObject("AudioManager").AddComponent<AudioManager>();
            }
            return instance;
        }
    }

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    private float volume = 0.8f;
    private AudioClip pendingLoop;

    // Satu volume untuk semua suara (musik dan SFX)
    public float Volume
    {
        get => volume;
        set
        {
            volume = Mathf.Clamp01(value);
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(VolumeKey, volume);
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        // Volume tersimpan dari MainMenu otomatis berlaku di semua scene
        volume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
        AudioListener.volume = volume;
    }

    // Dipanggil saat panel settings ditutup (aman untuk WebGL)
    public void Save()
    {
        PlayerPrefs.Save();
    }

    private AudioClip GetClip(string clipName)
    {
        if (cache.TryGetValue(clipName, out var clip)) return clip;

        clip = Resources.Load<AudioClip>("Audio/" + clipName);
        if (clip == null) Debug.LogWarning($"AudioManager: clip '{clipName}' tidak ditemukan di Resources/Audio");

        cache[clipName] = clip;
        return clip;
    }

    public void PlayMusic(string loopName, string introName = null)
    {
        var loop = GetClip(loopName);
        if (loop == null) return;

        var intro = string.IsNullOrEmpty(introName) ? null : GetClip(introName);
        
        if (musicSource.isPlaying && (musicSource.clip == loop || pendingLoop == loop)) return;

        if (intro != null)
        {
            pendingLoop = loop;
            musicSource.loop = false;
            musicSource.clip = intro;
        }

        else
        {
            pendingLoop = null;
            musicSource.loop = true;
            musicSource.clip = loop;
        }

        musicSource.Play();
    }

    public void StopMusic()
    {
        pendingLoop = null;
        musicSource.Stop();
        musicSource.clip = null;
    }

    void Update()
    {
        if (pendingLoop != null && !musicSource.isPlaying)
        {
            musicSource.clip = pendingLoop;
            musicSource.loop = true;
            musicSource.Play();
            pendingLoop = null;
        }
    }

    public void PlaySfx(string clipName, float pitchVariance = 0f)
    {
        var clip = GetClip(clipName);
        if (clip == null) return;

        sfxSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        sfxSource.PlayOneShot(clip);
    }
}