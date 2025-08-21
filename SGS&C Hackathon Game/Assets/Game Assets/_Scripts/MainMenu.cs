using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    [Header("Level Buttons")]
    public GameObject ButtonPrefab;
    public List<Button> levelButtons = new();
    public int noOfLevels;

    public TMP_Dropdown rowDropdown;
    public TMP_Dropdown colDropdown;
    public TMP_Text LearningsText;

    [Header("Level Completion Colors")]
    public Color levelFullyCompletedColor;
    public Color levelEasyPartiallyCompletedColor;
    public Color levelHardPartiallyCompletedColor;
    public Color levelNotCompletedColor;

    private List<string> levelFiles = new(); // holds paths for runtime levels

    private void Start()
    {
        noOfLevels = GetLevelCount();
        GameData.Instance.noOfLevels = noOfLevels;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        if (ButtonPrefab != null)
            InitializeButtons();

        // Dropdowns
        rowDropdown.ClearOptions();
        colDropdown.ClearOptions();

        List<string> rowOptions = new();
        for (int i = 4; i <= 10; i++) rowOptions.Add(i.ToString());
        rowDropdown.AddOptions(rowOptions);

        List<string> colOptions = new();
        for (int i = 10; i <= 20; i++) colOptions.Add(i.ToString());
        colDropdown.AddOptions(colOptions);
    }

    void Update()
    {
        if (LearningsText != null)
        {   
            LearningsText.text = $"Learnings: {GameData.Instance.learnings}";
        }
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (SceneManager.GetActiveScene().name == "0")
        {
#if UNITY_EDITOR
            // Editor: order by LevelSaveSO.priority
            string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { "Assets/Game Assets/LevelSaveSO" });
            List<(string guid, float priority)> ordered = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelSaveSO so = AssetDatabase.LoadAssetAtPath<LevelSaveSO>(path);
                float prio = ExtractPriorityFromAsset(so);
                ordered.Add((guid, prio));
            }

            ordered.Sort((a, b) => a.priority.CompareTo(b.priority));

            if (GameData.Instance.LevelClicked >= 0 && GameData.Instance.LevelClicked < ordered.Count)
            {
                string path = AssetDatabase.GUIDToAssetPath(ordered[GameData.Instance.LevelClicked].guid);
                LevelSaveSO so = AssetDatabase.LoadAssetAtPath<LevelSaveSO>(path);

                var creator = FindAnyObjectByType<LevelCreationFromSO>();
                if (creator != null) creator.LevelSaveSO = so;
            }
#else
            // Runtime: load JSON levels (StreamingAssets + persistentDataPath)
            if (GameData.Instance.LevelClicked >= 0 && GameData.Instance.LevelClicked < levelFiles.Count)
            {
                string json = File.ReadAllText(levelFiles[GameData.Instance.LevelClicked]);
                LevelSaveSO so = ScriptableObject.CreateInstance<LevelSaveSO>();
                JsonUtility.FromJsonOverwrite(json, so);

                var creator = FindAnyObjectByType<LevelCreationFromSO>();
                if (creator != null) creator.LevelSaveSO = so;
            }
#endif
        }
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < noOfLevels; i++)
        {
            GameObject btn = Instantiate(ButtonPrefab, ButtonPrefab.transform.parent);
            btn.SetActive(true);
            levelButtons.Add(btn.GetComponent<Button>());

            // Get completion status for this level
            var (easyCompleted, hardCompleted) = GameData.Instance.GetLevelCompletionStatus(i);
            
            // Create button text with completion indicators
            string buttonText = $"{i}";
            if (easyCompleted || hardCompleted)
            {
                // buttonText += "\n";
                // if (easyCompleted) buttonText += "✓";
                // if (hardCompleted) buttonText += "★";
            }
            
            levelButtons[i].GetComponentInChildren<TMP_Text>().text = buttonText;
            
            // Change button color based on completion status
            var buttonImage = levelButtons[i].GetComponent<Image>();
            if (easyCompleted && hardCompleted)
            {
                buttonImage.color = levelFullyCompletedColor; // Both completed
            }
            else if (easyCompleted && !hardCompleted)
            {
                buttonImage.color = levelEasyPartiallyCompletedColor; // One mode completed
            }
            else if (!easyCompleted && hardCompleted)
            {
                buttonImage.color = levelHardPartiallyCompletedColor; // One mode completed
            }
            else
            {
                buttonImage.color = levelNotCompletedColor; // Not completed
            }
            
            int a = i;
            levelButtons[i].onClick.AddListener(() => { OnLevelButtonClicked(a); });
        }
    }

    private void OnLevelButtonClicked(int a)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
