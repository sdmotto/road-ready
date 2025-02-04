using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dynamicCamAnchor : MonoBehaviour
{
    [Header("Tracking Settings")]
    [Tooltip("The transform whose X and Z position will be followed (e.g. the CameraRig or ViewingCamera).")]
    public Transform trackingTarget;
    
    [Tooltip("Offset above the detected surface (to keep the anchor just above the ground).")]
    public float surfaceOffset = 0.1f;
    
    [Tooltip("The layer(s) that represent the ground/mesh surface.")]
    public LayerMask groundLayer;

    void LateUpdate()
    {
        if (trackingTarget == null)
        {
            Debug.LogWarning("DynamicCameraAnchor: Tracking target not assigned!");
            return;
        }

        // Get the horizontal (X/Z) position from the tracking target.
        float x = trackingTarget.position.x;
        float z = trackingTarget.position.z;

        // Raycast from high above at (x, z) straight downward.
        float raycastStartHeight = 10000f;
        Vector3 rayOrigin = new Vector3(x, raycastStartHeight, z);
        Ray ray = new Ray(rayOrigin, Vector3.down);
        RaycastHit hit;
        float groundY = 0f;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            groundY = hit.point.y;
        }
        else
        {
            // If no ground was hit, you can choose a default height.
            groundY = 0f;
        }

        // Set the dynamic camera's Y to the ground's Y plus a small offset.
        float newY = groundY + surfaceOffset;

        // Update the position so that X and Z follow the tracking target, and Y is set to the surface.
        transform.position = new Vector3(x, newY, z);
    }
}
