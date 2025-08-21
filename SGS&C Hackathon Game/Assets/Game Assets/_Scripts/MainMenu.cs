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

    private List<string> levelFiles = new(); // holds paths for runtime levels

    private void Start()
    {
        noOfLevels = GetLevelCount();
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

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (SceneManager.GetActiveScene().name == "0")
        {
#if UNITY_EDITOR
            // Editor loads directly from assets
            string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { "Assets/Game Assets/LevelSaveSO" });
            List<(string guid, int number)> numberedAssets = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path);
                string[] parts = fileName.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[^1], out int num))
                    numberedAssets.Add((guid, num));
            }

            numberedAssets.Sort((a, b) => a.number.CompareTo(b.number));

            if (GameData.Instance.LevelClicked >= 0 && GameData.Instance.LevelClicked < numberedAssets.Count)
            {
                string path = AssetDatabase.GUIDToAssetPath(numberedAssets[GameData.Instance.LevelClicked].guid);
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

            levelButtons[i].GetComponentInChildren<TMP_Text>().text = $"{i}";
            int a = i;
            levelButtons[i].onClick.AddListener(() => { OnLevelButtonClicked(a); });
        }
    }

    private void OnLevelButtonClicked(int a)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
#if UNITY_EDITOR
            // Delete inside editor
            string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { "Assets/Game Assets/LevelSaveSO" });
            List<(string guid, int number)> numberedAssets = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path);
                string[] parts = fileName.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[^1], out int num))
                    numberedAssets.Add((guid, num));
            }

            numberedAssets.Sort((x, y) => x.number.CompareTo(y.number));

            if (a >= 0 && a < numberedAssets.Count)
            {
                string path = AssetDatabase.GUIDToAssetPath(numberedAssets[a].guid);
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
        InitializeButtons();
    }

    public int GetLevelCount()
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { "Assets/Game Assets/LevelSaveSO" });

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string[] parts = fileName.Split(' ');
            if (parts.Length > 1 && int.TryParse(parts[^1], out _)) count++;
        }
        return count;
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
        string runtimeDir = Path.Combine(Application.persistentDataPath, "LevelSaveSO");
        if (Directory.Exists(runtimeDir))
        {
            foreach (var file in Directory.GetFiles(runtimeDir, "*.json"))
                levelFiles.Add(file);
        }

        return levelFiles.Count;
#endif
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
