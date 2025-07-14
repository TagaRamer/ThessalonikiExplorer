using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LeaderboardController : MonoBehaviour
{
    [Header("UI References")]
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    public Button backButton;
    public Button clearScoresButton;
    public TMP_InputField playerNameInput;
    public Button saveScoreButton;
    public TextMeshProUGUI currentScoreText;

    [Header("Input Toggle")]
    public Button toggleInputButton;
    public GameObject inputPanel; // Panel containing input field and save button
    public TextMeshProUGUI toggleButtonText;

    [Header("Animation Settings")]
    public float entryAnimationDelay = 0.1f;
    public float entryAnimationDuration = 0.5f;
    public float inputToggleAnimationDuration = 0.3f;

    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();
    private List<GameObject> entryObjects = new List<GameObject>();
    private bool isInputPanelVisible = false;

    private void Start()
    {
        SetupUI();
        LoadLeaderboard();
        DisplayLeaderboard();
        AudioManager.Instance?.PlayLeaderboardMusic();
    }

    private void SetupUI()
    {
        backButton.onClick.AddListener(OnBackClicked);
        clearScoresButton.onClick.AddListener(OnClearScoresClicked);
        saveScoreButton.onClick.AddListener(OnSaveScoreClicked);

        // Setup toggle button
        if (toggleInputButton != null)
        {
            toggleInputButton.onClick.AddListener(OnToggleInputClicked);
            Debug.Log("Toggle button listener added successfully!");
        }
        else
        {
            Debug.LogError("Toggle Input Button is not assigned in the inspector!");
        }

        // Display current score
        if (currentScoreText != null)
        {
            currentScoreText.text = $"Current Score: {GameManager.Instance.currentScore}";
        }

        // Setup player name input
        if (playerNameInput != null)
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            playerNameInput.text = savedName;
        }

        // Initially hide input panel
        if (inputPanel != null)
        {
            inputPanel.SetActive(false);
            isInputPanelVisible = false;
            UpdateToggleButtonText();
            Debug.Log("Input panel initially hidden.");
        }
        else
        {
            Debug.LogError("Input Panel is not assigned in the inspector!");
        }
    }

    private void OnToggleInputClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (inputPanel == null)
        {
            Debug.LogError("Input Panel is not assigned!");
            return;
        }

        isInputPanelVisible = !isInputPanelVisible;

        if (isInputPanelVisible)
        {
            // Show input panel with lively animations
            inputPanel.SetActive(true);

            // Reset transform states
            inputPanel.transform.localScale = Vector3.zero;
            inputPanel.transform.localPosition = Vector3.zero;

            // Create a sequence of animations for a lively effect
            Sequence showSequence = DOTween.Sequence();

            // Scale up with bounce
            showSequence.Append(inputPanel.transform.DOScale(1.1f, inputToggleAnimationDuration * 0.6f)
                .SetEase(Ease.OutBack));

            // Scale down to normal size
            showSequence.Append(inputPanel.transform.DOScale(1f, inputToggleAnimationDuration * 0.4f)
                .SetEase(Ease.InOutQuad));

            // Add a subtle shake for liveliness
            showSequence.Join(inputPanel.transform.DOPunchPosition(new Vector3(0, 10, 0), 0.3f, 5, 0.5f));

            // Focus on input field after animation
            showSequence.OnComplete(() => {
                if (playerNameInput != null)
                {
                    playerNameInput.Select();
                    playerNameInput.ActivateInputField();
                }
            });

            // Animate individual elements inside the panel
            //AnimateInputElements(true);
        }
        else
        {
            // Hide input panel with smooth animation
            Sequence hideSequence = DOTween.Sequence();

            // Animate individual elements first
            //AnimateInputElements(false);

            // Scale down with ease
            hideSequence.Append(inputPanel.transform.DOScale(0.8f, inputToggleAnimationDuration * 0.3f)
                .SetEase(Ease.InQuad));

            hideSequence.Append(inputPanel.transform.DOScale(0f, inputToggleAnimationDuration * 0.7f)
                .SetEase(Ease.InBack));

            // Deactivate after animation
            hideSequence.OnComplete(() => {
                inputPanel.SetActive(false);
            });
        }

        UpdateToggleButtonText();
    }

    private void AnimateInputElements(bool show)
    {
        if (inputPanel == null) return;

        // Get all child elements to animate
        Transform[] childElements = inputPanel.GetComponentsInChildren<Transform>();

        for (int i = 0; i < childElements.Length; i++)
        {
            if (childElements[i] == inputPanel.transform) continue; // Skip the parent
            Debug.Log($"Animating child element: {childElements[i].name}");
            if (show)
            {
                Vector3 originalPos;
                if (i == 0)
                {
                    // Animate elements appearing with staggered timing
                    childElements[i].localScale = Vector3.zero;
                    childElements[i].DOScale(5.6f, inputToggleAnimationDuration * 0.8f)
                        .SetDelay(i * 0.1f)
                        .SetEase(Ease.OutBack);

                    // Add some vertical movement
                    originalPos = childElements[i].localPosition;
                    childElements[i].localPosition = originalPos + Vector3.up * 30f;
                    childElements[i].DOLocalMove(originalPos, inputToggleAnimationDuration * 0.6f)
                        .SetDelay(i * 0.1f)
                        .SetEase(Ease.OutQuad);
                    continue;
                }
                // Animate elements appearing with staggered timing
                childElements[i].localScale = Vector3.zero;
                childElements[i].DOScale(1f, inputToggleAnimationDuration * 0.8f)
                    .SetDelay(i * 0.1f)
                    .SetEase(Ease.OutBack);

                // Add some vertical movement
                originalPos = childElements[i].localPosition;
                childElements[i].localPosition = originalPos + Vector3.up * 30f;
                childElements[i].DOLocalMove(originalPos, inputToggleAnimationDuration * 0.6f)
                    .SetDelay(i * 0.1f)
                    .SetEase(Ease.OutQuad);
            }
            else
            {
                // Animate elements disappearing
                childElements[i].DOScale(0f, inputToggleAnimationDuration * 0.5f)
                    .SetDelay((childElements.Length - i) * 0.05f)
                    .SetEase(Ease.InQuad);
            }
        }
    }

    private void UpdateToggleButtonText()
    {
        if (toggleButtonText != null)
        {
            string newText = isInputPanelVisible ? "Απόκρυψη Πληκτρολόγησης" : "Αποθήκευσε Βαθμολογία";

            // Animate text change
            toggleButtonText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);

            // Change text with a small delay for the punch effect
            DOVirtual.DelayedCall(0.1f, () => {
                toggleButtonText.text = newText;
            });
        }
    }

    private void LoadLeaderboard()
    {
        leaderboardEntries.Clear();

        // Load scores from PlayerPrefs
        int scoreCount = PlayerPrefs.GetInt("LeaderboardCount", 0);

        for (int i = 0; i < scoreCount; i++)
        {
            string name = PlayerPrefs.GetString($"LeaderboardName_{i}", "Unknown");
            int score = PlayerPrefs.GetInt($"LeaderboardScore_{i}", 0);
            string date = PlayerPrefs.GetString($"LeaderboardDate_{i}", "Unknown");

            leaderboardEntries.Add(new LeaderboardEntry(name, score, date));
        }

        // Sort by score (highest first)
        leaderboardEntries = leaderboardEntries.OrderByDescending(entry => entry.score).ToList();

        // Keep only top 10
        if (leaderboardEntries.Count > 10)
        {
            leaderboardEntries = leaderboardEntries.Take(10).ToList();
        }
    }

    private void SaveLeaderboard()
    {
        PlayerPrefs.SetInt("LeaderboardCount", leaderboardEntries.Count);

        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            PlayerPrefs.SetString($"LeaderboardName_{i}", leaderboardEntries[i].playerName);
            PlayerPrefs.SetInt($"LeaderboardScore_{i}", leaderboardEntries[i].score);
            PlayerPrefs.SetString($"LeaderboardDate_{i}", leaderboardEntries[i].date);
        }

        PlayerPrefs.Save();
    }

    private void DisplayLeaderboard()
    {
        // Clear existing entries
        foreach (var obj in entryObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        entryObjects.Clear();

        // Create new entries
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            CreateLeaderboardEntry(i, leaderboardEntries[i]);
        }

        // If no scores, show message
        if (leaderboardEntries.Count == 0)
        {
            CreateEmptyMessage();
        }
    }

    private void CreateLeaderboardEntry(int index, LeaderboardEntry entry)
    {
        if (leaderboardEntryPrefab == null || leaderboardContent == null) return;

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
        entryObjects.Add(entryObj);

        // Get entry components
        var entryController = entryObj.GetComponent<LeaderboardEntryUI>();
        if (entryController != null)
        {
            entryController.SetupEntry(index + 1, entry.playerName, entry.score, entry.date);
        }
        
        else
        {
            // Fallback: Try to find text components directly
            Debug.LogWarning("LeaderboardEntryUI component not found! Trying direct text assignment...");

            TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 4)
            {
                texts[0].text = $"#{index + 1}";  // Rank
                texts[1].text = entry.playerName;   // Name
                texts[2].text = entry.score.ToString(); // Score
                texts[3].text = entry.date;         // Date
            }
        }

        // Animate entry appearance
        entryObj.transform.localScale = Vector3.zero;
        entryObj.transform.DOScale(2f, entryAnimationDuration)
            .SetDelay(index * entryAnimationDelay)
            .SetEase(Ease.OutBack);
    }

    private void CreateEmptyMessage()
    {
        GameObject messageObj = new GameObject("EmptyMessage");
        messageObj.transform.SetParent(leaderboardContent);

        var textComponent = messageObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "No scores yet!\nPlay the game to set your first score.";
        textComponent.fontSize = 24;
        textComponent.color = Color.gray;
        textComponent.alignment = TextAlignmentOptions.Center;

        var rectTransform = messageObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 100);
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = Vector2.zero;

        entryObjects.Add(messageObj);
    }

    public void OnSaveScoreClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        string playerName = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Anonymous";
        }

        // Save player name for future use
        PlayerPrefs.SetString("PlayerName", playerName);

        // Add current score to leaderboard
        string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");
        LeaderboardEntry newEntry = new LeaderboardEntry(playerName, GameManager.Instance.currentScore, currentDate);

        leaderboardEntries.Add(newEntry);

        // Sort and trim
        leaderboardEntries = leaderboardEntries.OrderByDescending(entry => entry.score).ToList();
        if (leaderboardEntries.Count > 10)
        {
            leaderboardEntries = leaderboardEntries.Take(10).ToList();
        }

        // Save and refresh display
        SaveLeaderboard();
        DisplayLeaderboard();

        // Disable save button to prevent multiple saves
        saveScoreButton.interactable = false;
        saveScoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "Score Saved!";

        // Hide input panel after saving (with animation)
        if (isInputPanelVisible)
        {
            // Add a small delay before hiding to show the "Score Saved!" message
            DOVirtual.DelayedCall(1f, () => {
                OnToggleInputClicked();
            });
        }

        // Debug output to verify the save
        Debug.Log($"Saved score: {playerName} - {GameManager.Instance.currentScore} points on {currentDate}");
    }

    public void OnClearScoresClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        // Clear all leaderboard data
        leaderboardEntries.Clear();

        // Clear PlayerPrefs
        int scoreCount = PlayerPrefs.GetInt("LeaderboardCount", 0);
        for (int i = 0; i < scoreCount; i++)
        {
            PlayerPrefs.DeleteKey($"LeaderboardName_{i}");
            PlayerPrefs.DeleteKey($"LeaderboardScore_{i}");
            PlayerPrefs.DeleteKey($"LeaderboardDate_{i}");
        }
        PlayerPrefs.DeleteKey("LeaderboardCount");
        PlayerPrefs.Save();

        // Refresh display
        DisplayLeaderboard();

        // Re-enable save button
        saveScoreButton.interactable = true;
        saveScoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save Score";
    }

    public void OnBackClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        GameManager.Instance?.LoadScene(GameManager.Instance.mapScene);
    }
}

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public string date;

    public LeaderboardEntry(string name, int playerScore, string entryDate)
    {
        playerName = name;
        score = playerScore;
        date = entryDate;
    }
}

