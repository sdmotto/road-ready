using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class timerScript : MonoBehaviour
{
    [Header("UI Reference")]
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

    public void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Format the time to two decimals.
            timerText.text = string.Format("{0:0.00} sec", elapsedTime);
        }
    }

    // Call this to start the timer.
    public void StartTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;
        Debug.Log("Route Timer started.");
    }


    // Call this to stop the timer.
    public void StopTimer()
    {
        timerRunning = false;
        Debug.Log("Route Timer stopped. Final time: " + elapsedTime.ToString("0.00") + " sec");
    }

    public void ResetTimer()
    {
        elapsedTime = 0;
        timerRunning = false;
        Debug.Log("Reset timer!!!");
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

    // getter for elapsed time
    public float GetElapsedTime()
    {
        return elapsedTime;
    }

}
