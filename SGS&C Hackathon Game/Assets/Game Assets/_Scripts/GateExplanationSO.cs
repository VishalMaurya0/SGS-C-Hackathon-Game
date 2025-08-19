using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GateExplanationSO", menuName = "Scriptable Objects/GateExplanationSO")]
public class GateExplanationSO : ScriptableObject
{
    public List<GateExplanation> gateExplanations = new();
}

[System.Serializable]
public class GateExplanation
{
    public gates gateType;
    public string desc;
}