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

public class StopSignGenerator : MonoBehaviour
{
    [Header("Cesium & Overpass Settings")]
    public CesiumGeoreference cesiumGeoreference;
    [SerializeField] private float baseLat = 41.65962f;
    [SerializeField] private float baseLon = -91.53464f;
    [SerializeField] private int queryRadius = 1000;
    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=";
    private HashSet<long> processedStopSignIds = new HashSet<long>();

    [Header("Zone & UI Settings")]
    // Reference to the UI Image for the stop sign indicator (invisible by default).
    public Image stopSignImage;
    // Reference to the TextMeshPro element displaying the player's speed.
    public TMP_Text speedText;
    // The detection zone radius (Unity units).
    public float zoneRadius = 20f;
    // Penalty for leaving the zone without stopping.
    public float stopSignPenalty = 10f;

    [Header("Scoring")]
    // Reference to the score manager.
    public scoreScript scoreManager;

    void Start()
    {
        // Ensure the stop sign image starts invisible.
        SetAlpha(stopSignImage, 0f);
        StartCoroutine(SweepArea());
    }

    // Convert latitude, longitude, and a given height to a Unity world position.
    private double3 LatLongToUnityPosition(float lat, float lon, float height)
    {
        double3 earthPosition = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(lon, lat, height));
        return cesiumGeoreference.TransformEarthCenteredEarthFixedPositionToUnity(earthPosition);
    }

    // Sweep the area around the base coordinate and query stop sign nodes.
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

            // Convert lat/lon to a world position (using height = 0).
            double3 basePos = LatLongToUnityPosition(lat, lon, 0f);
            Vector3 worldBasePos = ToVector3(basePos);

            // Create the detection zone at this position.
            CreateStopSignZone(worldBasePos);
        }
    }

    // Create a detection zone (with visual debugging) at the given position.
    private void CreateStopSignZone(Vector3 position)
    {
        GameObject zone = new GameObject("StopSignZone");
        zone.transform.position = position;
        // Tag the zone so it can be detected later.
        zone.tag = "StopSignZone";

        // Parent all zones under "stopSignZones".
        GameObject parent = GameObject.Find("stopSignZones");
        if (parent == null)
        {
            parent = new GameObject("stopSignZones");
        }
        zone.transform.parent = parent.transform;

        // Add a CapsuleCollider to simulate a vertical cylinder.
        CapsuleCollider collider = zone.AddComponent<CapsuleCollider>();
        collider.radius = zoneRadius;
        collider.height = 10000f; // Effectively infinite height.
        collider.direction = 1; // Y-axis.
        collider.isTrigger = true;

        // Create a visual representation: a transparent red cylinder.
        GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zoneVisual.transform.parent = zone.transform;
        zoneVisual.transform.localPosition = Vector3.zero;
        zoneVisual.transform.localRotation = Quaternion.identity;
        // Scale the cylinder to match the collider (default Unity Cylinder has a height of 2 units).
        float scaleY = 10000f / 2f;
        zoneVisual.transform.localScale = new Vector3(zoneRadius * 2f, scaleY, zoneRadius * 2f);
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = new Color(1f, 0f, 0f, 0.3f);
        redMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        redMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        redMat.SetInt("_ZWrite", 0);
        redMat.DisableKeyword("_ALPHATEST_ON");
        redMat.EnableKeyword("_ALPHABLEND_ON");
        redMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        redMat.renderQueue = 3000;
        zoneVisual.GetComponent<MeshRenderer>().material = redMat;
        // Remove the collider from the visual, as it's only for debugging.
        Destroy(zoneVisual.GetComponent<Collider>());

        Debug.Log($"Created stop sign zone at position: {position}");

        // Add the trigger component to handle zone events.
        ZoneTrigger trigger = zone.AddComponent<ZoneTrigger>();
        trigger.stopSignImage = stopSignImage;
        trigger.speedText = speedText;
        trigger.stopSignPenalty = stopSignPenalty;
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

    // Nested class to handle trigger events for a stop sign zone.
    public class ZoneTrigger : MonoBehaviour
    {
        public Image stopSignImage;
        public TMP_Text speedText;
        public float stopSignPenalty;
        public scoreScript scoreManager;
        private bool stopSignActive = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                return;

            // Activate the stop sign indicator.
            SetAlpha(stopSignImage, 1f);
            stopSignActive = true;
            Debug.Log("Player entered stop sign zone: " + gameObject.name + " - Stop sign image activated.");
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
                return;

            if (stopSignActive && speedText != null)
            {
                // Remove non-numeric characters from the speed text.
                string currentSpeed = Regex.Replace(speedText.text, @"[^0-9.\-]+", "");
                double currSpeedDB;
                if (double.TryParse(currentSpeed, out currSpeedDB) && currSpeedDB < 0.1)
                {
                    SetAlpha(stopSignImage, 0f);
                    stopSignActive = false;
                    Debug.Log("Player stopped in zone: " + gameObject.name + " - Stop sign image deactivated.");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                return;

            // If the player exits while the stop sign indicator is still active, apply a penalty.
            if (stopSignActive)
            {
                Debug.Log("Player left stop sign zone without stopping: " + gameObject.name + " - 10 point penalty applied.");
                if (scoreManager != null)
                {
                    Debug.Log("subtracting or supposedly");
                    scoreManager.noStop(10);
                }
            }
            SetAlpha(stopSignImage, 0f);
            stopSignActive = false;
            Debug.Log("Player exited stop sign zone: " + gameObject.name + " - Stop sign image deactivated.");
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
