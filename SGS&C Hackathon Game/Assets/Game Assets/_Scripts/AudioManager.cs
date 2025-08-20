using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;

    public AudioClip WireConnected;
    public AudioClip WireDisconnected;
    public AudioClip GatePlaced;
    public AudioClip click1;
    public AudioClip click0;
    public AudioClip won;
    public AudioClip lose;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // survive scene changes
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject); // kill duplicate managers
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null) return;

        if (audioSource.clip == music && audioSource.isPlaying) return; // don’t restart same music

        audioSource.clip = music;
        audioSource.loop = true;
        audioSource.Play();
    }



    public void PlayWireConnected()
    {
        PlaySound(WireConnected);
    }

    public void PlayClick1()
    {
        PlaySound(click1);
    }

    public void PlayClick0()
    {
        PlaySound(click0);
    }

    public void PlayWireDisconnected()
    {
        PlaySound(WireDisconnected);
    }

    public void PlayGatePlaced()
    {
        PlaySound(GatePlaced);
    }

    public void PlayWon()
    {
        PlaySound(won);
    }

    public void PlayLose()
    {
        PlaySound(lose);
    }

    public void PlayGateRotated()
    {
        // reuse WireDrag but slightly lower pitch
        PlaySound(GatePlaced, pitch: 0.3f, volume: 0.3f);
    }

    public void PlayGateSelected()
    {
        // reuse WireConnected with slightly higher pitch
        PlaySound(WireConnected, pitch: 1.9f, volume: 0.3f);
    }

    public void PlayGateDeSelected()
    {
        // reuse WireDisconnected but softer and lower pitch
        PlaySound(WireDisconnected, pitch: 0.4f, volume: 0.3f);
    }


    public void PlaySound(AudioClip clip, float pitch = 1f, float volume = 1f)
    {
        if (clip == null) return;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume);

        // reset pitch so it doesn’t affect the next sound
        audioSource.pitch = 1f;
    }
}
