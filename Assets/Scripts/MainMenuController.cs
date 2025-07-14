using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button settingsButton;
    public Button leaderboardButton;
    public Button resetButton;
    public Button exitButton;
    public CanvasGroup titleGroup;
    public CanvasGroup buttonGroup;
    
    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicToggle;
    public Toggle sfxToggle;
    public Button closeSettingsButton;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 1f;
    public float buttonAnimationDelay = 0.5f;
    public float buttonStaggerDelay = 0.1f;
    
    private void Start()
    {
        SetupUI();
        PlayEntryAnimations();
        AudioManager.Instance?.PlayMainMenuMusic();
    }
    
    private void SetupUI()
    {
        // Setup button listeners
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        resetButton.onClick.AddListener(OnResetClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);
        
        // Setup settings panel
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        SetupSettingsControls();
        
        // Initial UI state for animations
        titleGroup.alpha = 0f;
        buttonGroup.alpha = 0f;
        
        // Add button hover effects
        AddButtonHoverEffects();
    }


    private void SetupSettingsControls()
    {
        if (AudioManager.Instance != null)
        {
            // Setup sliders
            musicVolumeSlider.value = AudioManager.Instance.musicVolume;
            sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
            
            // Setup toggles
            musicToggle.isOn = AudioManager.Instance.IsMusicEnabled();
            sfxToggle.isOn = AudioManager.Instance.IsSFXEnabled();
            
            // Add listeners
            musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            sfxVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            musicToggle.onValueChanged.AddListener(AudioManager.Instance.SetMusicEnabled);
            sfxToggle.onValueChanged.AddListener(AudioManager.Instance.SetSFXEnabled);
        }
    }
    
    private void AddButtonHoverEffects()
    {
        Button[] buttons = { playButton, settingsButton, leaderboardButton, exitButton };
        
        foreach (Button button in buttons)
        {
            // Add scale animation on hover
            var buttonTransform = button.transform;
            
            button.onClick.AddListener(() => 
            {
                AudioManager.Instance?.PlayButtonClick();
                buttonTransform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.5f);
            });
        }
    }
    
    private void PlayEntryAnimations()
    {
        // Animate title
        titleGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuart);
        
        // Animate buttons with stagger
        DOVirtual.DelayedCall(buttonAnimationDelay, () =>
        {
            buttonGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuart);
            
            // Individual button animations
            Button[] buttons = { playButton, settingsButton, leaderboardButton, resetButton , exitButton };
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                button.transform.localScale = Vector3.zero;
                
                DOVirtual.DelayedCall(i * buttonStaggerDelay, () =>
                {
                    button.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
                });
            }
        });
    }
    
    #region Button Click Handlers
    
    public void OnPlayClicked()
    {
        StartCoroutine(TransitionToScene(GameManager.Instance.mapScene));
    }
    
    public void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.localScale = Vector3.zero;
            settingsPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
    }
    
    public void OnLeaderboardClicked()
    {
        StartCoroutine(TransitionToScene(GameManager.Instance.leaderboardScene));
    }
    
    public void OnExitClicked()
    {
        // Animate out before quitting
        titleGroup.DOFade(0f, 0.5f);
        buttonGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            GameManager.Instance?.QuitGame();
        });
    }
    
    public void OnCloseSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                settingsPanel.SetActive(false);
            });
        }
    }

    private void OnResetClicked()
    {
        GameManager.Instance.HardResetToMainMenu();
    }
    #endregion

    private System.Collections.IEnumerator TransitionToScene(string sceneName)
    {
        // Fade out animation
        titleGroup.DOFade(0f, 0.5f);
        buttonGroup.DOFade(0f, 0.5f);
        
        yield return new WaitForSeconds(0.5f);
        
        GameManager.Instance?.LoadScene(sceneName);
    }
}