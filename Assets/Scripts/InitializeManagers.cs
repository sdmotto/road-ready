using UnityEngine;

public class InitializeManagers : MonoBehaviour
{
    private static bool managersInitialized = false;

    void Awake()
    {
        if (managersInitialized) return;

        managersInitialized = true;

        EnsureManager<SupabaseManager>("SupabaseManager");
        EnsureManager<RouteManager>("RouteManager");
        EnsureManager<RouteStatsManager>("RouteStatsManager");
    }

    // makes sure all 'managers' are properly initialized
    private void EnsureManager<T>(string name) where T : MonoBehaviour
    {
        if (FindObjectOfType<T>() == null)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
            DontDestroyOnLoad(go);
        }
    }
}
