using UnityEngine;
using TMPro;

public class displayScore : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    void Update()
    {
        if (scoreText == null) return;

        float score = Data.Instance.score;
        float avgSpeed = Data.Instance.averageSpeed;
        float maxSpeed = Data.Instance.maxSpeed;
        float totalTime = Data.Instance.elapsedTime;

        scoreText.text = $"Avg Speed: {avgSpeed:F1} MPH\n" +
                       $"Max Speed: {maxSpeed:F1} MPH\n" +
                       $"Time: {totalTime:F2} sec\n" +
                       $"Score: {score:F1}";
    }
}
