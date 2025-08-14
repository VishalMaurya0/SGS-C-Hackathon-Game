using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSaveSO", menuName = "Scriptable Objects/LevelSaveSO")]
public class LevelSaveSO : ScriptableObject
{
    public List<List<Cell>> gridCells;
    public List<GateOption> gateOptions = new();
    public List<Cell> inputs = new();
}
