using System;
using UnityEngine;

public class Data : MonoBehaviour
{
    // Static instance accessible from anywhere.
    public static Data Instance { get; private set; }

    public Route route;

    // Shared data fields
    public float averageSpeed;
    public float maxSpeed;
    public float elapsedTime;
    public float score;

    public int totalStopCount;
    public int stopSignStops;
    public int lightSuccessCount;
    public int totalCollisions;
    public int numLeftTurns;
    public int numRightTurns;

    public float stopSignPenalty;
    public float redPenalty;   
    public float yellowPenalty;
    public float collisionPenalty;
    public float speedingPenalty;
    public float turnSigPenalty;

    public RouteStats ToRouteStatsModel()
    {
        return new RouteStats
        {
            AverageSpeed = averageSpeed,
            MaxSpeed = maxSpeed,
            ElapsedTime = elapsedTime,
            Score = score,
            StopSignStops = stopSignStops,
            LightSuccessCount = lightSuccessCount,
            TotalCollisions = totalCollisions,
            NumLeftTurns = numLeftTurns,
            NumRightTurns = numRightTurns,
            StopSignPenalty = stopSignPenalty,
            RedPenalty = redPenalty,
            YellowPenalty = yellowPenalty,
            CollisionPenalty = collisionPenalty,
            SpeedingPenalty = speedingPenalty,
            TurnSigPenalty = turnSigPenalty,
            UserId = Guid.Parse(SupabaseManager.Instance.UserId),
            RouteId = route.Id
        };
    }


    void Awake()
    {
        // Ensure only one instance exists.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Persist across scenes
            DontDestroyOnLoad(gameObject);
        }
    }
}