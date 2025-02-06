using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerDeleterScript : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag in the camera rig (Transform) used for marker deletion raycasting.")]
    public Transform cameraRig;

    [Header("Deletion Settings")]
    [Tooltip("Maximum distance for the deletion raycast.")]
    public float deletionRayDistance = 1000f;

    void Update()
    {
        // Check for Backspace key press
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (cameraRig != null)
            {
                // Create a ray from the camera rig's position in its forward direction
                Ray deletionRay = new Ray(cameraRig.position, cameraRig.forward);
                RaycastHit hit;

                // Perform the raycast
                if (Physics.Raycast(deletionRay, out hit, deletionRayDistance))
                {
                    // Check if the hit object is tagged as "Marker"
                    if (hit.collider.CompareTag("Marker"))
                    {
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Camera Rig is not assigned in MarkerDeleterScript!");
            }
        }
    }
}
