using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GateTypes", menuName = "Scriptable Objects/GateTypes")]
public class GateTypesSO : ScriptableObject
{
    public List<GateBehaviour> gates = new List<GateBehaviour>();
}
