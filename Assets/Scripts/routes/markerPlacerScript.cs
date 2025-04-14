using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class markerPlacerScript : MonoBehaviour
{
    [Header("References")]
    public Transform cameraRig;
    public GameObject markerPrefab;
    public Transform markerHolder;

    [Header("Line Settings")]
    public Material lineMaterial;
    public float lineWidth = 0.2f;
    public int sampleStepsPerSegment = 10;

    [Header("Placement Settings")]
    public LayerMask groundLayer;
    public float groundOffset = 0.1f;

    private List<GameObject> markerList = new List<GameObject>();

    public GameObject routeNamerPanel;
    public TMP_InputField routeNameInput;

    private string routeName;
    public static bool locked = false;
    private GameObject lastRouteObject = null;

    void Update()
    {
        if (locked) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverInteractiveUI()) return;

            Vector3 markerPosition = GetMouseWorldPosition();
            GameObject newMarker = Instantiate(markerPrefab, markerPosition, Quaternion.identity, markerHolder);
            markerList.Add(newMarker);
        }
    }

    public async void CreateRouteMap()
    {
        if (markerHolder.childCount == 0)
        {
            Debug.LogWarning("No markers to create route.");
            return;
        }

        // Destroy the previous route (line + markers)
        if (lastRouteObject != null)
        {
            Destroy(lastRouteObject);
        }

        // Gather markers
        List<Transform> markers = new List<Transform>();
        for (int i = 0; i < markerHolder.childCount; i++)
        {
            markers.Add(markerHolder.GetChild(i));
        }

        // Generate smoothed ground points
        List<Vector3> groundPoints = new List<Vector3>();
        if (markers.Count > 0)
            groundPoints.Add(GetGroundPosition(markers[0].position));

        for (int i = 0; i < markers.Count - 1; i++)
        {
            for (int step = 1; step <= sampleStepsPerSegment; step++)
            {
                float t = (float)step / sampleStepsPerSegment;
                Vector3 interp = Vector3.Lerp(markers[i].position, markers[i + 1].position, t);
                groundPoints.Add(GetGroundPosition(interp));
            }
        }

        // Save Route data to persistentDataPath
        Route route = new Route
        {
            RouteName = routeName,
            linePoints = groundPoints
        };

        foreach (Transform marker in markers)
        {
            route.markerPositions.Add(marker.position);
        }

        await RouteManager.Instance.SaveRouteAsync(route);

        // Create a parent object to hold the route line and markers
        lastRouteObject = new GameObject("Route_" + route.RouteName);

        // Create the line object
        GameObject lineObj = new GameObject("LineRenderer_" + route.RouteName);
        lineObj.transform.SetParent(lastRouteObject.transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.widthMultiplier = lineWidth;
        lr.useWorldSpace = true;
        lr.positionCount = groundPoints.Count;

        for (int i = 0; i < groundPoints.Count; i++)
        {
            lr.SetPosition(i, groundPoints[i]);
        }

        // Reparent markers to the route object
        foreach (Transform marker in markers)
        {
            marker.SetParent(lastRouteObject.transform);
        }

        // Tag start and end markers
        if (markers.Count > 0)
        {
            markers[0].gameObject.tag = "StartMarker";
            markers[markers.Count - 1].gameObject.tag = "EndMarker";
        }

        // Clear the marker list for new placement
        markerList.Clear();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 markerPosition = new Vector3(0, groundOffset, 0);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            markerPosition = hit.point + new Vector3(0, groundOffset, 0);
        }
        return markerPosition;
    }

    private Vector3 GetGroundPosition(Vector3 originalPos)
    {
        Vector3 pos = originalPos;
        Ray ray = new Ray(new Vector3(originalPos.x, 1000f, originalPos.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            pos.y = hit.point.y + groundOffset;
        }
        return pos;
    }

    private bool IsPointerOverInteractiveUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
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

    public void Name()
    {
        routeName = routeNameInput.text;
        Debug.Log("User entered route name: " + routeName);
        routeNamerPanel.SetActive(false);

        CreateRouteMap();
        locked = false;
    }
}
