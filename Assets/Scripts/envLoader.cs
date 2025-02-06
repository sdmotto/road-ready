using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EnvLoader : MonoBehaviour
{
    public static Dictionary<string, string> EnvVariables = new Dictionary<string, string>();

    void Awake()
    {
        Debug.Log("EnvLoader: Awake called. Starting environment load process.");

        // Print the current Application.dataPath.
        Debug.Log($"EnvLoader: Application.dataPath = {Application.dataPath}");

        // Attempt to find the project root by going up one level from 'Assets'
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        Debug.Log($"EnvLoader: Computed projectRoot = {projectRoot}");

        // Build the full path to the .env file in the root
        string envPath = Path.Combine(projectRoot, ".env");
        Debug.Log($"EnvLoader: Using envPath = {envPath}");

        if (!File.Exists(envPath))
        {
            Debug.LogError($"EnvLoader: .env file NOT found at {envPath}");
            return;
        }

        // Read all lines from the .env
        string[] lines = File.ReadAllLines(envPath);
        Debug.Log($"EnvLoader: Read {lines.Length} lines from .env at {envPath}");

        foreach (string line in lines)
        {
            Debug.Log($"EnvLoader: Processing line: '{line}'");
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
            {
                Debug.Log("EnvLoader: Skipping empty/comment line.");
                continue;
            }

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0)
            {
                Debug.LogWarning($"EnvLoader: No '=' in line: '{line}', skipping.");
                continue;
            }

            string key = line.Substring(0, separatorIndex).Trim();
            string value = line.Substring(separatorIndex + 1).Trim();

            Debug.Log($"EnvLoader: Found key='{key}', value='{value}'");
            EnvVariables[key] = value;
        }

        Debug.Log($"EnvLoader: Successfully loaded {EnvVariables.Count} environment variables from the root .env.");
    }

    public static string GetEnv(string key)
    {
        Debug.Log($"EnvLoader.GetEnv: Attempting to retrieve key='{key}' from dictionary.");
        if (!EnvVariables.ContainsKey(key))
        {
            Debug.LogError($"EnvLoader.GetEnv: Key '{key}' not found! Check spelling and .env.");
            Debug.Log("EnvLoader.GetEnv: Available keys in EnvVariables are:");
            foreach (var kvp in EnvVariables)
            {
                Debug.Log($"   {kvp.Key} = {kvp.Value}");
            }
            return null;
        }

        Debug.Log($"EnvLoader.GetEnv: Key '{key}' found, value = '{EnvVariables[key]}'");
        return EnvVariables[key];
    }
}
