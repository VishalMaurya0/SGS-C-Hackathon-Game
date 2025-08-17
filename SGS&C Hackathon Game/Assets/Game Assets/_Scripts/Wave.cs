using UnityEngine;

public class WaveMotion : MonoBehaviour
{
    [Header("Wave Settings")]
    public float amplitude = 1f;   // How high it waves (peak)
    public float frequency = 2f;   // How fast it waves
    public float speed = 1f;       // Movement speed along the X-axis (optional)

    private Vector3 startPos;

    void Start()
    {
        // Store the initial position
        startPos = transform.position;
    }

    void Update()
    {
        // Calculate new Y offset using sine wave
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;

        // Optional: move in X while waving
        float newX = startPos.x + (Time.time * speed);

        // Apply new position
        transform.position = new Vector3(newX, newY, startPos.z);
    }
}
