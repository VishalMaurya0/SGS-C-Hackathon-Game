using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "CircuitGeneratorSO", menuName = "Scriptable Objects/CircuitGeneratorSO")]
public class CircuitGeneratorSO : ScriptableObject
{
    public int rows;
    public int columns;

    public List<GateOption> gateOptions;
}

[System.Serializable]
public class GateOption
{
    public gates gateType;
    public int amount;
    public TMP_Text text;
}