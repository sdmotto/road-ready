using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadWorldScene()
    {
        SceneManager.LoadScene("routes");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}