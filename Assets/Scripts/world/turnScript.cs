using UnityEngine;

public class VehicleHeadingTurnDetector : MonoBehaviour
{
    // Minimum yaw change (in degrees) per frame to be considered turning.
    public float yawChangeThreshold = 0.0001f;
    
    // Minimum duration (in seconds) for a turn to be considered valid.
    public float minTurnDuration = 0.5f;
    
    // Minimum total yaw change (in degrees) required to count as a turn.
    public float cumulativeTurnThreshold = 75f;

    // State variables for tracking a potential turn.
    private bool isTurning = false;
    private float cumulativeTurn = 0f;
    private float turnTimer = 0f;
    
    // Store the previous frame's yaw.
    private float lastYaw;

    void Start()
    {
        // Initialize lastYaw to the starting rotation.
        lastYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        float currentYaw = transform.eulerAngles.y;

        //Debug.Log(currentYaw);

        // Compute the change in yaw between frames, accounting for angle wrapping.
        float deltaYaw = Mathf.DeltaAngle(lastYaw, currentYaw);

        // Check if this frame's yaw change is significant enough to be considered part of a turn.
        if (Mathf.Abs(deltaYaw) > yawChangeThreshold)
        {
            if (!isTurning)
            {
                Debug.Log("starting turning event");
                // Start a new turn event.
                isTurning = true;
                cumulativeTurn = deltaYaw;
                turnTimer = Time.deltaTime;
            }
            else
            {
                // Continue accumulating yaw changes and timing.
                cumulativeTurn += deltaYaw;
                turnTimer += Time.deltaTime;
            }
        }
        else
        {
            // If we were in the middle of a turn but now the yaw change is small,
            // consider that the turn has ended.
            if (isTurning)
            {
                // Check if the turn lasted long enough and the cumulative change qualifies.
                if (turnTimer >= minTurnDuration && Mathf.Abs(cumulativeTurn) >= cumulativeTurnThreshold)
                {
                    if (cumulativeTurn > 0)
                        Debug.Log("Detected turn right.");
                    else
                        Debug.Log("Detected turn left.");
                }
                // Reset turn detection state.
                isTurning = false;
                cumulativeTurn = 0f;
                turnTimer = 0f;
            }
        }

        // Update lastYaw for the next frame.
        lastYaw = currentYaw;
    }
}
