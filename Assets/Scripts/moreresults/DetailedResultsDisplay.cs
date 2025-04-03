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
            $"Total Stops: not done!!!\n" +
            $"Stop Sign Stops: {Data.Instance.stopSignStops}\n" +
            $"Traffic Lights Obeyed: {Data.Instance.lightSuccessCount}\n" +
            $"Total Collisions: {Data.Instance.totalCollisions}";

        // Penalty summary
        penaltyBreakdownText.text =
            $"For Collisions : {Data.Instance.collisionPenalty:F1}\n" +
            $"Running Yellow: {Data.Instance.yellowPenalty:F1}\n" +
            $"Running Red: {Data.Instance.redPenalty:F1}\n" +
            $"Running Stop Signs: {Data.Instance.stopSignPenalty:F1}\n" +
            $"Speeding: {Data.Instance.speedingPenalty:F1}";
    }
}
