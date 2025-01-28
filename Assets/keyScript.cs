using System.Collections;
using System.Collections.Generic;
using CesiumForUnity;
using UnityEngine;

public class keyScript : MonoBehaviour
{
    private string apiKey;
    public Cesium3DTileset cesiumTileset;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("keyScript: Start() called.");

        // Retrieve the API key from the EnvLoader
        Debug.Log("keyScript: Attempting to load 'GOOGLE_API_KEY' from EnvLoader.");
        apiKey = EnvLoader.GetEnv("GOOGLE_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("keyScript: 'apiKey' is null or empty. Unable to proceed.");
        }
        else
        {
            Debug.Log($"keyScript: 'apiKey' successfully retrieved: {apiKey}");
        }

        // Check if we have a valid Cesium3DTileset and valid apiKey
        if (cesiumTileset != null && !string.IsNullOrEmpty(apiKey))
        {
            Debug.Log("keyScript: cesiumTileset is assigned and apiKey is valid. Constructing new URL...");

            // Construct the new URL with the API key
            string baseUrl = "https://tile.googleapis.com/v1/3dtiles/root.json?key=";
            string newUrl = baseUrl + apiKey;

            // Update the URL in the Cesium3DTileset component
            cesiumTileset.url = newUrl;

            Debug.Log($"keyScript: Cesium URL updated to: {newUrl}");
        }
        else
        {
            if (cesiumTileset == null)
            {
                Debug.LogError("keyScript: cesiumTileset is null. Please assign a Cesium3DTileset in the Inspector.");
            }
            Debug.LogError("keyScript: Failed to update Cesium URL. Ensure Cesium3DTileset is assigned and API_KEY is valid.");
        }
    }
}
