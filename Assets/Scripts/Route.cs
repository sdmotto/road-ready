using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class Route
{
    public string routeName;
    public List<Vector3> markerPositions = new List<Vector3>();
    public List<Vector3> linePoints = new List<Vector3>();

    private static string RoutesFolder => Path.Combine(Application.persistentDataPath, "Routes");

    public void Save()
    {
        if (!Directory.Exists(RoutesFolder))
            Directory.CreateDirectory(RoutesFolder);

        string json = JsonUtility.ToJson(this, true);
        string filePath = Path.Combine(RoutesFolder, routeName + ".json");

        File.WriteAllText(filePath, json);
        Debug.Log($"[RouteData] Saved route '{routeName}' to {filePath}");
    }

    public static List<Route> LoadAll()
    {
        List<Route> routes = new List<Route>();

        if (!Directory.Exists(RoutesFolder))
            return routes;

        string[] files = Directory.GetFiles(RoutesFolder, "*.json");
        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            Route route = JsonUtility.FromJson<Route>(json);
            if (route != null)
                routes.Add(route);
        }

        Debug.Log($"[RouteData] Loaded {routes.Count} route(s) from {RoutesFolder}");
        return routes;
    }
}

