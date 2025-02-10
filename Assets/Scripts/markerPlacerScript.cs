using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // For directory/file operations

#if UNITY_EDITOR
using UnityEditor;
#endif

public class markerPlacerScript : MonoBehaviour
{
    [Header("References")]
    // Assign your cameraRig (or player rig) transform here
    public Transform cameraRig;
    
    // Assign your marker prefab (the red cylinder prefab) in the Inspector
    public GameObject markerPrefab;

    // Assign the markerHolder GameObject (all markers will be parented here)
    public Transform markerHolder;

    [Header("Line Settings")]
    // A material for the line; assign one in the Inspector (e.g., an Unlit/Color material)
    public Material lineMaterial;
    // The width for the connecting line
    public float lineWidth = 0.2f;

    [Header("Placement Settings")]
    // Layer mask for your ground mesh; make sure your ground tiles are on this layer
    public LayerMask groundLayer;
    
    // How far above the ground to place the marker
    public float groundOffset = 0.1f;

    // (Optional) List to track markers in the order placed
    // This is kept in case you need the order later;
    // however, we will use markerHolder's children when transferring markers.
    private List<GameObject> markerList = new List<GameObject>();

    void Update()
    {
        // On left mouse click, place a marker.
        if (Input.GetMouseButtonDown(0))
        {
            // Calculate marker position using cameraRig's x and z, and adjust y by raycasting
            Vector3 markerPosition = new Vector3(cameraRig.position.x, 0f, cameraRig.position.z);
            Ray ray = new Ray(new Vector3(cameraRig.position.x, 1000f, cameraRig.position.z), Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                markerPosition.y = hit.point.y + groundOffset;
            }
            else
            {
                markerPosition.y = groundOffset;
            }

            // Instantiate the marker as a child of markerHolder.
            GameObject newMarker = Instantiate(markerPrefab, markerPosition, Quaternion.identity, markerHolder);
            markerList.Add(newMarker);
        }

        // When the user presses C, create the route map.
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateRouteMap();
        }
    }

    void CreateRouteMap()
    {
        // Make sure there are markers in the markerHolder
        if (markerHolder.childCount == 0)
        {
            Debug.LogWarning("No markers in markerHolder to create a route map.");
            return;
        }

        // Determine a unique route map name based on the number of existing route map prefabs.
        int routeNumber = 1;
#if UNITY_EDITOR
        string folderPath = "Assets/Routes";
        // Create the Routes folder if it doesn't exist.
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Routes");
        }
        // Count the existing routeMap prefabs in the folder.
        string[] files = Directory.GetFiles(folderPath, "routeMap*.prefab", SearchOption.TopDirectoryOnly);
        routeNumber = files.Length + 1;
#else
        // In a non-editor build, you can use the marker list count or another method.
        routeNumber = 1;
#endif
        // Format the route map name as routeMapXXXXX (5 digits)
        string routeMapName = "routeMap" + routeNumber.ToString("D5");

        // Create the routeMap GameObject.
        GameObject routeMap = new GameObject(routeMapName);

        // Create a child object to hold the LineRenderer for connecting markers.
        GameObject lineObj = new GameObject("LineConnector");
        lineObj.transform.parent = routeMap.transform;

        // Add and configure the LineRenderer.
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        // We'll get the markers from markerHolder in the order they were placed.
        int markerCount = markerHolder.childCount;
        lr.positionCount = markerCount;
        List<Transform> markers = new List<Transform>();
        // Copy the children from markerHolder into a temporary list to preserve the order.
        for (int i = 0; i < markerCount; i++)
        {
            markers.Add(markerHolder.GetChild(i));
        }
        // Set positions in the LineRenderer and reparent each marker to routeMap.
        for (int i = 0; i < markers.Count; i++)
        {
            lr.SetPosition(i, markers[i].position);
            markers[i].SetParent(routeMap.transform);
        }

        // Optionally clear the marker list if you no longer need it.
        markerList.Clear();

#if UNITY_EDITOR
        // Save the routeMap as a prefab in the Routes folder.
        string prefabPath = folderPath + "/" + routeMapName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(routeMap, prefabPath);
        Debug.Log("RouteMap prefab saved to: " + prefabPath);
#endif
    }
}
