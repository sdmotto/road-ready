using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public void LoadWorld()
    {
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadWorldScene();
        }
        else
        {
            Debug.LogError("[ButtonHandler] SceneManager.Instance is null!");
        }
    }

    public void LoadMenu()
    {
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadMenuScene();
        }
        else
        {
            Debug.LogError("[ButtonHandler] SceneManager.Instance is null!");
        }
    }

    public void LoadRoutes()
    {
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadRoutesScene();
        }
        else
        {
            Debug.LogError("[ButtonHandler] SceneManager.Instance is null!");
        }
    }

    public void LoadSettings()
    {
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadSettingsScene();
        }
        else
        {
            Debug.LogError("[ButtonHandler] SceneManager.Instance is null!");
        }
    }

    public void LoadMoreResults()
    {
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadMoreResultsScene();
        }
        else
        {
            Debug.LogError("[ButtonHandler] SceneManager.Instance is null!");
        }
    }
}
