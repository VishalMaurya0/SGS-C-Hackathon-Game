using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSaveSO", menuName = "Scriptable Objects/LevelSaveSO")]
public class LevelSaveSO : ScriptableObject
{
    public List<CellData> cellData = new();
    public List<InputCellData> inputData = new();
    public List<bool> outputData = new();
    public float cellSize;
    public int rows;
    public int cols;
    public float priority;
}


[System.Serializable]
public class CellData
{
    public int value;
    public bool isSource;
    public gates gate;
    public bool isGate;
    public int outputDir;
    public int sourceID;
    public List<bool> connections = new (4);
}

[System.Serializable]
public class InputCellData
{
    public int value;
    public bool isSource;
    public int sourceID;
    public List<bool> connections = new(4);
}
