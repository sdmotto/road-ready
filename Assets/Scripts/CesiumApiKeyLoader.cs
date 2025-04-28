using UnityEngine;
using CesiumForUnity;

public class CesiumApiKeyLoader : MonoBehaviour
{
    [Tooltip("Cesium 3D Tileset to update with Google API key.")]
    public Cesium3DTileset cesiumTileset;

    private const string ApiKeyEnvVar = "GOOGLE_API_KEY";
    private const string GoogleTilesBaseUrl = "https://tile.googleapis.com/v1/3dtiles/root.json?key=";

    void Start()
    {
        Debug.Log("[CesiumApiKeyLoader] Start() called.");
        InitializeCesiumTileset();
    }

    private void InitializeCesiumTileset()
    {
        string apiKey = EnvManager.Get(ApiKeyEnvVar);

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError($"[CesiumApiKeyLoader] Failed to load '{ApiKeyEnvVar}' from .env. Ensure it's set and accessible.");
            return;
        }

        if (cesiumTileset == null)
        {
            Debug.LogError("[CesiumApiKeyLoader] Cesium3DTileset reference is missing. Assign it in the Inspector.");
            return;
        }

        string fullUrl = GoogleTilesBaseUrl + apiKey;
        cesiumTileset.url = fullUrl;
    }
}
