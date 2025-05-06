using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class EnvManager
{
    private static Dictionary<string, string> env = new Dictionary<string, string>();
    private static bool isLoaded = false;

    // Load in variables from .env
    public static void LoadEnv()
    {
        if (isLoaded) return;
        isLoaded = true;

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string envPath = Path.Combine(projectRoot, ".env");

        if (!File.Exists(envPath))
        {
            Debug.LogWarning($"[EnvManager] .env file not found at: {envPath}");
            return;
        }

        string[] lines = File.ReadAllLines(envPath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            int eq = line.IndexOf('=');
            if (eq < 0) continue;

            string key = line.Substring(0, eq).Trim();
            string value = line.Substring(eq + 1).Trim();
            env[key] = value;
        }

        Debug.Log($"[EnvManager] Loaded {env.Count} environment variables from {envPath}");
    }

    // get .env variable
    public static string Get(string key)
    {
        LoadEnv();

        if (!env.ContainsKey(key))
        {
            Debug.LogWarning($"[EnvManager] Key '{key}' not found in .env");
            return null;
        }

        return env[key];
    }
}
