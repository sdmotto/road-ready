using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class MarkerDeleterScript : MonoBehaviour
{
    [Header("Deletion Settings")]
    [Tooltip("Maximum distance for the deletion raycast.")]
    public float deletionRayDistance = 1000f;

    void Update()
    {
        // Check for right mouse button click (button index 1)
        if (Input.GetMouseButtonDown(1))
        {
            if (IsPointerOverInteractiveUI()) return;

            // Create a ray from the main camera using the current mouse position.
            Ray deletionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Draw the ray in the Scene view for 1 second
            Debug.DrawRay(deletionRay.origin, deletionRay.direction * deletionRayDistance, Color.red, 1f);

            // Perform the raycast
            if (Physics.Raycast(deletionRay, out hit, deletionRayDistance))
            {
                Debug.Log("Ray hit: " + hit.collider.name);
                // Check if the hit object is tagged as "Marker"
                if (hit.collider.CompareTag("Marker"))
                {
                    // Debug.Log("Deleting marker: " + hit.collider.gameObject.name);
                    Destroy(hit.collider.gameObject);
                }
                else
                {
                    // Debug.Log("Hit object is not tagged as Marker.");
                }
            }
            else
            {
                Debug.Log("Ray did not hit any object.");
            }
        }
    }

    private bool IsPointerOverInteractiveUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // Check if any result is part of an interactive UI element.
        foreach (RaycastResult result in results)
        {
            // Using GetComponentInParent ensures that even if the hit UI element is a child,
            // it will still detect the button parent.
            Button btn = result.gameObject.GetComponentInParent<Button>();
            if (btn != null && btn.interactable)
                return true;
        }

        return false;
    }
}
