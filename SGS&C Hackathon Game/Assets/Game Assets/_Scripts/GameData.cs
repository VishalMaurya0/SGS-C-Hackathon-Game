using System;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    public List<GateExplainEntry> gatesToExplain = new();

    public bool isFirst = true;


    [Header("Info")]
    public int LevelClicked = 0;
    public int GameType = 0;
    public int row = 6;
    public int col = 10;
    

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
    }
}
