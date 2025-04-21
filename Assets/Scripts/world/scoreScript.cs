using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class scoreScript : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Base penalty applied for any collision.")]
    public float collisionBasePenalty = 5.0f;

    [Tooltip("Multiplier for the collision severity (using collision relative velocity).")]
    public float collisionSeverityMultiplier = 1.0f;

    [Header("Score Settings")]
    [Tooltip("Starting (maximum) score.")]
    public float maxScore = 100f;

    // Reference to the speed manager
    public speedScript speedManager;

    [Header("Stop Sign Detection")]
    [Tooltip("Reference to the UI Image for the stop sign indicator (invisible by default).")]
    public Image stopSignImage;

    // Internal tracking variables.
    private float totalCollisionPenalty = 0f;
    private float totalSpeedingPenalty = 0f;
    private int collisionCount = 0;
    private float currentScore;
    private float trafficSigPenalty = 0;
    private float turnSigPenalty = 0;
    private float stopSignPenalty = 0;
    private float redPenalty = 0;
    private float yellowPenalty = 0;
    private int totalStopCount = 0;
    private int stopSignStopCount = 0;
    private int lightSuccessCount = 0;
    private int numLeftTurns = 0;
    private int numRightTurns = 0;
    private bool wasMovingLastFrame = false;

    // references to speedScript and timerScript
    public speedScript speedScriptRef;
    public timerScript timerScriptRef;

    // Flag to control whether scoring is active.
    public static bool gradingActive = false;

    public Rigidbody carRigidbody;  


    void Start() {
        // init the score at the beginning of the run.
        currentScore = maxScore; 
    }

    void Update() {
        if (!gradingActive) return;

        // check if car is stopped and increment stop counter, wasMovingLast frame prevents counting additional stops if the car is just sitting there
        if (gradingActive && carRigidbody != null) {
            float currentSpeed = carRigidbody.velocity.magnitude;

            if (currentSpeed < 0.1f && wasMovingLastFrame)
            {
                totalStopCount++;
                Debug.Log($"Stop detected. Total stops: {totalStopCount}");
                wasMovingLastFrame = false;
            }
            else if (currentSpeed >= 0.1f)
            {
                wasMovingLastFrame = true;
            }
        }

        // check if player is speeding
        if (speedManager != null && speedManager.speedLimitText != null)
        {
            string limitText = speedManager.speedLimitText.text;
            string currentSpeedString = speedManager.speedText.text;

            if (limitText != "API Error")
            {
                // Remove non-numeric characters from the text speed limit
                string limitStr    = Regex.Replace(speedManager.speedLimitText.text, @"[^0-9.\-]+", "");
                string currentStr  = Regex.Replace(speedManager.speedText.text, @"[^0-9.\-]+", "");

                if (float.TryParse(limitStr, out float speedLimit) 
                    && float.TryParse(currentStr, out float currentSpeed))
                {
                    if (currentSpeed > speedLimit)
                    {
                        float overSpeed = currentSpeed - speedLimit;
                        float penaltyPerSecond = 0f;
                        
                        if (overSpeed <= 5f)
                        {
                            penaltyPerSecond = 1f;
                        }
                        else if (overSpeed <= 10f)
                        {
                            penaltyPerSecond = 5f;
                        }
                        else // overSpeed >= 11
                        {
                            penaltyPerSecond = 15f;
                        }

                        // Apply the penalty scaled by amount of time spent speeding
                        float penaltyThisFrame = penaltyPerSecond * Time.deltaTime;
                        totalSpeedingPenalty += penaltyThisFrame;
                        currentScore = calculateScore();
                        // Debug.Log("Speeding penalty applied: " + penaltyThisFrame +
                        //           " | Total Speeding Penalty: " + totalSpeedingPenalty +
                        //           " | Current Score: " + currentScore); annoying debug lol
                    }
                }
            }
        }
    }

    // method to intiate grading
    public void StartGrading()
    {
        gradingActive = true;
        ResetScore();  // Reset score at the start if needed.
        Debug.Log("Grading started. Score reset to: " + currentScore);
    }

    // ends scoring
    public void EndGrading(){
        gradingActive = false;
        Debug.Log("Grading ended. Final Score: " + currentScore);

        // score, speed, time, averageSpeed
        Data.Instance.score = currentScore;
        Data.Instance.maxSpeed = speedScriptRef.maxSpeed;
        Data.Instance.elapsedTime = timerScriptRef.GetElapsedTime();
        Data.Instance.averageSpeed = speedScriptRef.GetAverageSpeed();

        // Penalty breakdowns
        Data.Instance.collisionPenalty = totalCollisionPenalty;
        Data.Instance.speedingPenalty = totalSpeedingPenalty;
        Data.Instance.stopSignPenalty = stopSignPenalty;
        Data.Instance.redPenalty = redPenalty;
        Data.Instance.yellowPenalty = yellowPenalty;
        Data.Instance.turnSigPenalty = turnSigPenalty;

        // collision count
        Data.Instance.totalCollisions = collisionCount;

        // For stops
        Data.Instance.stopSignStops = stopSignStopCount;
        Data.Instance.lightSuccessCount = lightSuccessCount;
        Data.Instance.totalStopCount = totalStopCount;
        
        // For number of turns
        Data.Instance.numLeftTurns = numLeftTurns;
        Data.Instance.numRightTurns = numRightTurns; 

        SceneManager.LoadScene("results");
    }


    // Called automatically by Unity when this GameObject collides with another
    // Only processes collisions when grading is active
    void OnCollisionEnter(Collision collision) {
        if (!gradingActive) return;

        // Calculate collision severity based on relative velocity.
        float collisionSeverity = collision.relativeVelocity.magnitude;
        float penalty = collisionBasePenalty + (collisionSeverity * collisionSeverityMultiplier);

        totalCollisionPenalty += penalty;
        collisionCount++;
        currentScore = calculateScore();

        Debug.Log("Collision #" + collisionCount +
                  " | Severity: " + collisionSeverity +
                  " | Penalty: " + penalty +
                  " | Current Score: " + currentScore);
    }
    // Returns the current score.
    public float GetCurrentScore() {
        return currentScore;
    }

    // used in turnScript to get the number of left and right turns
    public void registerTurnCounts(int left, int right) {
        numLeftTurns = left;
        numRightTurns = right;
    }


    // Resets the score and penalty counters.
    public void ResetScore() {
        totalCollisionPenalty = 0f;
        totalSpeedingPenalty = 0f;
        collisionCount = 0;
        yellowPenalty = 0f;
        redPenalty = 0f;
        currentScore = maxScore;
    }

    // checks the penalty to determine if the lack of a stop was at a red light, yellow light, or stop sign
    public void noStop(int penalty) {
        if (!gradingActive) return;
        if(penalty == 5) {
            redPenalty += 5;
        }
        if(penalty == 2) {
            yellowPenalty +=2;
        }
        if(penalty == 10) {
            stopSignPenalty += 10;
        }

        currentScore = calculateScore();

        Debug.Log("Player did not stop fully!" +
            " | Current Score: " + currentScore);
    } 

    // increments total stop sign stops
    public void RegisterStopSignStop() {
        if (!gradingActive) return;
        stopSignStopCount++;
    }

    // adds penalty for not having turn signal on 
    public void noSignal(int penalty)
    {
        turnSigPenalty += penalty;
        Debug.Log("Turn Signal Penalty: " + penalty);
        calculateScore();
    }

    // calculates current score based on all of the current penalities
    private float calculateScore()
    {
        currentScore = Mathf.Max(0, maxScore - totalCollisionPenalty - totalSpeedingPenalty - stopSignPenalty - turnSigPenalty - redPenalty - yellowPenalty - turnSigPenalty);
        // Debug.Log("Current Score: " + currentScore);
        return currentScore;
    }

    // increments number of successful traffic lights
    public void RegisterTrafficLightSuccess() {
        if (!gradingActive) return;
        lightSuccessCount++;   
    }
}
