using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Background Music")]
    public AudioClip mainMenuMusic;
    public AudioClip mapSceneMusic;
    public AudioClip monumentMusic;
    public AudioClip leaderboardMusic;
    
    [Header("Sound Effects")]
    public AudioClip buttonClickSFX;
    public AudioClip correctAnswerSFX;
    public AudioClip wrongAnswerSFX;
    public AudioClip unlockSFX;
    public AudioClip typewriterSFX;
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    
    private bool musicEnabled = true;
    private bool sfxEnabled = true;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAudioSettings();
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("Music Source");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
        }
        
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }
        
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }
    
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (!musicEnabled || clip == null) return;
        
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        
        musicSource.clip = clip;
        musicSource.Play();
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (!sfxEnabled || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
    
    // Specific sound effect methods
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSFX);
    }
    
    public void PlayCorrectAnswer()
    {
        PlaySFX(correctAnswerSFX);
    }
    
    public void PlayWrongAnswer()
    {
        PlaySFX(wrongAnswerSFX);
    }
    
    public void PlayUnlockSound()
    {
        PlaySFX(unlockSFX);
    }
    
    public void PlayTypewriter()
    {
        PlaySFX(typewriterSFX);
    }
    
    // Scene-specific music
    public void PlayMainMenuMusic()
    {
        PlayBackgroundMusic(mainMenuMusic);
    }
    
    public void PlayMapSceneMusic()
    {
        PlayBackgroundMusic(mapSceneMusic);
    }
    
    public void PlayMonumentMusic()
    {
        PlayBackgroundMusic(monumentMusic);
    }
    
    public void PlayLeaderboardMusic()
    {
        PlayBackgroundMusic(leaderboardMusic);
    }
    
    // Settings controls
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        SaveAudioSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        SaveAudioSettings();
    }
    
    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        if (musicEnabled)
        {
            musicSource.UnPause();
        }
        else
        {
            musicSource.Pause();
        }
        SaveAudioSettings();
    }
    
    public void ToggleSFX()
    {
        sfxEnabled = !sfxEnabled;
        SaveAudioSettings();
    }
    
    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;
        if (enabled)
        {
            musicSource.UnPause();
        }
        else
        {
            musicSource.Pause();
        }
        SaveAudioSettings();
    }
    
    public void SetSFXEnabled(bool enabled)
    {
        sfxEnabled = enabled;
        SaveAudioSettings();
    }
    
    public bool IsMusicEnabled()
    {
        return musicEnabled;
    }
    
    public bool IsSFXEnabled()
    {
        return sfxEnabled;
    }
    
    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
    }
}