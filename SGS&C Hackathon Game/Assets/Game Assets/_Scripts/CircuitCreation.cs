using System;
using UnityEngine;

public class CircuitCreation : MonoBehaviour
{
    public bool gateMode;
    public bool isDragging;

    public virtual void HandleRightClick(Cell cell)
    {
        //throw new NotImplementedException();
    }

    public virtual void MakeGate(Cell cell)
    {
        //throw new NotImplementedException();
    }

    public virtual void RemoveGate(Cell cell)
    {
        //throw new NotImplementedException();
    }

    public virtual void StartWire(Cell cell)
    {
        //throw new NotImplementedException();
    }
    
    public virtual void EndWire()
    {
        //throw new NotImplementedException();
    }
}
