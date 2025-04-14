using System;
using System.Threading.Tasks;
using UnityEngine;

public class RouteStatsManager : MonoBehaviour
{
    public static RouteStatsManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> SaveRouteStatsAsync(RouteStats stats)
    {
        var client = SupabaseManager.Instance.GetClient();
        var userIdString = SupabaseManager.Instance.UserId;

        if (string.IsNullOrEmpty(userIdString))
        {
            Debug.LogError("Cannot save RouteStats: user not authenticated.");
            return false;
        }

        // Assign user ID
        stats.UserId = Guid.Parse(userIdString);

        try
        {
            await client.From<RouteStats>().Insert(stats);
            Debug.Log($"[RouteStatsManager] RouteStats saved for route ID {stats.RouteId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RouteStatsManager] Failed to save RouteStats: {ex.Message}");
            return false;
        }
    }
}
