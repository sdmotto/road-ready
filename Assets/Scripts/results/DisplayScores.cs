using UnityEngine;
using TMPro;

public class displayScore : MonoBehaviour
{
    [SerializeField] private TMP_Text hudText;

    void Update()
    {
        if (hudText == null) return;

        float score     = Data.Instance.score;
        float avgSpeed  = Data.Instance.averageSpeed;
        float maxSpeed  = Data.Instance.maxSpeed;
        float totalTime = Data.Instance.elapsedTime;

        hudText.text = $"Avg Speed: {avgSpeed:F1} MPH\n" +
                       $"Max Speed: {maxSpeed:F1} MPH\n" +
                       $"Time: {totalTime:F2} sec\n" +
                       $"Score: {score:F1}";
    }
}
