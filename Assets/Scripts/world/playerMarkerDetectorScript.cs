using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class playerMarkerDetectorScript : MonoBehaviour
{
    // Flags to ensure we only trigger the markers once.
    public bool startMarkerHit = false;
    public bool endMarkerHit = false;

    // Reference to the scoreScript attached to the same GameObject (xcar).
    private scoreScript scoring;

    public scoreScript scoreScript;

    private void Awake()
    {
        scoring = GetComponent<scoreScript>();
        if (scoring == null)
        {
            Debug.LogError("scoreScript not found on the xcar!");
        }
    }

    // eCheck if user has entered first route marker
    private async Task OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StartMarker") && !startMarkerHit)
        {
            //Debug.Log("Player has driven over the START marker!");
            startMarkerHit = true;
            scoring.resetEverything();
            scoring?.StartGrading();

            // Enable the proximity checker when the route starts
            RouteProximityChecker checker = GetComponent<RouteProximityChecker>();
            if (checker != null)
            {
                checker.enabled = true;
            }
        }
        // check if user has driven over the end marker
        else if (other.CompareTag("EndMarker") && !endMarkerHit)
        {
            //Debug.Log("Player has driven over the END marker!");
            endMarkerHit = true;
            scoring?.EndGrading();

            await RouteStatsManager.Instance.SaveRouteStatsAsync(Data.Instance.ToRouteStatsModel());

            // disable it after the route is complete
            RouteProximityChecker checker = GetComponent<RouteProximityChecker>();
            if (checker != null)
            {
                checker.enabled = false;
            }
        }
    }
}
