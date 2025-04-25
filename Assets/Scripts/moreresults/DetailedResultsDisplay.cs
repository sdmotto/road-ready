using UnityEngine;
using TMPro;

public class DetailedResultsDisplay : MonoBehaviour
{
    [Header("Grouped UI Blocks")]
    public TMP_Text generalInfoText;
    public TMP_Text penaltyBreakdownText;

    void Start()
    {
        // General info
        generalInfoText.text =
            $"Total Stops: {Data.Instance.totalStopCount}\n" +
            $"Stop Sign Stops: {Data.Instance.stopSignStops}\n" +
            $"Traffic Lights Obeyed: {Data.Instance.lightSuccessCount}\n" +
            $"Total Collisions: {Data.Instance.totalCollisions}\n" +
            $"Left Turns: {Data.Instance.numLeftTurns}\n" +
            $"Right Turns: {Data.Instance.numRightTurns}";

        // Penalty summary
        penaltyBreakdownText.text =
            $"POINT DEDUCTIONS\n" +
            $"For Collisions : {Data.Instance.collisionPenalty:F1}\n" +
            $"Running Yellow: {Data.Instance.yellowPenalty:F1}\n" +
            $"Running Red: {Data.Instance.redPenalty:F1}\n" +
            $"Running Stop Signs: {Data.Instance.stopSignPenalty:F1}\n" +
            $"Speeding: {Data.Instance.speedingPenalty:F1}\n" +
            $"No Turn Signal: {Data.Instance.turnSigPenalty:F1}";
    }
}
