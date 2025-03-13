using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Base Movement Settings (at scaling factor = 1)")]
    public float baseMoveSpeed = 1f;  // Speed for horizontal (x/z) movement
    public float baseZoomSpeed = 1f;  // Speed for vertical (y) movement

    [Header("Zoom Scaling Settings")]
    [Tooltip("Minimum allowed scaling factor (when very close to the ground)")]
    public float minScalingFactor = 1f;
    [Tooltip("Maximum allowed scaling factor (when far from the ground)")]
    public float maxScalingFactor = 100f;

    void Update()
    {
        // --- Calculate scaling factor based on camera's distance from the ground ---
        float scalingFactor = 1f;
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // Use hit distance as scaling factor, clamped between min and max
            scalingFactor = Mathf.Clamp(hit.distance, minScalingFactor, maxScalingFactor);
        }

        // Read input from keyboard only so peripherals do not move the camera
        float inputX = 0f; // Left/Right movement
        float inputZ = 0f; // Forward/Backward movement
        float inputY = 0f; // Up/Down movement

        // Horizontal movement (left/right)
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputX -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputX += 1f;

        // Forward/Backward movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputZ += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputZ -= 1f;

        // Vertical movement (up/down)
        if (Input.GetKey(KeyCode.Space)) inputY += 1f;      // Space moves up
        if (Input.GetKey(KeyCode.LeftShift)) inputY -= 1f;  // Left Shift moves down

        // --- Apply Instant Movement (Snappy) ---
        Vector3 moveDirection = new Vector3(inputX, inputY, inputZ).normalized; // Prevent diagonal speed boost
        Vector3 moveAmount = moveDirection * baseMoveSpeed * scalingFactor * Time.deltaTime;

        transform.Translate(moveAmount, Space.World);

        // Debugging
        Debug.Log($"Move: {moveDirection}, Scaling Factor: {scalingFactor}");
    }
}