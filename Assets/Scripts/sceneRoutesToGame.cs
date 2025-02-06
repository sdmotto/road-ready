using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management


public class sceneRoutesToGame : MonoBehaviour
{
    [Tooltip("Enter the name of the game scene to load.")]
    public string gameSceneName = "world";

    void Update()
    {
        // Check if the user presses the G key
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Load the scene specified by gameSceneName
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
