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
    [SerializeField] private float baseLat = 41.65962f; // iowa city
    [SerializeField] private float baseLon = -91.53464f; // iowa city
    [SerializeField] private int queryRadius = 1000;
    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=";
    private HashSet<long> processedTrafficSignalIds = new HashSet<long>();

    [Header("Zone & UI Settings")]
    public TMP_Text speedText;
    public float zoneRadius = 5f;
    public int redLightPenalty = 5;
    public int yellowLightPenalty = 2;

    [Header("Scoring")]
    public scoreScript scoreManager;

    [Header("Traffic Light Images")]
    public Image redLightImage;
    public Image yellowLightImage;
    public Image greenLightImage;

    void Start()
    {
        // Ensure all traffic signal images start invisible.
        SetAlpha(redLightImage, 0f);
        SetAlpha(yellowLightImage, 0f);
        SetAlpha(greenLightImage, 0f);

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

        //Debug.Log($"Fetching traffic signal data for lat: {lat}, lon: {lon}");
        yield return StartCoroutine(FetchTrafficSignalData(overpassUrl + query));
    }

    // Fetch traffic signal data
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

    // Create a detection zone at the given position.
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
        collider.height = 10000f; // basically infinite height.
        collider.direction = 1; // Y-axis.
        collider.isTrigger = true;

        // Create a visual representation: a transparent cylinder 
        GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zoneVisual.transform.parent = zone.transform;
        zoneVisual.transform.localPosition = Vector3.zero;
        zoneVisual.transform.localRotation = Quaternion.identity;
        float scaleY = 10000f / 2f;
        zoneVisual.transform.localScale = new Vector3(zoneRadius * 2f, scaleY, zoneRadius * 2f);
        Material visualMat = new Material(Shader.Find("Standard"));

        // Initial debug tint (red); this will update as the light cycles.
        visualMat.color = new Color(1f, 0f, 0f, 0.3f);
        visualMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        visualMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        visualMat.SetInt("_ZWrite", 0);
        visualMat.DisableKeyword("_ALPHATEST_ON");
        visualMat.EnableKeyword("_ALPHABLEND_ON");
        visualMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        visualMat.renderQueue = 3000;
        zoneVisual.GetComponent<MeshRenderer>().material = visualMat;
        Destroy(zoneVisual.GetComponent<Collider>());

        //Debug.Log($"Created traffic signal zone at position: {position}");

        // Add the trigger component to handle zone events and light cycling.
        ZoneTrigger trigger = zone.AddComponent<ZoneTrigger>();
        trigger.speedText = speedText;
        trigger.redLightPenalty = redLightPenalty;
        trigger.scoreManager = scoreManager;
        trigger.zoneVisualRenderer = zoneVisual.GetComponent<MeshRenderer>();

        // Assign the traffic light images.
        trigger.redLightImage = redLightImage;
        trigger.yellowLightImage = yellowLightImage;
        trigger.greenLightImage = greenLightImage;
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

    // Nested class to handle trigger events and light cycling for a traffic signal zone.
    public class ZoneTrigger : MonoBehaviour
    {
        public TMP_Text speedText;
        public int redLightPenalty = 5;
        public int yellowLightPenalty = 2;
        public scoreScript scoreManager;
        public MeshRenderer zoneVisualRenderer;

        public Image redLightImage;
        public Image yellowLightImage;
        public Image greenLightImage;

        public enum LightStatus { Red, Yellow, Green }
        private LightStatus currentLightStatus;
        private Coroutine cycleCoroutine;

        private bool playerInside = false;

        private void Start()
        {
            // Initialize with a random state so zones start out unsynchronized.
            currentLightStatus = (LightStatus)UnityEngine.Random.Range(0, 3);
            UpdateZoneVisual();
            cycleCoroutine = StartCoroutine(CycleLights());
        }

        private IEnumerator CycleLights()
        {
            while (true)
            {
                float waitTime = 0f;
                switch (currentLightStatus)
                {
                    case LightStatus.Yellow:
                        waitTime = 10f;
                        currentLightStatus = LightStatus.Red;
                        break;
                    case LightStatus.Red:
                        waitTime = 10f;
                        currentLightStatus = LightStatus.Green;
                        break;
                    case LightStatus.Green:
                        waitTime = 3f;
                        currentLightStatus = LightStatus.Yellow;
                        break;
                }
                if (playerInside)
                {
                    ShowLight();
                }

                UpdateZoneVisual();
                yield return new WaitForSeconds(waitTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Process only if the player enters.
            if (other.CompareTag("Player")) return;
            playerInside = true;
            // Debug.Log("Player entered traffic signal zone: " + gameObject.name);
            ShowLight();
        }

        private void OnTriggerExit(Collider other)
        {
            // Process only if the player exits.
            if (other.CompareTag("Player")) return;
            playerInside = false;
            // Debug.Log("Player exited traffic signal zone: " + gameObject.name);
            HideAllLights();

            if (currentLightStatus == LightStatus.Red)
            {
                scoreManager.noStop(redLightPenalty); // call no stop function with 5 point penalty
            }
            else if (currentLightStatus == LightStatus.Yellow)
            {
                scoreManager.noStop(yellowLightPenalty); // call no stop function with 2 point penalty
            }
            else if (currentLightStatus == LightStatus.Green)
            {
                scoreManager.RegisterTrafficLightSuccess();
                // Debug.Log("OBEY");
            }
        }

        // Show only the UI image corresponding to the current light.
        private void ShowLight()
        {
            HideAllLights(); // Ensure all images are hidden first.
            switch (currentLightStatus)
            {
                case LightStatus.Red:
                    SetAlpha(redLightImage, 1f);
                    break;
                case LightStatus.Yellow:
                    SetAlpha(yellowLightImage, 1f);
                    break;
                case LightStatus.Green:
                    SetAlpha(greenLightImage, 1f);
                    break;
            }
        }

        // Hide all traffic light images.
        private void HideAllLights()
        {
            SetAlpha(redLightImage, 0f);
            SetAlpha(yellowLightImage, 0f);
            SetAlpha(greenLightImage, 0f);
        }

        // Update the zone's debug visual (the transparent cylinder) based on the current light.
        private void UpdateZoneVisual()
        {
            if (zoneVisualRenderer != null)
            {
                Color debugColor = Color.red;
                switch (currentLightStatus)
                {
                    case LightStatus.Red:
                        debugColor = new Color(1f, 0f, 0f, 0.3f);
                        break;
                    case LightStatus.Yellow:
                        debugColor = new Color(1f, 1f, 0f, 0.3f);
                        break;
                    case LightStatus.Green:
                        debugColor = new Color(0f, 1f, 0f, 0.3f);
                        break;
                }
                zoneVisualRenderer.material.color = debugColor;
            }
        }

        // Helper to set the alpha of a UI Image.
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
