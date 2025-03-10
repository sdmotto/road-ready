using UnityEngine;
using TMPro;

public class displayScore : MonoBehaviour
{
    [SerializeField] private TMP_Text hudText;            // Assign via the Inspector
    // [SerializeField] private speedScript speedScriptRef;    // Reference to your speedScript instance
    [SerializeField] private scoreScript scoreScriptRef;    // Reference to your scoreScript instance

    void Update()
    {
        // Retrieve values from your other scripts
        // float avgSpeed = speedScriptRef; // Ensure these properties are public in speedScript
        // float maxSpeed = speedScriptRef.MaxSpeed;
        // float time = scoreScriptRef.CurrentTime;      // Ensure these properties are public in scoreScript
        float score = Data.Instance.score;

        // Format the string. Using string interpolation here for clarity.
        hudText.text = $"Score: {score}";
    }
}