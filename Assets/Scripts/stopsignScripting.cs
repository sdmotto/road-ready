using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using CesiumForUnity;
using Unity.Mathematics;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class StopSignGenerator : MonoBehaviour
{
    public CesiumGeoreference cesiumGeoreference;
    // Reference to the UI Image for the stop sign indicator.
    // This should be set in the Inspector and be invisible (alpha=0) by default.
    public Image stopSignImage; 
    
    // The radius of the detection zone (in Unity units).
    public float zoneRadius = 10f;

    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=";
    // Keep track of stop sign nodes already processed.
    private HashSet<long> processedStopSignIds = new HashSet<long>();
    
    // Base coordinates for the area.
    private float baseLat = 41.65962f;
    private float baseLon = -91.53464f;
    // Radius (in meters) for the Overpass query.
    private int queryRadius = 1000;

    // Convert latitude, longitude, and a given height to a Unity world position.
    private double3 LatLongToUnityPosition(float lat, float lon, float height)
    {
        double3 earthPosition = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(lon, lat, height));
        return cesiumGeoreference.TransformEarthCenteredEarthFixedPositionToUnity(earthPosition);
    }

    void Start()
    {
        SetAlpha(stopSignImage, 0f);
        StartCoroutine(SweepArea());
    }

    // Make a single Overpass query around the base coordinate.
    private IEnumerator SweepArea()
    {
        float lat = baseLat;
        float lon = baseLon;
        string query = $@"
            [out:json];
            node(around:{queryRadius},{lat},{lon})[""highway""=""stop""];
            out geom;
        ";

        Debug.Log($"Fetching stop sign data for lat: {lat}, lon: {lon}");
        yield return StartCoroutine(FetchStopSignData(overpassUrl + query));
    }

    // Fetch stop sign data using UnityWebRequest.
    private IEnumerator FetchStopSignData(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                ParseAndCreateZones(jsonResponse);
            }
            else
            {
                Debug.LogError("Failed to fetch stop sign data: " + request.error);
            }
        }
    }

    // Parse the JSON response and create a detection zone for each stop sign.
    private void ParseAndCreateZones(string jsonResponse)
    {
        JObject json = JObject.Parse(jsonResponse);
        foreach (var element in json["elements"])
        {
            if ((string)element["type"] != "node")
                continue;

            long nodeId = (long)element["id"];
            if (processedStopSignIds.Contains(nodeId))
                continue;

            processedStopSignIds.Add(nodeId);

            float lat = (float)element["lat"];
            float lon = (float)element["lon"];

            // Convert lat/lon to a base world position using a fixed height (e.g., 0).
            double3 basePos = LatLongToUnityPosition(lat, lon, 0f);
            Vector3 worldBasePos = ToVector3(basePos);

            // Create an invisible cylindrical detection zone at this world position.
            CreateStopSignZone(worldBasePos);
        }
    }

    // Create an invisible cylindrical detection zone at the given position.
    private void CreateStopSignZone(Vector3 position)
    {
        GameObject zone = new GameObject("StopSignZone");
        zone.transform.position = position;
        // Use a CapsuleCollider to simulate a cylinder that extends vertically.
        CapsuleCollider collider = zone.AddComponent<CapsuleCollider>();
        collider.radius = zoneRadius;
        collider.height = 10000f; // Extremely tall so it effectively extends infinitely upward and downward.
        collider.direction = 1; // Y-axis.
        collider.isTrigger = true;

        // Add the detection script and assign the UI Image reference.
        StopSignZoneTrigger trigger = zone.AddComponent<StopSignZoneTrigger>();
        trigger.stopSignImage = this.stopSignImage;
    }

    // Helper to convert from double3 to Unity's Vector3.
    private static Vector3 ToVector3(double3 d)
    {
        return new Vector3((float)d.x, (float)d.y, (float)d.z);
    }

        private void SetAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}

public class StopSignZoneTrigger : MonoBehaviour
{
    // Reference to the UI Image for the stop sign indicator.
    public Image stopSignImage;
    // Flag to track whether the stop sign indicator is active.
    private bool stopSignActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // When the player enters the zone, make the stop sign image visible.
        SetAlpha(stopSignImage, 1f);
        stopSignActive = true;
        Debug.Log("Player entered stop sign zone: " + gameObject.name + " - Stop sign image activated.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // While the player is in the zone, check if they have come to a complete stop.
        if (stopSignActive)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && rb.velocity.magnitude < 0.1f)
            {
                SetAlpha(stopSignImage, 0f);
                stopSignActive = false;
                Debug.Log("Player stopped in zone: " + gameObject.name + " - Stop sign image deactivated.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // When the player leaves the zone, hide the stop sign image.
        SetAlpha(stopSignImage, 0f);
        stopSignActive = false;
        Debug.Log("Player exited stop sign zone: " + gameObject.name + " - Stop sign image deactivated.");
    }

    // Helper method to set the alpha value of the UI Image.
    private void SetAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}
