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

    // Reference to the speed manager (assumes speedScript exposes currentSpeed and speedLimitText).
    public speedScript speedManager;

    [Header("Stop Sign Detection")]
    [Tooltip("Reference to the UI Image for the stop sign indicator (invisible by default).")]
    public Image stopSignImage;

    // Internal tracking variables.
    private float totalCollisionPenalty = 0f;
    private float totalSpeedingPenalty = 0f;
    private int collisionCount = 0;
    private float currentScore;
    private float stopPenalty = 0;
    private float trafficSigPenalty = 0;

    // references to speedScript and timerScript
    public speedScript speedScriptRef;
    public timerScript timerScriptRef;

    // Flag to control whether scoring is active.
    private bool gradingActive = false;

    void Start()
    {
        // Initialize the score at the beginning of the run.
        currentScore = maxScore;
    }

    void Update()
    {
        if (!gradingActive)
            return;

        // Check speeding penalty as before.
        if (speedManager != null && speedManager.speedLimitText != null)
        {
            string limitText = speedManager.speedLimitText.text;
            string currentSpeedString = speedManager.speedText.text;

            if (limitText != "API Error")
            {
                // Remove non-numeric characters from the text.
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

                        // Apply the penalty scaled by time.
                        float penaltyThisFrame = penaltyPerSecond * Time.deltaTime;
                        totalSpeedingPenalty += penaltyThisFrame;
                        currentScore = Mathf.Max(0, maxScore - totalCollisionPenalty - totalSpeedingPenalty - stopPenalty);
                        Debug.Log("Speeding penalty applied: " + penaltyThisFrame +
                                  " | Total Speeding Penalty: " + totalSpeedingPenalty +
                                  " | Current Score: " + currentScore);
                    }
                }
            }
        }
    }

    //Activates collision and speeding scoring.
    //Call this method (e.g., from your start marker trigger) to begin grading.
    public void StartGrading()
    {
        gradingActive = true;
        ResetScore();  // Reset score at the start if needed.
        Debug.Log("Grading started. Score reset to: " + currentScore);
    }

    // Deactivates scoring.
    //Call this method o end grading.
    public void EndGrading(){
        gradingActive = false;
        Debug.Log("Grading ended. Final Score: " + currentScore);

        Data.Instance.score = currentScore;
        Data.Instance.maxSpeed = speedScriptRef.maxSpeed;
        Data.Instance.elapsedTime = timerScriptRef.GetElapsedTime();
        Data.Instance.averageSpeed = speedScriptRef.GetAverageSpeed();
        Debug.Log("Samples: " + speedScriptRef.speedSamples); // print number of samples
        Debug.Log("Avg Speed: " + speedScriptRef.GetAverageSpeed()); // print avg speed
        SceneManager.LoadScene("results");
    }


    // Called automatically by Unity when this GameObject collides with another.
    //Only processes collisions when grading is active.

    void OnCollisionEnter(Collision collision)
    {
        if (!gradingActive)
            return;

        // Calculate collision severity based on relative velocity.
        float collisionSeverity = collision.relativeVelocity.magnitude;
        float penalty = collisionBasePenalty + (collisionSeverity * collisionSeverityMultiplier);

        totalCollisionPenalty += penalty;
        collisionCount++;
        currentScore = Mathf.Max(0, maxScore - totalCollisionPenalty - totalSpeedingPenalty - stopPenalty);

        Debug.Log("Collision #" + collisionCount +
                  " | Severity: " + collisionSeverity +
                  " | Penalty: " + penalty +
                  " | Current Score: " + currentScore);
    }

   
    // Detect stop sign zone violations.
    // When the player leaves a stop sign zone (tagged "StopSignZone") while the stop sign image is still active,
    // it means they did not come to a complete stop and are penalized 10 points.
  

    // Returns the current score.
    public float GetCurrentScore()
    {
        return currentScore;
    }


    // Resets the score and penalty counters.

    public void ResetScore()
    {
        totalCollisionPenalty = 0f;
        totalSpeedingPenalty = 0f;
        collisionCount = 0;
        currentScore = maxScore;
    }

    public void noStop(int penalty)
    {
        stopPenalty += penalty;

        currentScore = Mathf.Max(0, maxScore - totalCollisionPenalty - totalSpeedingPenalty - stopPenalty - trafficSigPenalty);

        Debug.Log("Player did not stop fully!" +
            " | Current Score: " + currentScore);
    } 

    public void trafficSignal(int penalty)
    {
        trafficSigPenalty += penalty;

        currentScore = Mathf.Max(0, maxScore - totalCollisionPenalty - totalSpeedingPenalty - stopPenalty - trafficSigPenalty);

        Debug.Log("Player did not obey traffic signal laws!" +
            " | Current Score: " + currentScore);
    }
}
