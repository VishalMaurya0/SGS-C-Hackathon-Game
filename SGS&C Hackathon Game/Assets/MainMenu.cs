using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class MainMenui : MonoBehaviour
{
    public static int reactions = 0;
    public TextMeshProUGUI tm;
    public void Load(string sr)
    {
        SceneManager.LoadScene(sr);
    }

    void Update()
    {
        if (reactions < 12)
        {
            tm.text = reactions.ToString();
        }
        else
        {
            tm.text = "All reactions completed";
        }
    }
}