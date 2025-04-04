using UnityEngine;

public class Data : MonoBehaviour
{
    // Static instance accessible from anywhere.
    public static Data Instance { get; private set; }

    // Shared data fields
    public float averageSpeed;
    public float maxSpeed;
    public float elapsedTime;
    public float score;


    public int stopSignStops;
    public int lightSuccessCount;
    public int totalCollisions;

    public float stopSignPenalty;
    public float redPenalty;   
    public float yellowPenalty;
    public float collisionPenalty;
    public float speedingPenalty;

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
            // Optional: Persist across scenes
            DontDestroyOnLoad(gameObject);
        }
    }
}