#if UNITY_EDITOR
            // Delete inside editor (ordered by priority)
            string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { "Assets/Game Assets/LevelSaveSO" });
            List<(string guid, float priority)> ordered = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelSaveSO so = AssetDatabase.LoadAssetAtPath<LevelSaveSO>(path);
                float prio = ExtractPriorityFromAsset(so);
                ordered.Add((guid, prio));
            }

            ordered.Sort((x, y) => x.priority.CompareTo(y.priority));

            if (a >= 0 && a < ordered.Count)
            {
                string path = AssetDatabase.GUIDToAssetPath(ordered[a].guid);
                if (AssetDatabase.DeleteAsset(path))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"Deleted LevelSaveSO at index {a}: {path}");
                    RefreshButtons();
                }
            }
#else
            // Delete runtime JSON file
            if (a >= 0 && a < levelFiles.Count)
            {
                File.Delete(levelFiles[a]);
                Debug.Log($"[RUNTIME] Deleted {levelFiles[a]}");
                RefreshButtons();
            }
#endif
        }
        else
        {
            GameData.Instance.LevelClicked = a;
            SceneManager.LoadScene("0");
        }
    }

    private void RefreshButtons()
    {
        foreach (var btn in levelButtons) Destroy(btn.gameObject);
        levelButtons.Clear();

        noOfLevels = GetLevelCount();
        GameData.Instance.noOfLevels = noOfLevels;
        InitializeButtons();
    }

    public int GetLevelCount()
    {
#if UNITY_EDITOR
        // Editor: count by assets, but order elsewhere by priority
        string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { "Assets/Game Assets/LevelSaveSO" });
        return guids.Length;
#else
        levelFiles.Clear();

        // 1. Built-in levels (StreamingAssets)
        string streamingDir = Path.Combine(Application.streamingAssetsPath, "Levels");
        if (Directory.Exists(streamingDir))
        {
            foreach (var file in Directory.GetFiles(streamingDir, "*.json"))
                levelFiles.Add(file);
        }

        // 2. Player-created levels (persistentDataPath)
        // Support both legacy folder name and current one
        string runtimeDirLegacy = Path.Combine(Application.persistentDataPath, "LevelSaveSO");
        if (Directory.Exists(runtimeDirLegacy))
        {
            foreach (var file in Directory.GetFiles(runtimeDirLegacy, "*.json"))
                levelFiles.Add(file);
        }

        string runtimeDir = Path.Combine(Application.persistentDataPath, "LevelSaves");
        if (Directory.Exists(runtimeDir))
        {
            foreach (var file in Directory.GetFiles(runtimeDir, "*.json"))
                levelFiles.Add(file);
        }

        // Sort by priority from JSON (ascending). Fallback: alphabetical if no priority.
        levelFiles.Sort((x, y) =>
        {
            int px = ExtractPriorityFromJson(x);
            int py = ExtractPriorityFromJson(y);
            if (px != (float)int.MaxValue || py != (float)int.MaxValue)
                return px.CompareTo(py);
            return string.Compare(Path.GetFileName(x), Path.GetFileName(y), StringComparison.OrdinalIgnoreCase);
        });

        return levelFiles.Count;
#endif
    }

    private float ExtractPriorityFromAsset(LevelSaveSO so)
    {
        try { return so != null ? so.priority : (float)int.MaxValue; } catch { return int.MaxValue; }
    }

    [System.Serializable]
    private class LevelMeta { public int priority; }

    private int ExtractPriorityFromJson(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            var meta = JsonUtility.FromJson<LevelMeta>(json);
            if (meta != null) return meta.priority;
        }
        catch { }
        return int.MaxValue;
    }

    public void SetGameMode(int gameMode) => GameData.Instance.GameType = gameMode;

    public void PlayClicked()
    {
        if (GameData.Instance.GameType == 2)
        {
            SceneManager.LoadScene("Level Builder");

            int selectedRow = int.Parse(rowDropdown.options[rowDropdown.value].text);
            int selectedCol = int.Parse(colDropdown.options[colDropdown.value].text);

            GameData.Instance.row = selectedRow;
            GameData.Instance.col = selectedCol;
        }
    }

    public void Home() => SceneManager.LoadScene("Main Menu");

    public void ResetGateKnowledge()
    {
        GameData.Instance.gatesToExplain.Clear();
        foreach (gates gate in Enum.GetValues(typeof(gates)))
            GameData.Instance.gatesToExplain.Add(new GateExplainEntry(gate, false));
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null) AudioManager.Instance.audioSource.PlayOneShot(clip);
    }
}

[System.Serializable]
public class GateExplainEntry
{
    public gates gateType;
    public bool explained;

    public GateExplainEntry(gates gate, bool explained)
    {
        this.gateType = gate;
        this.explained = explained;
    }
}
