using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMarkerDetectorScript : MonoBehaviour
{
    // Flags to ensure we only trigger the markers once.
    private bool startMarkerHit = false;
    private bool endMarkerHit = false;

    // Reference to the scoreScript attached to the same GameObject (xcar).
    private scoreScript scoring;

    private void Awake()
    {
        scoring = GetComponent<scoreScript>();
        if (scoring == null)
        {
            Debug.LogError("scoreScript not found on the xcar!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StartMarker") && !startMarkerHit)
        {
            Debug.Log("Player has driven over the START marker!");
            startMarkerHit = true;
            scoring?.StartGrading();

            // ✅ Enable the proximity checker when the route starts
            RouteProximityChecker checker = GetComponent<RouteProximityChecker>();
            if (checker != null)
            {
                checker.enabled = true;
            }
        }
        else if (other.CompareTag("EndMarker") && !endMarkerHit)
        {
            Debug.Log("Player has driven over the END marker!");
            endMarkerHit = true;
            scoring?.EndGrading();

            // ✅ Optionally disable it after the route is complete
            RouteProximityChecker checker = GetComponent<RouteProximityChecker>();
            if (checker != null)
            {
                checker.enabled = false;
            }
        }
    }
}
