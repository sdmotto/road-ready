using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using CesiumForUnity;
using Unity.Mathematics;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class TrafficSignalGenerator : MonoBehaviour
{
    [Header("Cesium & Overpass Settings")]
    public CesiumGeoreference cesiumGeoreference;
    [SerializeField] private float baseLat = 41.65962f;
    [SerializeField] private float baseLon = -91.53464f;
    [SerializeField] private int queryRadius = 1000;
    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=";
    private HashSet<long> processedTrafficSignalIds = new HashSet<long>();

    [Header("Zone & UI Settings")]
    // Reference to the UI Image for the traffic signal indicator (invisible by default).
    public Image trafficSignalImage;
    // Reference to the TextMeshPro element displaying the player's speed.
    public TMP_Text speedText;
    // The detection zone radius (Unity units).
    public float zoneRadius = 20f;
    // Penalty for leaving the zone without stopping.
    public float trafficSignalPenalty = 10f;

    [Header("Scoring")]
    // Reference to the score manager.
    public scoreScript scoreManager;

    void Start()
    {
        // Ensure the traffic signal image starts invisible.
        SetAlpha(trafficSignalImage, 0f);
        StartCoroutine(SweepArea());
    }

    // Convert latitude, longitude, and a given height to a Unity world position.
    private double3 LatLongToUnityPosition(float lat, float lon, float height)
    {
        double3 earthPosition = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(lon, lat, height));
        return cesiumGeoreference.TransformEarthCenteredEarthFixedPositionToUnity(earthPosition);
    }

    // Sweep the area around the base coordinate and query traffic signal nodes.
    private IEnumerator SweepArea()
    {
        float lat = baseLat;
        float lon = baseLon;
        string query = $@"
            [out:json];
            node(around:{queryRadius},{lat},{lon})[""highway""=""traffic_signals""];
            out geom;
        ";

        Debug.Log($"Fetching traffic signal data for lat: {lat}, lon: {lon}");
        yield return StartCoroutine(FetchTrafficSignalData(overpassUrl + query));
    }

    // Fetch traffic signal data using UnityWebRequest.
    private IEnumerator FetchTrafficSignalData(string url)
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
                Debug.LogError("Failed to fetch traffic signal data: " + request.error);
            }
        }
    }

    // Parse the JSON response and create a detection zone for each traffic signal.
    private void ParseAndCreateZones(string jsonResponse)
    {
        JObject json = JObject.Parse(jsonResponse);
        foreach (var element in json["elements"])
        {
            if ((string)element["type"] != "node")
                continue;

            long nodeId = (long)element["id"];
            if (processedTrafficSignalIds.Contains(nodeId))
                continue;

            processedTrafficSignalIds.Add(nodeId);

            float lat = (float)element["lat"];
            float lon = (float)element["lon"];

            // Convert lat/lon to a world position (using height = 0).
            double3 basePos = LatLongToUnityPosition(lat, lon, 0f);
            Vector3 worldBasePos = ToVector3(basePos);

            // Create the detection zone at this position.
            CreateTrafficSignalZone(worldBasePos);
        }
    }

    // Create a detection zone (with visual debugging) at the given position.
    private void CreateTrafficSignalZone(Vector3 position)
    {
        GameObject zone = new GameObject("TrafficSignalZone");
        zone.transform.position = position;
        // Tag the zone so it can be detected later.
        zone.tag = "TrafficSignalZone";

        // Parent all zones under "trafficSignalZones".
        GameObject parent = GameObject.Find("trafficSignalZones");
        if (parent == null)
        {
            parent = new GameObject("trafficSignalZones");
        }
        zone.transform.parent = parent.transform;

        // Add a CapsuleCollider to simulate a vertical cylinder.
        CapsuleCollider collider = zone.AddComponent<CapsuleCollider>();
        collider.radius = zoneRadius;
        collider.height = 10000f; // Effectively infinite height.
        collider.direction = 1; // Y-axis.
        collider.isTrigger = true;

        // Create a visual representation: a transparent blue cylinder.
        GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zoneVisual.transform.parent = zone.transform;
        zoneVisual.transform.localPosition = Vector3.zero;
        zoneVisual.transform.localRotation = Quaternion.identity;
        // Scale the cylinder to match the collider (default Unity Cylinder has a height of 2 units).
        float scaleY = 10000f / 2f;
        zoneVisual.transform.localScale = new Vector3(zoneRadius * 2f, scaleY, zoneRadius * 2f);
        Material blueMat = new Material(Shader.Find("Standard"));
        blueMat.color = new Color(0f, 0f, 1f, 0.3f); // Blue color for visual differentiation.
        blueMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        blueMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        blueMat.SetInt("_ZWrite", 0);
        blueMat.DisableKeyword("_ALPHATEST_ON");
        blueMat.EnableKeyword("_ALPHABLEND_ON");
        blueMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        blueMat.renderQueue = 3000;
        zoneVisual.GetComponent<MeshRenderer>().material = blueMat;
        // Remove the collider from the visual, as it's only for debugging.
        Destroy(zoneVisual.GetComponent<Collider>());

        Debug.Log($"Created traffic signal zone at position: {position}");

        // Add the trigger component to handle zone events.
        ZoneTrigger trigger = zone.AddComponent<ZoneTrigger>();
        trigger.trafficSignalImage = trafficSignalImage;
        trigger.speedText = speedText;
        trigger.trafficSignalPenalty = trafficSignalPenalty;
        trigger.scoreManager = scoreManager;
    }

    // Helper to convert from double3 to Unity's Vector3.
    private static Vector3 ToVector3(double3 d)
    {
        return new Vector3((float)d.x, (float)d.y, (float)d.z);
    }

    // Helper method to set the alpha value of a UI Image.
    private void SetAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    // Nested class to handle trigger events for a traffic signal zone.
    public class ZoneTrigger : MonoBehaviour
    {
        public Image trafficSignalImage;
        public TMP_Text speedText;
        public float trafficSignalPenalty;
        public scoreScript scoreManager;
        private bool trafficSignalActive = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                return;

            // Activate the traffic signal indicator.
            SetAlpha(trafficSignalImage, 1f);
            trafficSignalActive = true;
            Debug.Log("Player entered traffic signal zone: " + gameObject.name + " - Traffic signal image activated.");
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
                return;

            if (trafficSignalActive && speedText != null)
            {
                // Remove non-numeric characters from the speed text.
                string currentSpeed = Regex.Replace(speedText.text, @"[^0-9.\-]+", "");
                double currSpeedDB;
                if (double.TryParse(currentSpeed, out currSpeedDB) && currSpeedDB < 0.1)
                {
                    SetAlpha(trafficSignalImage, 0f);
                    trafficSignalActive = false;
                    Debug.Log("Player stopped in zone: " + gameObject.name + " - Traffic signal image deactivated.");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                return;

            // If the player exits while the traffic signal indicator is still active, apply a penalty.
            if (trafficSignalActive)
            {
                Debug.Log("Player left traffic signal zone without stopping: " + gameObject.name + " - 10 point penalty applied.");
                if (scoreManager != null)
                {
                    scoreManager.noStop();
                }
            }
            SetAlpha(trafficSignalImage, 0f);
            trafficSignalActive = false;
            Debug.Log("Player exited traffic signal zone: " + gameObject.name + " - Traffic signal image deactivated.");
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
}
