using UnityEngine;

public class VehicleHeadingTurnDetector : MonoBehaviour
{
    // Yaw change (degrees) per sample required to consider that sample as turning
    public float yawChangeThreshold = 1f;

    // Total yaw change (degrees) required between the first turning sample and the last straight sample to count as a turn
    public float cumulativeTurnThreshold = 45f;

    // Sampling interval (seconds)
    public float samplingInterval = 0.1f;

    // Number of consecutive samples above threshold required to start a turn event
    public int consecutiveTurnSamplesToStart = 3;

    // Number of consecutive samples below threshold required to end a turn event
    public int consecutiveStraightSamplesToEnd = 5;

    public Rigidbody carRigidbody; // car rigidbody

    public int numLeftTurns = 0;
    public int numRightTurns = 0;

    // State variables
    private bool isTurning = false;
    private float cumulativeTurn = 0f;  // Total yaw change accumulated during a turn event
    private int consecutiveTurnCount = 0; // Counts consecutive samples above threshold before turn event
    private int consecutiveStraightCount = 0; // Counts consecutive samples below threshold during an event
    private float tempAccumulatedTurn = 0f; // Accumulates yaw changes before a turn event officially begins


    // Timer for sampling
    private float sampleTimer = 0f;

    // Store the previous sample's yaw value
    private float lastYaw;

    // method to check if user is reversing so that a turn is not detected when reversing
    private bool IsReversing()
    {
        // Convert world velocity to local space
        Vector3 localVelocity = transform.InverseTransformDirection(carRigidbody.velocity);
        return localVelocity.z < -0.1f; // Negative z = reversing
    }

    public scoreScript scoreScript;

    void Start()
    {
        // Initialize lastYaw to the current rotation at start
        lastYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (IsReversing())
        {
            // Skip turn detection if reversing
            return;
        }
        // Increase the sample timer
        sampleTimer += Time.deltaTime;
        if (sampleTimer < samplingInterval)
            return;  // Not time to sample yet

        // Reset the sample timer
        sampleTimer = 0f;

        // Get the current yaw
        float currentYaw = transform.eulerAngles.y;
        // Compute the change in yaw since the last sample 
        float deltaYaw = Mathf.DeltaAngle(lastYaw, currentYaw);
        // Update lastYaw for the next sample
        lastYaw = currentYaw;

        // if not already in a turning event
        if (!isTurning)
        {
            if (Mathf.Abs(deltaYaw) > yawChangeThreshold)
            {
                // Count this sample as a turning sample
                consecutiveTurnCount++;
                tempAccumulatedTurn += deltaYaw;

                // If there are enough consecutive samples, begin turn event
                if (consecutiveTurnCount >= consecutiveTurnSamplesToStart)
                {
                    // Debug.Log("begin turning event");
                    isTurning = true;
                    cumulativeTurn = tempAccumulatedTurn; // Start accumulating from the first turning sample
                    // Reset the straight sample counter 
                    consecutiveStraightCount = 0;
                }
            }
            else
            {
                // Reset if the sample doesn't exceed the threshold
                consecutiveTurnCount = 0;
                tempAccumulatedTurn = 0f;
            }
        }
        else // If in a turning event
        {
            // add the current sample's delta to the cumulative turn.
            cumulativeTurn += deltaYaw;

            // Check if the current sample indicates straight
            if (Mathf.Abs(deltaYaw) <= yawChangeThreshold)
            {
                consecutiveStraightCount++;
            }
            else
            {
                // Reset straight counter if a significant change is detected
                consecutiveStraightCount = 0;
            }

            // If there are five consecutive straight samples end the turn event
            if (consecutiveStraightCount >= consecutiveStraightSamplesToEnd)
            {
                // Evaluate if the total accumulated yaw change qualifies as a turn
                if (Mathf.Abs(cumulativeTurn) >= cumulativeTurnThreshold)
                {
                    if (cumulativeTurn > 0)
                    {
                        // Debug.Log("Detected turn right.");
                        numRightTurns++;
                        if (!TurnSignalController.rightActive && scoreScript.gradingActive)
                        {
                            // Debug.Log("Player did not have right signal active!");
                            scoreScript.noSignal(3);
                        }
                    }
                    else
                    {
                        // Debug.Log("Detected turn left.");
                        numLeftTurns++;
                        if (!TurnSignalController.leftActive && scoreScript.gradingActive)
                        {
                            // Debug.Log("Player did not have left signal active!");
                            scoreScript.noSignal(3);
                        }
                    }
                }
                // Reset all state variables for the next event.
                isTurning = false;
                cumulativeTurn = 0f;
                consecutiveTurnCount = 0;
                tempAccumulatedTurn = 0f;
                consecutiveStraightCount = 0;
            }
        }
        scoreScript.registerTurnCounts(numLeftTurns, numRightTurns); // pass number of turns to scoreScript
    }
}
