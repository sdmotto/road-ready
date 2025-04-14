using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    public static RouteManager Instance;

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

    public async Task<List<Route>> GetAllRoutesForUserAsync()
    {
        var client = SupabaseManager.Instance.GetClient();
        var userId = SupabaseManager.Instance.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("User is not signed in.");
            return new List<Route>();
        }

        var response = await client
            .From<Route>()
            .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
            .Get();

        return response.Models;
    }

    public async Task<bool> SaveRouteAsync(Route route)
    {
        var client = SupabaseManager.Instance.GetClient();
        var userId = SupabaseManager.Instance.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Cannot save route: user not authenticated.");
            return false;
        }

        route.UserId = Guid.Parse(userId);

        try
        {
            await client.From<Route>().Insert(route);
            Debug.Log($"Saved route '{route.RouteName}' for user {userId}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save route: {ex.Message}");
            return false;
        }
    }
}
