using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCamAnchor : MonoBehaviour
{
    [Header("Tracking Settings")]
    public Transform trackingTarget;

    void LateUpdate()
    {
        if (trackingTarget == null)
        {
            Debug.LogWarning("DynamicCamAnchor: Tracking target not assigned!");
            return;
        }

        // Set this object's position to match the CameraRig's position exactly
        transform.position = trackingTarget.position;
    }
}
