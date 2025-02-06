using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class markerPlacerScript : MonoBehaviour
{
    [Header("References")]
    // Assign your cameraRig (or player rig) transform here
    public Transform cameraRig;
    
    // Assign your marker prefab (the red cylinder prefab) in the Inspector
    public GameObject markerPrefab;
    
    [Header("Placement Settings")]
    // Layer mask for your ground mesh; make sure your ground tiles are on this layer
    public LayerMask groundLayer;
    
    // How far above the ground to place the marker
    public float groundOffset = 0.1f;

    void Update()
    {
        // Check if left mouse button was pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Get the x and z from the cameraRig.
            Vector3 markerPosition = new Vector3(cameraRig.position.x, 0f, cameraRig.position.z);
            
            // To ensure the marker is placed just above the ground, cast a ray downward.
            // We start the ray high above the desired position.
            Ray ray = new Ray(new Vector3(cameraRig.position.x, 1000f, cameraRig.position.z), Vector3.down);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                // Set y to the hit point’s y plus our offset.
                markerPosition.y = hit.point.y + groundOffset;
            }
            else
            {
                // If no ground collider is hit, use a default y (e.g., groundOffset)
                markerPosition.y = groundOffset;
            }
            
            // Instantiate the marker prefab at the calculated position with no rotation.
            Instantiate(markerPrefab, markerPosition, Quaternion.identity);
        }
    }
}
