using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Import TextMeshPro namespace
using TMPro;

public class timerScript : MonoBehaviour
{
    [Header("UI Reference")]
    // Use a TMP_Text field instead of UnityEngine.UI.Text
    public TMP_Text timerText;

    private float elapsedTime = 0f;
    private bool timerRunning = false;

    void Update()
    {
        // If the timer is running, accumulate time and update the display.
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Format the time to two decimals.
            timerText.text = string.Format("{0:0.00} sec", elapsedTime);
        }
    }

    /// <summary>
    /// Call this to start the timer.
    /// </summary>
    public void StartTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;
        Debug.Log("Route Timer started.");
    }

    /// <summary>
    /// Call this to stop the timer.
    /// </summary>
    public void StopTimer()
    {
        timerRunning = false;
        Debug.Log("Route Timer stopped. Final time: " + elapsedTime.ToString("0.00") + " sec");
    }

    // Detect trigger collisions with markers.
    private void OnTriggerEnter(Collider other)
    {
        // When the player touches the start marker, start the timer.
        if (other.CompareTag("StartMarker"))
        {
            if (!timerRunning)
            {
                StartTimer();
            }
        }
        // When the player touches the end marker, stop the timer.
        else if (other.CompareTag("EndMarker"))
        {
            if (timerRunning)
            {
                StopTimer();
            }
        }
    }
}
