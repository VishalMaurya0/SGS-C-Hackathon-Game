using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CircuitGeneratorSO", menuName = "Scriptable Objects/CircuitGeneratorSO")]
public class CircuitGeneratorSO : ScriptableObject
{
    public int rows;
    public int columns_MakeItOdd;
    [Tooltip("Output.Count == column")]public List<bool> Output;
}
