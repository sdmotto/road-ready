using UnityEngine;
using TMPro;

public class DetailedResultsDisplay : MonoBehaviour
{
    [Header("Grouped UI Blocks")]
    public TMP_Text stopInfoText;
    public TMP_Text penaltyBreakdownText;

    void Start() {
        // Stop counts + collisions
        stopInfoText.text =
            $"Total Stops: {Data.Instance.stopSignStops + Data.Instance.stopLightStops}\n" +
            $"At Stop Signs: {Data.Instance.stopSignStops}\n" +
            $"At Stop Lights: {Data.Instance.stopLightStops}\n" +
            $"Total Collisions: {Data.Instance.totalCollisions}";

        // Penalty summary
        penaltyBreakdownText.text =
            $"For Collisions : {Data.Instance.collisionPenalty:F1}\n" +
            $"Running Stop Signs: {Data.Instance.stopSignPenalty:F1}\n" +
            $"Running Stop Lights: {Data.Instance.stopLightPenalty:F1}\n" +
            $"Speeding: {Data.Instance.speedingPenalty:F1}";
    }
}
