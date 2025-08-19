using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Inputs")]
    public KeyCode upKey;
    public KeyCode downKey;

    [Header("Values")]
    public int inputValue = 0;

    private void Update()
    {
        if (Input.GetKey(upKey))
        {
            inputValue = 1;
        }
        
        if (Input.GetKey(downKey))
        {
            inputValue = -1;
        }
    }


}
