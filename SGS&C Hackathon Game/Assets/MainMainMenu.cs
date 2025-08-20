using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMainMenu : MonoBehaviour
{
    public void load(string j)
    {
        SceneManager.LoadScene(j);
    }
}
