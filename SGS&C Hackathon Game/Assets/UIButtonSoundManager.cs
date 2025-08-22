using UnityEngine;
using UnityEngine.UI;

public class GlobalUIButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;
    private static GlobalUIButtonSound instance;

    void Awake()
    {
        // Singleton pattern so only one exists
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        // Subscribe to scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Add listeners to all buttons in the new scene
        foreach (Button button in FindObjectsOfType<Button>())
        {
            button.onClick.AddListener(() => PlayClickSound());
        }
    }

    void PlayClickSound()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
