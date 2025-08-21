using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    public List<GateExplainEntry> gatesToExplain = new();

    public bool isFirst = true;


    [Header("Info")]
    public int LevelClicked = 0;
    public int noOfLevels;
    public int GameType = 0;
    public int row = 6;
    public int col = 10;

    [Header("Level Completion Tracking")]
    public Dictionary<int, bool> easyModeCompleted = new();
    public Dictionary<int, bool> hardModeCompleted = new();
    public int learnings = 0; 

    [Header("Feedback")]
    public GameObject FeedbackPanel;
    public Coroutine feedbackCoroutine;
    public TMP_Text LearningsText;

    [Header("Periodic Feedback")]
    public List<string> periodicFeedbackMessages = new List<string>
    {
        "You're doing great!",
        "You've got this!",
        "Almost there!",
        "You're learning fast!",
        "Nice work!",
        "Stay focused!",
        "You're on fire! Keep Thinking!",
        "Brilliant! Keep going!",
        "Amazing progress! Keep going!",
        "You're getting better! Keep going!",
        "You can Rotate The Gate by selecting the gate and clicking on the cell",
        "You can Remove The Gate by Right Clicking",
    };
    private Coroutine periodicFeedbackCoroutine;
    private float lastPeriodicFeedbackTime;

    public AudioClip MenuMusic;
    public AudioClip GameMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Only initialize once
        if (gatesToExplain.Count == 0)
        {
            foreach (gates gate in Enum.GetValues(typeof(gates)))
            {
                gatesToExplain.Add(new GateExplainEntry(gate, false));
            }
        }

        // Load completion data
        LoadCompletionData();

        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            //AudioManager.Instance.PlayMusic(MenuMusic);
        }else
        {
            //AudioManager.Instance.PlayMusic(GameMusic);
        }

        if (LearningsText != null)
        {   
            LearningsText.text = $"Learnings: {learnings}";
        }
    }

    public void StartPeriodicFeedback()
    {
        if (periodicFeedbackCoroutine != null)
        {
            StopCoroutine(periodicFeedbackCoroutine);
        }
        periodicFeedbackCoroutine = StartCoroutine(PeriodicFeedbackRoutine());
    }

    public void StopPeriodicFeedback()
    {
        if (periodicFeedbackCoroutine != null)
        {
            StopCoroutine(periodicFeedbackCoroutine);
            periodicFeedbackCoroutine = null;
        }
    }

    private IEnumerator PeriodicFeedbackRoutine()
    {
        while (true)
        {
            // Wait for a random time between 25-50 seconds
            float waitTime = UnityEngine.Random.Range(25f, 50f);
            yield return new WaitForSeconds(waitTime);
            
            // Only show feedback if we're in a game scene and no other feedback is showing
            if (SceneManager.GetActiveScene().name == "0" && feedbackCoroutine == null)
            {
                string randomMessage = periodicFeedbackMessages[UnityEngine.Random.Range(0, periodicFeedbackMessages.Count)];
                ShowFeedback(randomMessage, false);
            }
        }
    }

    public void ShowFeedback(string feedback, bool won, bool forced = false)
    {
        if (feedbackCoroutine != null && !forced)
        {
            return;
        }

        if (feedbackCoroutine != null && forced)
        {
            StopCoroutine(feedbackCoroutine);
        }

        FeedbackPanel.SetActive(true);
        FeedbackPanel.GetComponentInChildren<TMP_Text>().text = feedback;
        int childCount = FeedbackPanel.transform.childCount - 1;
        int randomIndex = UnityEngine.Random.Range(1, childCount);
        if (won) randomIndex = 0;
        FeedbackPanel.transform.GetChild(randomIndex).gameObject.SetActive(true);

        feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay(randomIndex));
    }

    private IEnumerator HideFeedbackAfterDelay(int randomIndex)
    {
        yield return new WaitForSeconds(3f);
        FeedbackPanel.transform.GetChild(randomIndex).gameObject.SetActive(false);
        FeedbackPanel.SetActive(false);
        feedbackCoroutine = null;
    }


    // Mark level as completed for the current game type
    public void MarkLevelCompleted(int levelIndex)
    {
        // Was this level already solved in any mode before this call?
        bool wasSolved =
            (easyModeCompleted.ContainsKey(levelIndex) && easyModeCompleted[levelIndex]) ||
            (hardModeCompleted.ContainsKey(levelIndex) && hardModeCompleted[levelIndex]);

        if (GameType == 0) // Easy mode
        {
            easyModeCompleted[levelIndex] = true;
        }
        else if (GameType == 1) // Hard mode
        {
            hardModeCompleted[levelIndex] = true;
        }

        // If it was not solved before and now it is solved in at least one mode, increment learnings
        bool nowSolved =
            (easyModeCompleted.ContainsKey(levelIndex) && easyModeCompleted[levelIndex]) ||
            (hardModeCompleted.ContainsKey(levelIndex) && hardModeCompleted[levelIndex]);
        if (!wasSolved && nowSolved)
        {
            learnings++;
            LearningsText.text = $"Learnings: {learnings}";
        }

        // Save completion data immediately
        SaveCompletionData();
    }

    // Check if level is completed for the current game type
    public bool IsLevelCompleted(int levelIndex)
    {
        if (GameType == 0) // Easy mode
        {
            return easyModeCompleted.ContainsKey(levelIndex) && easyModeCompleted[levelIndex];
        }
        else if (GameType == 1) // Hard mode
        {
            return hardModeCompleted.ContainsKey(levelIndex) && hardModeCompleted[levelIndex];
        }
        return false;
    }

    // Get completion status for both modes
    public (bool easy, bool hard) GetLevelCompletionStatus(int levelIndex)
    {
        bool easyCompleted = easyModeCompleted.ContainsKey(levelIndex) && easyModeCompleted[levelIndex];
        bool hardCompleted = hardModeCompleted.ContainsKey(levelIndex) && hardModeCompleted[levelIndex];
        return (easyCompleted, hardCompleted);
    }

    // Save completion data to PlayerPrefs
    public void SaveCompletionData()
    {
        // Save easy mode completions
        string easyData = "";
        foreach (var kvp in easyModeCompleted)
        {
            if (kvp.Value) // Only save completed levels
            {
                easyData += kvp.Key + ",";
            }
        }
        PlayerPrefs.SetString("EasyModeCompleted", easyData);

        // Save hard mode completions
        string hardData = "";
        foreach (var kvp in hardModeCompleted)
        {
            if (kvp.Value) // Only save completed levels
            {
                hardData += kvp.Key + ",";
            }
        }
        PlayerPrefs.SetString("HardModeCompleted", hardData);

        // Persist learnings as well (redundant but handy for analytics/UI)
        PlayerPrefs.SetInt("Learnings", learnings);

        PlayerPrefs.Save();
    }

    // Load completion data from PlayerPrefs
    private void LoadCompletionData()
    {
        easyModeCompleted.Clear();
        hardModeCompleted.Clear();

        // Load easy mode completions
        string easyData = PlayerPrefs.GetString("EasyModeCompleted", "");
        if (!string.IsNullOrEmpty(easyData))
        {
            string[] levels = easyData.Split(',');
            foreach (string level in levels)
            {
                if (int.TryParse(level, out int levelIndex))
                {
                    easyModeCompleted[levelIndex] = true;
                }
            }
        }

        // Load hard mode completions
        string hardData = PlayerPrefs.GetString("HardModeCompleted", "");
        if (!string.IsNullOrEmpty(hardData))
        {
            string[] levels = hardData.Split(',');
            foreach (string level in levels)
            {
                if (int.TryParse(level, out int levelIndex))
                {
                    hardModeCompleted[levelIndex] = true;
                }
            }
        }

        // Recompute learnings from union of solved levels across modes
        var uniqueSolved = new System.Collections.Generic.HashSet<int>();
        foreach (var kvp in easyModeCompleted) if (kvp.Value) uniqueSolved.Add(kvp.Key);
        foreach (var kvp in hardModeCompleted) if (kvp.Value) uniqueSolved.Add(kvp.Key);
        learnings = uniqueSolved.Count;
    }
}
