using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void Load(string sr)
    {
        SceneManager.LoadScene(sr);
    }
}
