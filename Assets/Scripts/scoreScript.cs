using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Internal tracking variables.
    private float totalCollisionPenalty = 0f;
    private int collisionCount = 0;
    private float currentScore;

    // Flag to control whether collisions affect the score.
    private bool gradingActive = false;

    void Start()
    {
        // Initialize the score at the beginning of the run.
        currentScore = maxScore;
    }

    /// <summary>
    /// Activates collision scoring.
    /// Call this method (e.g., from your start marker trigger) to begin grading.
    /// </summary>
    public void StartGrading()
    {
        gradingActive = true;
        ResetScore();  // Reset score at the start if needed.
        Debug.Log("Grading started. Score reset to: " + currentScore);
    }

    /// <summary>
    /// Deactivates collision scoring.
    /// Call this method (e.g., from your end marker trigger) to end grading.
    /// </summary>
    public void EndGrading()
    {
        gradingActive = false;
        Debug.Log("Grading ended. Final Score: " + currentScore);
    }

    /// <summary>
    /// Called automatically by Unity when this GameObject collides with another.
    /// Only processes collisions when grading is active.
    /// </summary>
    /// <param name="collision">Collision data.</param>
    void OnCollisionEnter(Collision collision)
    {
        // Only process collisions if grading is active.
        if (!gradingActive)
            return;

        // Optionally: Ignore collisions from parts like wheels if needed.
        // Example:
        // if (collision.collider.CompareTag("Wheel"))
        //     return;

        // Get the collision severity based on the relative velocity.
        float collisionSeverity = collision.relativeVelocity.magnitude;

        // Calculate the penalty for this collision.
        float penalty = collisionBasePenalty + (collisionSeverity * collisionSeverityMultiplier);

        // Update our collision counters.
        totalCollisionPenalty += penalty;
        collisionCount++;

        // Subtract the penalty from the current score.
        currentScore = Mathf.Max(0, maxScore - totalCollisionPenalty);

        Debug.Log("Collision #" + collisionCount +
                  " | Severity: " + collisionSeverity +
                  " | Penalty: " + penalty +
                  " | Current Score: " + currentScore);
    }

    /// <summary>
    /// Returns the current score.
    /// </summary>
    public float GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// Resets the score and collision counters.
    /// </summary>
    public void ResetScore()
    {
        totalCollisionPenalty = 0f;
        collisionCount = 0;
        currentScore = maxScore;
    }
}
