using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyScript : MonoBehaviour
{
    private string apiKey;

    // Start is called before the first frame update
    void Start()
    {
        // Retrieve the API key from the EnvLoader
        apiKey = EnvLoader.GetEnv("API_KEY");

        // Check if the key was successfully retrieved
        if (!string.IsNullOrEmpty(apiKey))
        {
            Debug.Log("API Key successfully loaded: " + apiKey);
        }
        else
        {
            Debug.LogError("Failed to load the API Key. Make sure it's defined in the .env file.");
        }
    }
}
