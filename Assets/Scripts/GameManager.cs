using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    
    [Header("Game State")]
    public int currentScore = 0;
    public int correctAnswersCount = 0;
    public List<bool> monumentsUnlocked = new List<bool> { true, false, false, false, false }; // First monument always unlocked
    public List<bool> monumentsCompleted = new List<bool> { false, false, false, false, false };
    public List<int> monumentQuestionProgress = new List<int> { 0, 0, 0, 0, 0 };   // next Q to ask per monument
    public int currentMonumentIndex = 0;
    
    [Header("Game Settings")]
    public int pointsPerCorrectAnswer = 10;
    public int pointsLostPerWrongAnswer = 5;
    public int answersNeededToUnlock = 4;
    
    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string mapScene = "MapScene";
    public string leaderboardScene = "Leaderboard";
    public List<string> monumentScenes = new List<string> 
    { 
        "WhiteTower", 
        "ArchOfGalerius", 
        "Rotunda", 
        "ChurchOfSaintDemetrios", 
        "Eptapyrgio" 
    };

#if UNITY_EDITOR        // keep it out of release builds
    [SerializeField, Tooltip("Tick → resets PlayerPrefs, then unticks itself")]
    private bool resetPlayerPrefs;
#endif

#if UNITY_EDITOR
    private void OnValidate()
    {
        // this runs whenever the inspector value changes in Edit *or* Play mode
        if (resetPlayerPrefs)
        {
            resetPlayerPrefs = false;       // auto-untick so it feels like a button
            ResetProgress();                // or ResetAllPrefs() if you added it
            Debug.Log("PlayerPrefs reset via inspector");

            // If you want a full wipe of *every* key, swap the two lines above for:
            // PlayerPrefs.DeleteAll();
            // ResetProgress();
        }
    }
#endif

    [Header("Monument Data")]
    public List<MonumentData> monuments = new List<MonumentData>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeMonuments();
    }
    
    private void InitializeMonuments()
    {
        if (monuments.Count == 0)
        {
            monuments.Add(new MonumentData("White Tower", "The symbol of Thessaloniki"));
            monuments.Add(new MonumentData("Arch of Galerius", "Also known as Kamara"));
            monuments.Add(new MonumentData("Rotunda", "Ancient Roman monument"));
            monuments.Add(new MonumentData("Church of Saint Demetrios", "Patron saint of the city"));
            monuments.Add(new MonumentData("Eptapyrgio", "Byzantine fortress walls"));
        }
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        if (currentScore < 0) currentScore = 0;
        SaveGameData();
    }
    
    public void CorrectAnswer()
    {
        AddScore(pointsPerCorrectAnswer);
        correctAnswersCount++;
        
        // Check if we should unlock next monument
        if (correctAnswersCount >= answersNeededToUnlock && currentMonumentIndex + 1 < monumentsUnlocked.Count)
        {
            if (!monumentsUnlocked[currentMonumentIndex + 1])
            {
                monumentsUnlocked[currentMonumentIndex + 1] = true;
                AudioManager.Instance?.PlayUnlockSound();
            }
        }
        
        SaveGameData();
    }
    
    public void WrongAnswer()
    {
        AddScore(-pointsLostPerWrongAnswer);
        SaveGameData();
    }
    
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadMonumentScene(int monumentIndex)
    {
        if (monumentIndex >= 0 && monumentIndex < monumentScenes.Count && IsMonumentUnlocked(monumentIndex))
        {
            currentMonumentIndex = monumentIndex;
            LoadScene(monumentScenes[monumentIndex]);
        }
    }

    public bool IsMonumentUnlocked(int index)
    {
        return index >= 0 && index < monumentsUnlocked.Count && monumentsUnlocked[index];
    }

    public bool IsMonumentCompleted(int index)
    {
        return index >= 0 && index < monumentsCompleted.Count && monumentsCompleted[index];
    }
    public void ResetProgress()
    {
        // ───── clear saved keys ─────
        for (int i = 0; i < monumentQuestionProgress.Count; i++)
            PlayerPrefs.DeleteKey($"Monument_{i}_QuestionIndex");

        for (int i = 0; i < monumentsUnlocked.Count; i++)
            PlayerPrefs.DeleteKey($"Monument_{i}_Unlocked");

        for (int i = 0; i < monumentsCompleted.Count; i++)
            PlayerPrefs.DeleteKey($"Monument_{i}_Completed");

        PlayerPrefs.Save();          // flush everything to disk
        currentScore = 0;
        correctAnswersCount = 0;
        monumentsUnlocked = new List<bool> { true, false, false, false, false };
        monumentsCompleted = new List<bool> { false, false, false, false, false };
        currentMonumentIndex = 0;
        monumentQuestionProgress = new List<int> { 0, 0, 0, 0, 0 };

        SaveGameData();
    }
    
    public void SaveGameData()
    {
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.SetInt("CorrectAnswersCount", correctAnswersCount);
        PlayerPrefs.SetInt("CurrentMonumentIndex", currentMonumentIndex);
        for (int i = 0; i < monumentQuestionProgress.Count; i++)
            PlayerPrefs.SetInt($"Monument_{i}_QuestionIndex", monumentQuestionProgress[i]);
        // Save unlocked monuments
        for (int i = 0; i < monumentsUnlocked.Count; i++)
            PlayerPrefs.SetInt($"Monument_{i}_Unlocked", monumentsUnlocked[i] ? 1 : 0);
        
        //Save completed Levels
        for (int i = 0; i < monumentsCompleted.Count; i++)
            PlayerPrefs.SetInt($"Monument_{i}_Completed", monumentsCompleted[i] ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void LoadGameData()
    {
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        correctAnswersCount = PlayerPrefs.GetInt("CorrectAnswersCount", 0);
        currentMonumentIndex = PlayerPrefs.GetInt("CurrentMonumentIndex", 0);

        // ─── Question progress ───
        monumentQuestionProgress.Clear();                       // ①

        int slotCount = monumentsUnlocked.Count;                // in case you add more monuments later
        for (int i = 0; i < slotCount; i++)
            monumentQuestionProgress.Add(PlayerPrefs.GetInt($"Monument_{i}_QuestionIndex", 0));

        // ─── Unlocked flags ───
        for (int i = 0; i < slotCount; i++)
        {
            bool isUnlocked = (i == 0)                                   // first monument always open
                              || PlayerPrefs.GetInt($"Monument_{i}_Unlocked", 0) == 1;
            monumentsUnlocked[i] = isUnlocked;
        }

        // ─── Completed flags ───
        for (int i = 0; i < slotCount; i++)
        {
            bool isComplete = PlayerPrefs.GetInt($"Monument_{i}_Completed", 0) == 1;
            monumentsCompleted[i] = isComplete;
        }
    }

    public void HardResetToMainMenu()
    {
        ResetProgress();                       // clears PlayerPrefs & memory
        LoadScene(mainMenuScene);              // load whatever start scene you want
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

[System.Serializable]
public class MonumentData
{
    public string name;
    public string description;
    public Sprite backgroundImage;
    public List<QuestionData> questions;
    
    public MonumentData(string monumentName, string monumentDescription)
    {
        name = monumentName;
        description = monumentDescription;
        questions = new List<QuestionData>();
    }
}

[System.Serializable]
public class QuestionData
{
    public string questionText;
    public List<string> answers;
    public int correctAnswerIndex;
    
    public QuestionData(string question, List<string> answerOptions, int correct)
    {
        questionText = question;
        answers = answerOptions;
        correctAnswerIndex = correct;
    }
}