using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MapController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI progressText;
    public Button backButton;
    public Button leaderboardButton;

    [Header("Monument Markers")]
    public List<MonumentMarker> monumentMarkers = new List<MonumentMarker>();

    [Header("Tooltip")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    [Header("Animation Settings")]
    public float tooltipAnimationDuration = 0.3f;
    public float unlockAnimationDelay = 0.5f;

    private void Start()
    {
        SetupUI();
        InitializeMarkers();
        UpdateUI();
        AudioManager.Instance?.PlayMapSceneMusic();
    }

    private void SetupUI()
    {
        backButton.onClick.AddListener(OnBackClicked);
        //leaderboardButton.onClick.AddListener(OnLeaderboardClicked);

        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private void InitializeMarkers()
    {
        for (int i = 0; i < monumentMarkers.Count; i++)
        {
            var marker = monumentMarkers[i];
            if (marker != null)
            {
                marker.Initialize(i, this);

                // Set marker state based on game progress
                bool isUnlocked = GameManager.Instance.IsMonumentUnlocked(i);
                marker.SetMarkerState(isUnlocked);

                // Check if monument is completed (you can add completion logic here)
                bool isCompleted = GameManager.Instance.IsMonumentCompleted(i);
                marker.SetCompleted(isCompleted);
            }
        }
    }
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {GameManager.Instance.currentScore}";

        if (progressText != null)
        {
            int completeCount = 0;
            foreach (bool completed in GameManager.Instance.monumentsCompleted)
            {
                if (completed) completeCount++;
            }
            progressText.text = $"Progress: {completeCount}/{GameManager.Instance.monumentsCompleted.Count}";
        }
    }

    public void OnMarkerClicked(int monumentIndex)
    {

        if (GameManager.Instance.IsMonumentUnlocked(monumentIndex) && !GameManager.Instance.IsMonumentCompleted(monumentIndex))
        {
            AudioManager.Instance?.PlayButtonClick();
            // Play click animation on the marker
            if (monumentIndex < monumentMarkers.Count && monumentMarkers[monumentIndex] != null)
            {
                monumentMarkers[monumentIndex].PlayClickAnimation();
            }

            // Small delay before scene transition for visual feedback
            DOVirtual.DelayedCall(0.3f, () =>
            {
                GameManager.Instance.LoadMonumentScene(monumentIndex);
            });
        }
        else
        {
            // Show locked tooltip
            if (monumentIndex < monumentMarkers.Count && monumentMarkers[monumentIndex] != null)
            {
                ShowTooltip("Unlock by answering 4 questions correctly!", monumentMarkers[monumentIndex].transform.position);
            }
        }
    }

    public void OnMarkerHover(int monumentIndex, bool isHovering)
    {
        if (monumentIndex >= monumentMarkers.Count || monumentMarkers[monumentIndex] == null || !GameManager.Instance.IsMonumentUnlocked(monumentIndex) || GameManager.Instance.IsMonumentCompleted(monumentIndex))
            return;
        

        var marker = monumentMarkers[monumentIndex];

        if (isHovering)
        {
            string monumentName = "Monument " + (monumentIndex + 1);
            // Get monument name from GameManager if available
            if (monumentIndex < GameManager.Instance.monuments.Count)
            {
                monumentName = GameManager.Instance.monuments[monumentIndex].name;
            }

            string tooltipMessage;
            if (GameManager.Instance.IsMonumentUnlocked(monumentIndex))
            {
                tooltipMessage = $"Explore {monumentName}";
            }
            else
            {
                tooltipMessage = $"{monumentName}\nLocked - Answer 4 questions to unlock";
            }

            ShowTooltip(tooltipMessage, marker.transform.position);
        }
        else
        {
            HideTooltip();
        }
    }

    private void ShowTooltip(string text, Vector3 worldPosition)
    {
        if (tooltipPanel == null || tooltipText == null) return;

        tooltipText.text = text;
        tooltipPanel.SetActive(true);

        // Convert world position to screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        tooltipPanel.transform.position = screenPos + Vector3.up * 100f; // Offset above marker

        // Animate tooltip
        tooltipPanel.transform.localScale = Vector3.zero;
        tooltipPanel.transform.DOScale(1f, tooltipAnimationDuration).SetEase(Ease.OutBack);
    }

    private void HideTooltip()
    {
        if (tooltipPanel == null) return;

        tooltipPanel.transform.DOScale(0f, tooltipAnimationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            tooltipPanel.SetActive(false);
        });
    }

    public void OnBackClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        GameManager.Instance?.LoadScene(GameManager.Instance.mainMenuScene);
    }

    public void OnLeaderboardClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        GameManager.Instance?.LoadScene(GameManager.Instance.leaderboardScene);
    }

    private void OnEnable()
    {
        // Refresh UI when returning to map
        UpdateUI();

        // Update marker states in case progress changed
        RefreshMarkerStates();

        // Check for newly unlocked monuments and play unlock animations
        CheckForNewUnlocks();
    }

    private void RefreshMarkerStates()
    {
        for (int i = 0; i < monumentMarkers.Count; i++)
        {
            if (monumentMarkers[i] != null)
            {
                bool isUnlocked = GameManager.Instance.IsMonumentUnlocked(i);
                monumentMarkers[i].SetMarkerState(isUnlocked);

                bool isCompleted = GameManager.Instance.IsMonumentCompleted(i);
                monumentMarkers[i].SetCompleted(isCompleted);
            }
        }
    }

    private void CheckForNewUnlocks()
    {
        // This method can be used to play unlock animations for newly unlocked monuments
        // You could store previous unlock state and compare with current state
        for (int i = 0; i < monumentMarkers.Count; i++)
        {
            if (monumentMarkers[i] != null && GameManager.Instance.IsMonumentUnlocked(i))
            {
                // You could add logic here to check if this monument was just unlocked
                // For now, we'll skip the animation to avoid playing it every time
            }
        }
    }

    // Method to manually trigger unlock animation (can be called from other scripts)
    public void PlayUnlockAnimation(int monumentIndex)
    {
        if (monumentIndex >= 0 && monumentIndex < monumentMarkers.Count && monumentMarkers[monumentIndex] != null)
        {
            DOVirtual.DelayedCall(unlockAnimationDelay, () =>
            {
                monumentMarkers[monumentIndex].PlayUnlockAnimation();
            });
        }
    }

    // Method to mark a monument as completed
    public void SetMonumentCompleted(int monumentIndex, bool completed)
    {
        if (monumentIndex >= 0 && monumentIndex < monumentMarkers.Count && monumentMarkers[monumentIndex] != null)
        {
            monumentMarkers[monumentIndex].SetCompleted(completed);
        }
    }
}