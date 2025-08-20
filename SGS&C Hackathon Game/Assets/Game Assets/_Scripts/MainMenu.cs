using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    

    [Header("Level Buttons")]
    public GameObject ButtonPrefab;
    public List<Button> levelButtons = new();
    public int noOfLevels;
    public string saveFolder = "Assets/Game Assets/LevelSaveSO";

    public TMP_Dropdown rowDropdown;
    public TMP_Dropdown colDropdown;





    private void Start()
    {
        noOfLevels = GetAssetCountInPath();

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        if (ButtonPrefab != null)
            InitializeButtons();



        //dropdown
        rowDropdown.ClearOptions();
        colDropdown.ClearOptions();

        // Fill rows (4 to 10)
        List<string> rowOptions = new List<string>();
        for (int i = 4; i <= 10; i++)
        {
            rowOptions.Add(i.ToString());
        }
        rowDropdown.AddOptions(rowOptions);

        // Fill columns (10 to 20)
        List<string> colOptions = new List<string>();
        for (int i = 10; i <= 20; i++)
        {
            colOptions.Add(i.ToString());
        }
        colDropdown.AddOptions(colOptions);
    }


    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)        // Champt Gpt
    {
        if (SceneManager.GetActiveScene().name == "0")
        {
            // Get all asset GUIDs in the save folder
            string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { saveFolder });

            // Convert to (path, name) and filter only numbered ones
            List<(string guid, int number)> numberedAssets = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

                // Try to parse number from the file name
                string[] parts = fileName.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[^1], out int num))
                {
                    numberedAssets.Add((guid, num));
                }
            }

            // Sort by number so LevelSaveSO 0,1,2,... are in order
            numberedAssets.Sort((a, b) => a.number.CompareTo(b.number));

            if (GameData.Instance.LevelClicked >= 0 && GameData.Instance.LevelClicked < numberedAssets.Count)
            {
                string path = AssetDatabase.GUIDToAssetPath(numberedAssets[GameData.Instance.LevelClicked].guid);
                LevelSaveSO so = AssetDatabase.LoadAssetAtPath<LevelSaveSO>(path);

                var creator = FindAnyObjectByType<LevelCreationFromSO>();
                if (creator != null)
                {
                    creator.LevelSaveSO = so;
                }
            }
            else
            {
                Debug.LogWarning($"Invalid LevelClicked index {GameData.Instance.LevelClicked}, total numbered assets = {numberedAssets.Count}");
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
        if (Input.GetKey(KeyCode.LeftShift))   
        {
            // Find the corresponding asset
            string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { saveFolder });
            List<(string guid, int number)> numberedAssets = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

                string[] parts = fileName.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[^1], out int num))
                {
                    numberedAssets.Add((guid, num));
                }
            }

            numberedAssets.Sort((x, y) => x.number.CompareTo(y.number));

            if (a >= 0 && a < numberedAssets.Count)
            {
                string path = AssetDatabase.GUIDToAssetPath(numberedAssets[a].guid);
                bool success = AssetDatabase.DeleteAsset(path);
                if (success)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"Deleted LevelSaveSO at index {a}: {path}");

                    // Also refresh buttons in UI
                    foreach (var btn in levelButtons)
                    {
                        Destroy(btn.gameObject);
                    }
                    levelButtons.Clear();
                    noOfLevels = GetAssetCountInPath();
                    InitializeButtons();
                }
                else
                {
                    Debug.LogWarning($"Failed to delete LevelSaveSO {path}");
                }
            }
        }
        else
        {
            GameData.Instance.LevelClicked = a;
            SceneManager.LoadScene("0");
        }
    }


    public int GetAssetCountInPath()
    {
        // Ensure path ends with "/"
        if (!saveFolder.EndsWith("/")) saveFolder += "/";

        // Get all LevelSaveSO assets
        string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { saveFolder });

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            // Only count those with a number at the end (e.g. "LevelSaveSO 0")
            string[] parts = fileName.Split(' ');
            if (parts.Length > 1 && int.TryParse(parts[^1], out _))
            {
                count++;
            }
        }

        return count;
    }


    public void SetGameMode(int gameMode)
    {
        GameData.Instance.GameType = gameMode;
    }
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

    public void Home()
    {
        SceneManager.LoadScene("Main Menu");
    }


    public void ResetGateKnowledge()
    {
        GameData.Instance.gatesToExplain.Clear();
        foreach (gates gate in Enum.GetValues(typeof(gates)))
        {
            GameData.Instance.gatesToExplain.Add(new GateExplainEntry(gate, false));
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            AudioManager.Instance.audioSource.PlayOneShot(clip);
    }
}


[System.Serializable]
public class GateExplainEntry
{
    public gates gateType;
    public bool explained;

    public GateExplainEntry(gates gates, bool explained)
    {
        this.gateType = gates;
        this.explained = explained;
    }

}