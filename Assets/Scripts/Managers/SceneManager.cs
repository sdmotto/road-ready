using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadWorldScene() => UnitySceneManager.LoadScene("world");
    public void LoadRoutesScene() => UnitySceneManager.LoadScene("routes");
    public void LoadMenuScene() => UnitySceneManager.LoadScene("menu");
    public void LoadSettingsScene() => UnitySceneManager.LoadScene("settings");
    public void LoadMoreResultsScene() => UnitySceneManager.LoadScene("moreresults");
    public void LoadResultsScene() => UnitySceneManager.LoadScene("results");
    public void LoadAuthScene() => UnitySceneManager.LoadScene("auth");
    public void LoadScene(string name) => UnitySceneManager.LoadScene(name);
}
