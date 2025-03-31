using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management


public class sceneRoutesToGame : MonoBehaviour
{
    [Tooltip("Enter the name of the game scene to load.")]
    public string gameSceneName = "world";

    public void toGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
