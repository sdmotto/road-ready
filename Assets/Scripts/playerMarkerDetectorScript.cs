using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMarkerDetectorScript : MonoBehaviour
{
    // Flags to ensure we only print the message once per marker.
    private bool startMarkerHit = false;
    private bool endMarkerHit = false;

    // Ensure your player GameObject has a Rigidbody and Collider
    // (with the collider NOT set as trigger) for proper collision detection.
    // The marker objects should have a Collider with "Is Trigger" enabled.

    private void OnTriggerEnter(Collider other)
    {
        // Check if we hit the start marker.
        if (other.CompareTag("StartMarker") && !startMarkerHit)
        {
            Debug.Log("Player has driven over the START marker!");
            startMarkerHit = true;
        }
        // Check if we hit the end marker.
        else if (other.CompareTag("EndMarker") && !endMarkerHit)
        {
            Debug.Log("Player has driven over the END marker!");
            endMarkerHit = true;
        }
    }
}
