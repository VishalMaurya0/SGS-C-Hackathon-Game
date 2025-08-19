using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenui : MonoBehaviour
{
    public void Load(string sr)
    {
        SceneManager.LoadScene(sr);
    }
}
