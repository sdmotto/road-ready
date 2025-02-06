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
        // Check for U key press
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (cameraRig != null)
            {
                // Create a ray from the camera rig's position in its forward direction
                Ray deletionRay = new Ray(cameraRig.position, cameraRig.forward);
                RaycastHit hit;

                // Draw the ray in the Scene view for 1 second (visible during Play mode)
                Debug.DrawRay(cameraRig.position, cameraRig.forward * deletionRayDistance, Color.red, 1f);
                
                // Perform the raycast
                if (Physics.Raycast(deletionRay, out hit, deletionRayDistance))
                {
                    Debug.Log("Ray hit: " + hit.collider.name);
                    // Check if the hit object is tagged as "Marker"
                    if (hit.collider.CompareTag("Marker"))
                    {
                        Debug.Log("Deleting marker: " + hit.collider.gameObject.name);
                        Destroy(hit.collider.gameObject);
                    }
                    else
                    {
                        Debug.Log("Hit object is not tagged as Marker.");
                    }
                }
                else
                {
                    Debug.Log("Ray did not hit any object.");
                }
            }
            else
            {
                Debug.LogWarning("Camera Rig is not assigned in MarkerDeleterScript!");
            }
        }
    }
}
