using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float moveSpeed = 1f;   // For WASD movement on X/Z.
    public float zoomSpeed = 1f;   // For up/down movement on Y.

    void Update()
    {
        // --- Horizontal/Vertical movement (WASD) ---
        float moveX = Input.GetAxis("Horizontal");  // A/D or Left/Right keys
        float moveZ = Input.GetAxis("Vertical");    // W/S or Up/Down keys

        // Apply horizontal/vertical movement to the X/Z plane:
        Vector3 horizontalMovement = new Vector3(moveX, 0f, moveZ);

        // --- Zoom in/out (Shift = down, Space = up) ---
        float verticalMovement = 0f;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Move down (zoom in from the top-down perspective)
            verticalMovement -= 1f;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            // Move up (zoom out from the top-down perspective)
            verticalMovement += 1f;
        }

        // Combine movement vectors:
        Vector3 movement = horizontalMovement * moveSpeed + Vector3.up * (verticalMovement * zoomSpeed);

        // Apply movement based on world space, independent of camera angle:
        transform.Translate(movement * Time.deltaTime, Space.World);
    }
}
