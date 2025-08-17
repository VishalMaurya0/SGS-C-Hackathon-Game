using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }

    [Header("References")]



    [Header("Level Buttons")]
    public GameObject ButtonPrefab;
    public List<Button> levelButtons = new();
    public int noOfLevels;
    public string saveFolder = "Assets/Game Assets/LevelSaveSO";

    [Header("Info")]
    public int LevelClicked = 0;
    public int GameType = 0;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Makes sure this object persists across scenes
    }

    private void Start()
    {
        noOfLevels = GetAssetCountInPath();

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        if (ButtonPrefab != null)
            InitializeButtons();
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (SceneManager.GetActiveScene().name == "0")
        {
            // Get all asset GUIDs in the save folder
            string[] guids = AssetDatabase.FindAssets("", new[] { saveFolder });

            if (LevelClicked >= 0 && LevelClicked < guids.Length)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[LevelClicked]);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                var creator = FindAnyObjectByType<LevelCreationFromSO>();
                if (creator != null)
                {
                    creator.LevelSaveSO = (LevelSaveSO)so;
                }
            }
            else
            {
                Debug.LogWarning($"Invalid LevelClicked index {LevelClicked}, total assets = {guids.Length}");
            }
        }
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < noOfLevels; i++)
        {
            GameObject btn = Instantiate(ButtonPrefab, ButtonPrefab.transform.parent.transform);
            btn.SetActive(true);
            levelButtons.Add(btn.GetComponent<Button>());

            levelButtons[i].GetComponentInChildren<TMP_Text>().text = $"{i}";

            int a = i;
            levelButtons[i].onClick.AddListener(() => { OnLevelButtonClicked(a); });
        }
    }

    private void OnLevelButtonClicked(int a)
    {
        SceneManager.LoadScene("0");
        LevelClicked = a;
    }

    public int GetAssetCountInPath()
    {
        // Ensure path ends with "/"
        if (!saveFolder.EndsWith("/")) saveFolder += "/";

        // Get all asset GUIDs in the folder
        string[] assetGUIDs = AssetDatabase.FindAssets("", new[] { saveFolder });

        return assetGUIDs.Length;
    }

    public void SetGameMode(int gameMode)
    {
        GameType = gameMode;
    }
    public void PlayClicked()
    {
        if (GameType == 2)
        {
            SceneManager.LoadScene("Level Builder");
        }
    }

    public void Home()
    {
        SceneManager.LoadScene("Main Menu");
    }

}
