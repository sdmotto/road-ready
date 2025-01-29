using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadWorldScene()
    {
        SceneManager.LoadScene("world");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}