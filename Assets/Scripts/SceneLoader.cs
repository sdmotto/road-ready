using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadWorldScene()
    {
        SceneManager.LoadScene("routes");
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("menu");
    }

    public void LoadSettingsScene()
    {
        SceneManager.LoadScene("settings");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}