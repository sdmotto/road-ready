using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EnvLoader : MonoBehaviour
{
    // Dictionary to store all key-value pairs from the .env file
    public static Dictionary<string, string> EnvVariables = new Dictionary<string, string>();

    void Awake()
    {
        // Path to the .env file
        string envPath = Path.Combine(Application.dataPath, ".env");

        if (File.Exists(envPath))
        {
            string[] lines = File.ReadAllLines(envPath);
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // Split the line into key and value
                int separatorIndex = line.IndexOf('=');
                if (separatorIndex > 0)
                {
                    string key = line.Substring(0, separatorIndex).Trim();
                    string value = line.Substring(separatorIndex + 1).Trim();
                    EnvVariables[key] = value;
                }
            }

            Debug.Log("Environment variables loaded successfully!");
        }
        else
        {
            Debug.LogError(".env file not found at: " + envPath);
        }
    }

    // Helper method to get a variable by key
    public static string GetEnv(string key)
    {
        if (EnvVariables.ContainsKey(key))
        {
            return EnvVariables[key];
        }
        else
        {
            Debug.LogWarning($"Environment variable '{key}' not found!");
            return null;
        }
    }
}
