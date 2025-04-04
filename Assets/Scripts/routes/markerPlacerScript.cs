using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // For directory/file operations
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class markerPlacerScript : MonoBehaviour
{
    [Header("References")]
    // (Optional) You can still assign a camera rig if needed for other purposes, 
    // but for placement we'll use the main camera's mouse ray.
    public Transform cameraRig;
    
    // Assign your marker prefab (e.g., the red cylinder prefab) in the Inspector
    public GameObject markerPrefab;

    // Assign the markerHolder GameObject (all markers will be parented here)
    public Transform markerHolder;

    [Header("Line Settings")]
    // A material for the line; assign one in the Inspector (e.g., an Unlit/Color material)
    public Material lineMaterial;
    // The width for the connecting line
    public float lineWidth = 0.2f;
    // How many sample steps to take per segment between markers.
    // Increasing this number will yield a smoother line that follows the ground.
    public int sampleStepsPerSegment = 10;

    [Header("Placement Settings")]
    // Layer mask for your ground mesh; make sure your ground tiles are on this layer
    public LayerMask groundLayer;
    
    // How far above the ground to place the marker
    public float groundOffset = 0.1f;

    // (Optional) List to track markers in the order placed.
    private List<GameObject> markerList = new List<GameObject>();

    public GameObject routeNamerPanel;
    public TMP_InputField routeNameInput;

    private string name;

    public static bool locked = false;

    void Update()
    {
        if (locked) return;

        // On left mouse click, place a marker.
        if (Input.GetMouseButtonDown(0))
        {
            // If clicking over an interactive UI element, do nothing.
            if (IsPointerOverInteractiveUI()) return;

            Vector3 markerPosition = Vector3.zero;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Raycast from the mouse pointer into the world using the ground layer.
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                markerPosition = hit.point + new Vector3(0, groundOffset, 0);
            }
            else
            {
                // Fallback: if nothing is hit, use a default position at y=groundOffset.
                markerPosition = new Vector3(0, groundOffset, 0);
            }

            // Instantiate the marker as a child of markerHolder.
            GameObject newMarker = Instantiate(markerPrefab, markerPosition, Quaternion.identity, markerHolder);
            markerList.Add(newMarker);
        }
    }

    public void CreateRouteMap()
    {
        // Make sure there are markers in the markerHolder.
        if (markerHolder.childCount == 0)
        {
            Debug.LogWarning("No markers in markerHolder to create a route map.");
            return;
        }

        // Determine a unique route map name.
        int routeNumber = 1;
#if UNITY_EDITOR
        // Use the Resources/Routes folder path.
        string folderPath = "Assets/Resources/Routes";
        // Ensure that Assets/Resources exists.
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        // Ensure that Assets/Resources/Routes exists.
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Routes");
        }
        // Count the existing routeMap prefabs in the folder.
        string[] files = Directory.GetFiles(folderPath, "routeMap*.prefab", System.IO.SearchOption.TopDirectoryOnly);
        routeNumber = files.Length + 1;
#else
        routeNumber = 1;
#endif
        // Format the route map name as routeMapXXXXX (5 digits).
        string routeMapName = name;

        // Create the routeMap GameObject.
        GameObject routeMap = new GameObject(routeMapName);

        // Create a child object to hold the LineRenderer.
        GameObject lineObj = new GameObject("LineConnector");
        lineObj.transform.parent = routeMap.transform;

        // Add and configure the LineRenderer.
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;

        // Gather markers from markerHolder (in the order they were placed).
        int markerCount = markerHolder.childCount;
        List<Transform> markers = new List<Transform>();
        for (int i = 0; i < markerCount; i++)
        {
            markers.Add(markerHolder.GetChild(i));
        }

        // Generate a list of points that follow the ground.
        List<Vector3> groundPoints = new List<Vector3>();
        if (markers.Count > 0)
        {
            // Add the ground-projected position of the first marker.
            groundPoints.Add(GetGroundPosition(markers[0].position));
        }
        // For each segment between consecutive markers:
        for (int i = 0; i < markers.Count - 1; i++)
        {
            for (int step = 1; step <= sampleStepsPerSegment; step++)
            {
                float t = (float)step / sampleStepsPerSegment;
                // Interpolate between marker positions.
                Vector3 interpolatedPos = Vector3.Lerp(markers[i].position, markers[i + 1].position, t);
                // Project the interpolated position onto the ground.
                Vector3 groundPos = GetGroundPosition(interpolatedPos);
                groundPoints.Add(groundPos);
            }
        }

        // Set the positions in the LineRenderer.
        lr.positionCount = groundPoints.Count;
        for (int i = 0; i < groundPoints.Count; i++)
        {
            lr.SetPosition(i, groundPoints[i]);
        }

        // Reparent each marker from markerHolder to routeMap.
        foreach (Transform marker in markers)
        {
            marker.SetParent(routeMap.transform);
        }

        // Set the tags of the first and last markers for later detection.
        if (markers.Count > 0)
        {
            markers[0].gameObject.tag = "StartMarker";
            markers[markers.Count - 1].gameObject.tag = "EndMarker";
        }

        // Delete all intermediate markers (keep only the first and last).
        if (markers.Count > 2)
        {
            // Iterate backwards from second-to-last down to index 1.
            for (int i = markers.Count - 2; i > 0; i--)
            {
                Destroy(markers[i].gameObject);
            }
        }

        // Optionally clear the marker list if you no longer need it.
        markerList.Clear();

#if UNITY_EDITOR
        // Save the routeMap as a prefab in the Resources/Routes folder.
        string prefabPath = folderPath + "/" + routeMapName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(routeMap, prefabPath);
        Debug.Log("RouteMap prefab saved to: " + prefabPath);
#endif
    }

    // Helper method to "project" a position onto the ground.
    private Vector3 GetGroundPosition(Vector3 originalPos)
    {
        Vector3 pos = originalPos;
        Ray ray = new Ray(new Vector3(originalPos.x, 1000f, originalPos.z), Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            pos.y = hit.point.y + groundOffset;
        }
        return pos;
    }

    // Updated method: Using GetComponentInParent to check for interactive UI.
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
            // Use GetComponentInParent to catch buttons where the actual hit object is a child.
            Button btn = result.gameObject.GetComponentInParent<Button>();
            if (btn != null && btn.interactable)
                return true;
        }

        return false;
    }

    public void ShowRouteNamer()
    {
        routeNamerPanel.SetActive(true);
        routeNameInput.text = "";
        routeNameInput.ActivateInputField();
        locked = true;
    }

    public void Name() {
        Debug.Log("User entered route name: " + routeNameInput.text);
        routeNamerPanel.SetActive(false);

        name = routeNameInput.text;
        CreateRouteMap();
        locked = false;
    }
}
