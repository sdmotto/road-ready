using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CesiumForUnity;    // Or whatever namespace Cesium uses in your setup
using Unity.Mathematics; // For double3, math.sin, etc.
using System;            // For Math.Sqrt, Math.Sin, etc.
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class speedScript : MonoBehaviour
{
    [SerializeField] private CesiumGeoreference cesiumGeoreference;
    [SerializeField] private TMP_Text speedText;
    
    // How often (in seconds) to update the speed reading.
    [SerializeField] private float updateInterval = 0.5f;

    [SerializeField] private TMP_Text speedLimitText;

    private float speedLimitInterval = 5f;

    // We'll store the previous latitude/longitude and time 
    private double lastLat;
    private double lastLon;
    private float lastTime;

    private int counter = 20;

    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=";

    void Start()
    {
        // Initialize lat/lon from the car's starting position
        double3 latLonHeight = UnityPositionToLatLongHeight(transform.position);
        lastLat = latLonHeight.x;  // lat
        lastLon = latLonHeight.y;  // lon
        lastTime = Time.time;
    }

    void Update()
    {
        float dt = Time.time - lastTime;
        if (dt >= updateInterval)
        {
            // Get the current lat/lon
            double3 currentLatLonHeight = UnityPositionToLatLongHeight(transform.position);
            double currentLat = currentLatLonHeight.x;
            double currentLon = currentLatLonHeight.y;

            // Compute distance (in meters) using the Haversine formula (2D on Earth's surface)
            double distanceMeters = HaversineDistance(lastLat, lastLon, currentLat, currentLon);

            // Convert to speed in m/s, then mph
            double speedMps = distanceMeters / dt;
            double speedMph = speedMps * 2.23694; // 1 m/s ~ 2.23694 mph

            // Display speed to one decimal place
            speedText.text = speedMph.ToString("F1") + " MPH";

            if (counter >= 10)
            {
                StartCoroutine(GetSpeedDataCoroutine(lastLat, lastLon, (limit) => {
                    // If limit is null or equals "NA" (ignoring case), set default of 25 MPH.
                    if (!string.IsNullOrEmpty(limit) && !limit.Equals("NA", StringComparison.OrdinalIgnoreCase))
                    {
                        speedLimitText.text = limit;
                    }
                    else
                    {
                        speedLimitText.text = "API Error";
                    }
                }));
                counter = 0;
            }

            // Update for next interval
            lastLat = currentLat;
            lastLon = currentLon;
            lastTime = Time.time;
            counter++;
        }
    }

    /// <summary>
    /// The "opposite" of LatLongToUnityPosition:
    /// 1) Transform the given Unity world position to Earth-Centered Earth-Fixed (ECEF).
    /// 2) Convert ECEF to (longitude, latitude, height).
    /// 3) Reorder that to (latitude, longitude, height) in a double3.
    /// </summary>
    private double3 UnityPositionToLatLongHeight(Vector3 unityPos)
    {
        // 1) Unity -> ECEF.  Wrap the Vector3 into a double3 for proper type matching.
        double3 ecef = cesiumGeoreference.TransformUnityPositionToEarthCenteredEarthFixed(new double3(unityPos.x, unityPos.y, unityPos.z));

        // 2) ECEF -> (lon, lat, height)
        double3 lonLatHeight = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);

        // 3) Reorder to (lat, lon, height)
        return new double3(lonLatHeight.y, lonLatHeight.x, lonLatHeight.z);
    }

    /// <summary>
    /// Returns the 2D "surface" distance (in meters) between two lat/lon points
    /// using the Haversine formula. Assumes Earth is a sphere with radius ~6371 km.
    /// </summary>
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Earth radius in meters
        const double R = 6371000.0;  

        // Convert degrees to radians
        double latRad1 = lat1 * math.PI / 180.0;
        double latRad2 = lat2 * math.PI / 180.0;
        double deltaLat = (lat2 - lat1) * math.PI / 180.0;
        double deltaLon = (lon2 - lon1) * math.PI / 180.0;

        double sinDeltaLat = Math.Sin(deltaLat / 2);
        double sinDeltaLon = Math.Sin(deltaLon / 2);

        // Haversine formula
        double a = sinDeltaLat * sinDeltaLat +
                   Math.Cos(latRad1) * Math.Cos(latRad2) *
                   sinDeltaLon * sinDeltaLon;

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Distance in meters on Earth's surface
        return R * c;
    }

    private IEnumerator GetSpeedDataCoroutine(double lat, double lon, Action<string> callback)
    {
        string query = $@"
                        [out:json];
                        way(around:100,{lat},{lon})
                        ['highway'~'primary|secondary|tertiary|motorway|trunk|motorway_link|trunk_link|primary_link|secondary_link|tertiary_link|residential'];
                        out geom;
                        ";

        // It is a good idea to URL-escape the query string.
        string url = overpassUrl + UnityWebRequest.EscapeURL(query);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                JObject json = JObject.Parse(jsonResponse);
                JArray elements = (JArray)json["elements"];

                if (elements != null && elements.Count > 0)
                {
                    JObject firstElement = (JObject)elements[0];
                    JObject tags = (JObject)firstElement["tags"];
                    if (tags != null && tags.ContainsKey("maxspeed"))
                    {
                        string maxSpeedStr = (string)tags["maxspeed"];
                        callback(maxSpeedStr);
                    }
                    else
                    {
                        callback(null);
                    }
                }
                else
                {
                    callback(null);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch road data: " + request.error);
                callback(null);
            }
        }
    }
}
