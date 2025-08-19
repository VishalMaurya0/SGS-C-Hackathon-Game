using UnityEngine;

public class fire : MonoBehaviour
{
    public GameObject y;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sphere"))
        {
            y.SetActive(true);
        }
    }
}
