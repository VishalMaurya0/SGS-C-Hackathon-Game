using System;
using UnityEngine;

public class CircuitCreation : MonoBehaviour
{
    public bool gateMode;
    public bool isDragging;

    public virtual void HandleRightClick(Cell cell)
    {

    }

    public virtual void MakeGate(Cell cell)
    {
    }

    public virtual void RemoveGate(Cell cell)
    {
    }

    public virtual void StartWire(Cell cell)
    {
    }
    
    public virtual void EndWire()
    {

    }
}
