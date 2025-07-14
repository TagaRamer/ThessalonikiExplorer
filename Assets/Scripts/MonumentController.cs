using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MonumentController : MonoBehaviour
{
    [Header("Character System")]
    public Image characterImage;
    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite angrySprite;
    [Header("Question Set Reference")]
    public List<MonumentQuestionSet> questionSets; // Drag all sets in order in Inspector

    [Header("Background")]
    public Image backgroundImage;
    
    [Header("Question UI")]
    public GameObject questionPanel;
    public TextMeshProUGUI questionText;
    public List<Button> answerButtons = new List<Button>();
    public List<TextMeshProUGUI> answerTexts = new List<TextMeshProUGUI>();
    
    [Header("Feedback UI")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackText;
    public Button feedbackButton;
    public TextMeshProUGUI feedbackButtonText;
    
    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    public Button backButton;
    
    [Header("Animation Settings")]
    public float typewriterSpeed = 0.05f;
    public float feedbackAnimationDuration = 0.5f;
    public float characterEmotionDuration = 2f;
    
    [Header("Monument Data")]
    public int monumentIndex = 0;
    public List<QuestionData> questions = new List<QuestionData>();
    
    private int currentQuestionIndex = 0;
    private bool isAnswering = false;
    private Coroutine typewriterCoroutine;
    
    private void Start()
    {
        SetupUI();
        LoadMonumentData();
        StartQuiz();
        AudioManager.Instance?.PlayMonumentMusic();
    }
    
    private void SetupUI()
    {
        // Setup answer buttons
        for (int i = 0; i < answerButtons.Count; i++)
        {
            int buttonIndex = i; // Capture for closure
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(buttonIndex));
        }
        
        // Setup other buttons
        feedbackButton.onClick.AddListener(OnFeedbackButtonClicked);
        backButton.onClick.AddListener(OnBackClicked);

        // Initialize UI states
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        // Initialize UI states
        if (feedbackButton != null)
            feedbackButton.gameObject.SetActive(false); SetCharacterSprite(neutralSprite);
        UpdateScoreDisplay();
    }

    private void LoadMonumentData()
    {
        monumentIndex = GameManager.Instance.currentMonumentIndex;

        if (monumentIndex < questionSets.Count)
        {
            var questionSet = questionSets[monumentIndex];

            // Set background and character sprites
            if (backgroundImage != null) backgroundImage.sprite = questionSet.backgroundImage;
            neutralSprite = questionSet.neutralCharacter;
            happySprite = questionSet.happyCharacter;
            angrySprite = questionSet.angryCharacter;

            questions = new List<QuestionData>(questionSet.questions); // use SO questions
        }
    }

    private void LoadQuestionsForMonument(int index)
    {
        // This would typically load from a ScriptableObject or JSON file
        // For now, let's create sample questions for each monument
        questions.Clear();
        
        switch (index)
        {
            case 0: // White Tower
                questions.Add(new QuestionData(
                    "When was the White Tower of Thessaloniki built?",
                    new List<string> { "15th century", "16th century", "17th century", "18th century" },
                    0
                ));
                questions.Add(new QuestionData(
                    "What was the White Tower originally called?",
                    new List<string> { "Tower of Kalamaria", "Red Tower", "Tower of Blood", "Byzantine Tower" },
                    2
                ));
                break;
                
            case 1: // Arch of Galerius
                questions.Add(new QuestionData(
                    "Who commissioned the Arch of Galerius?",
                    new List<string> { "Emperor Constantine", "Emperor Galerius", "Emperor Justinian", "Emperor Diocletian" },
                    1
                ));
                questions.Add(new QuestionData(
                    "What victory does the arch commemorate?",
                    new List<string> { "Victory over Goths", "Victory over Persians", "Victory over Bulgars", "Victory over Arabs" },
                    1
                ));
                break;
                
            case 2: // Rotunda
                questions.Add(new QuestionData(
                    "What was the Rotunda originally built as?",
                    new List<string> { "A church", "A mausoleum", "A palace", "A library" },
                    1
                ));
                questions.Add(new QuestionData(
                    "The Rotunda was later converted into what?",
                    new List<string> { "A mosque", "A museum", "A library", "A palace" },
                    0
                ));
                break;
                
            case 3: // Church of Saint Demetrios
                questions.Add(new QuestionData(
                    "Saint Demetrios is the patron saint of which city?",
                    new List<string> { "Athens", "Constantinople", "Thessaloniki", "Patras" },
                    2
                ));
                questions.Add(new QuestionData(
                    "What type of building was Saint Demetrios martyred in?",
                    new List<string> { "A church", "A prison", "A bathhouse", "A palace" },
                    2
                ));
                break;
                
            case 4: // Eptapyrgio
                questions.Add(new QuestionData(
                    "How many towers does Eptapyrgio traditionally have?",
                    new List<string> { "Five", "Six", "Seven", "Eight" },
                    2
                ));
                questions.Add(new QuestionData(
                    "What was Eptapyrgio used for in the Ottoman period?",
                    new List<string> { "A palace", "A prison", "A mosque", "A market" },
                    1
                ));
                break;
        }
    }
    
    private void StartQuiz()
    {
        currentQuestionIndex = GameManager.Instance.monumentQuestionProgress[monumentIndex];
        DisplayQuestion();
    }
    
    private void DisplayQuestion()
    {
        if (currentQuestionIndex >= questions.Count)
        {
            GameManager.Instance.monumentsCompleted[monumentIndex] = true; ; // Move to next monument
            GameManager.Instance.SaveGameData(); ; // Move to next monument
            // All questions completed, return to map
            ReturnToMap();
            return;
        }
        ToggleBackButton(true);           // ⬅️ re-enable Back
        var question = questions[currentQuestionIndex];
        
        // Reset character to neutral
        SetCharacterSprite(neutralSprite);
        
        // Enable question panel, disable feedback panel
        questionPanel.SetActive(true);
        feedbackPanel.SetActive(false);
        feedbackButton.gameObject.SetActive(false);

        // Enable answer buttons
        SetAnswerButtonsInteractable(true);
        
        // Display question with typewriter effect
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterText(questionText, question.questionText));
        
        // Set up answer buttons
        for (int i = 0; i < answerButtons.Count && i < question.answers.Count; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerTexts[i].text = question.answers[i];
        }
        
        // Hide unused buttons
        for (int i = question.answers.Count; i < answerButtons.Count; i++)
        {
            answerButtons[i].gameObject.SetActive(false);
        }
        
        // Animate question panel entrance
        questionPanel.transform.localScale = Vector3.zero;
        questionPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
    }
    
    private void OnAnswerSelected(int answerIndex)
    {
        if (isAnswering) return;
        
        isAnswering = true;
        AudioManager.Instance?.PlayButtonClick();
        
        // Disable answer buttons to prevent multiple clicks
        SetAnswerButtonsInteractable(false);
        
        var question = questions[currentQuestionIndex];
        bool isCorrect = answerIndex == question.correctAnswerIndex;
        
        if (isCorrect)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }
    }
    
    private void HandleCorrectAnswer()
    {
        // Change character sprite to happy
        SetCharacterSprite(happySprite);
        
        // Play correct sound
        AudioManager.Instance?.PlayCorrectAnswer();
        
        // Update score
        GameManager.Instance.CorrectAnswer();
        UpdateScoreDisplay();
        
        // Show feedback
        ShowFeedback("ΣΩΣΤΑ! ΜΠΟΡΕΙΣ ΝΑ ΣΥΝΕΧΙΣΕΙΣ", Color.green, "ΕΠΟΜΕΝΗ");
       
        // Character animation
        characterImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 1, 0.5f);
        PersistProgress();
    }
    
    private void HandleWrongAnswer()
    {
        // Change character sprite to angry
        SetCharacterSprite(angrySprite);
        
        // Play wrong sound
        AudioManager.Instance?.PlayWrongAnswer();
        
        // Update score
        GameManager.Instance.WrongAnswer();
        UpdateScoreDisplay();
        
        // Show feedback
        ShowFeedback("ΛΑΘΟΣ ΑΠΑΝΤΗΣΗ!", Color.red, "ΠΡΟΣΠΑΘΗΣΕ ΞΑΝΑ");
        
        // Character shake animation
        characterImage.transform.DOShakePosition(0.5f, 10f, 10, 90f);
        PersistProgress();
    }
    
    private void ShowFeedback(string message, Color color, string buttonText)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackButtonText.text = buttonText;
        
        // Hide question panel and show feedback panel
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(true);
        feedbackButton.gameObject.SetActive(true);

        ToggleBackButton(false);          // ⬅️ disable Back
        // Animate feedback panel
        feedbackPanel.transform.localScale = Vector3.zero; 
        feedbackButton.transform.localScale = Vector3.zero; 
        feedbackPanel.transform.DOScale(1f, feedbackAnimationDuration).SetEase(Ease.OutBack);
        feedbackButton.transform.DOScale(1f, feedbackAnimationDuration).SetEase(Ease.OutBack);

        // Auto-reset character sprite after a delay
        DOVirtual.DelayedCall(characterEmotionDuration, () =>
        {
            SetCharacterSprite(neutralSprite);
        });
    }
    
    private void OnFeedbackButtonClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        
        if (feedbackButtonText.text == "ΠΡΟΣΠΑΘΗΣΕ ΞΑΝΑ")
        {
            // Allow the player to try the same question again
            isAnswering = false;
            DisplayQuestion();
        }
        else // "Next"
        {
            // Move to next question
            currentQuestionIndex++;
            isAnswering = false;
            DisplayQuestion();
        }
    }
    
    private void SetCharacterSprite(Sprite sprite)
    {
        if (characterImage != null && sprite != null)
        {
            characterImage.sprite = sprite;
        }
    }
    
    private void SetAnswerButtonsInteractable(bool interactable)
    {
        foreach (var button in answerButtons)
        {
            button.interactable = interactable;
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {GameManager.Instance.currentScore}";
        }
    }
    
    private IEnumerator TypewriterText(TextMeshProUGUI textComponent, string fullText)
    {
        textComponent.text = "";
        
        foreach (char c in fullText)
        {
            textComponent.text += c;
            AudioManager.Instance?.PlayTypewriter();
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    private void ReturnToMap()
    {
        // Fade out animation before returning to map
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            GameManager.Instance.LoadScene(GameManager.Instance.mapScene);
        });
    }
    
    public void OnBackClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        PersistProgress();
        ReturnToMap();
    }
    private void ToggleBackButton(bool interactable)
    {
        if (backButton != null)
            backButton.interactable = interactable;
    }
    private void PersistProgress()
    {
        GameManager.Instance.monumentQuestionProgress[monumentIndex] = currentQuestionIndex;
        GameManager.Instance.SaveGameData();
    }
}