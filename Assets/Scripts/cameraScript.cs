using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Base Movement Settings (at scaling factor = 1)")]
    public float baseMaxMoveSpeed = 10f;      // Base horizontal (x/z) speed
    public float baseMoveAcceleration = 5f;   // Base horizontal acceleration
    public float baseMaxZoomSpeed = 10f;      // Base vertical (y) speed
    public float baseZoomAcceleration = 5f;   // Base vertical acceleration

    [Header("Zoom Scaling Settings")]
    [Tooltip("Minimum allowed scaling factor (when very close to the ground)")]
    public float minScalingFactor = 1f;
    [Tooltip("Maximum allowed scaling factor (when far from the ground)")]
    public float maxScalingFactor = 100f;

    // Current velocities
    private Vector3 horizontalVelocity = Vector3.zero;
    private float verticalVelocity = 0f;

    void Update()
    {
        // --- Calculate scaling factor based on camera's distance from the ground ---
        float scalingFactor = 1f;
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // Use the hit distance as the scaling factor, clamped between our min and max
            scalingFactor = Mathf.Clamp(hit.distance, minScalingFactor, maxScalingFactor);
        }
        // Optionally, you can adjust the scaling factor non-linearly here if desired.

        // --- Read Input ---
        float inputX = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys
        float inputZ = Input.GetAxis("Vertical");   // W/S or Up/Down arrow keys
        float inputY = 0f;
        if (Input.GetKey(KeyCode.LeftShift)) inputY -= 1f; // Shift moves downward
        if (Input.GetKey(KeyCode.Space))     inputY += 1f; // Space moves upward

        // --- Horizontal Movement (x,z) ---
        Vector3 targetHorizontalDir = new Vector3(inputX, 0f, inputZ);
        if (targetHorizontalDir.magnitude > 0.01f)
        {
            targetHorizontalDir.Normalize();
            // Multiply by the base max speed and scale by our distance factor
            Vector3 targetHorizontalVelocity = targetHorizontalDir * baseMaxMoveSpeed * scalingFactor;
            // Smoothly accelerate toward the target velocity
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetHorizontalVelocity,
                baseMoveAcceleration * Time.deltaTime
            );
        }
        else
        {
            // If no input, stop horizontal movement
            horizontalVelocity = Vector3.zero;
        }

        // --- Vertical Movement (y) ---
        if (Mathf.Abs(inputY) > 0.01f)
        {
            float targetVerticalVelocity = inputY * baseMaxZoomSpeed * scalingFactor;
            verticalVelocity = Mathf.MoveTowards(
                verticalVelocity,
                targetVerticalVelocity,
                baseZoomAcceleration * Time.deltaTime
            );
        }
        else
        {
            // If no vertical input, stop vertical movement
            verticalVelocity = 0f;
        }

        // --- Combine and Apply Movement ---
        Vector3 finalVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
        transform.Translate(finalVelocity * Time.deltaTime, Space.World);
    }
}
