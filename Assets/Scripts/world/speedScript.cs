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

public struct RoadData
{
    public string name;
    public string maxspeed;
}


public class speedScript : MonoBehaviour
{
    [SerializeField] private CesiumGeoreference cesiumGeoreference;
    [SerializeField] public TMP_Text speedText;

    // How often (in seconds) to update the speed reading.
    [SerializeField] private float updateInterval = 0.5f;

    [SerializeField] public TMP_Text speedLimitText;
    [SerializeField] public TMP_Text roadNameText;

    // private float speedLimitInterval = 5f;

    // We'll store the previous latitude/longitude and time 
    private double lastLat;
    private double lastLon;
    private float lastTime;

    private int counter = 20;

    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=";

    public float maxSpeed = 0f;
    public float totalSpeed = 0f;
    public int speedSamples = 0;
    public string directionTravelled = "";
    private string speedLimit = "";
    private string roadName = "";


    public float GetAverageSpeed()
    {
        return speedSamples > 0 ? totalSpeed / speedSamples : 0f;
    }

    public void setAverageSpeed()
    {
        speedSamples = 0;
        totalSpeed = 0f;
    }

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
            string direction = GetDirection(lastLat, lastLon, currentLat, currentLon);

            // Convert to speed in m/s, then mph
            double speedMps = distanceMeters / dt;
            double speedMph = speedMps * 2.23694; // 1 m/s ~ 2.23694 mph
            if (speedMph > 0.5)
            {
                directionTravelled = direction;
            }

            // Display speed to one decimal place
            speedText.text = speedMph.ToString("F1") + " MPH";

            // Update max speed, limit it to less than 200 mph because resetting the car can create an abnoramlly high speed
            if ((float)speedMph <= 200f && (float)speedMph > maxSpeed)
            {
                maxSpeed = (float)speedMph;
            }

            // Track total speed and number of samples, only include if below 200mph
            if ((float)speedMph <= 200f)
            {
                totalSpeed += (float)speedMph;
                speedSamples++;
            }

            if (counter >= 10)
            {
                StartCoroutine(GetSpeedDataCoroutine(lastLat, lastLon, (roadData) =>
                {
                    if (!string.IsNullOrEmpty(roadData.maxspeed) && !roadData.maxspeed.Equals("NA", StringComparison.OrdinalIgnoreCase))
                    {
                        speedLimit = roadData.maxspeed;
                        roadName = roadData.name;
                    }
                    else
                    {
                        speedLimitText.text = "API Error";
                    }
                }));
                counter = 0;
            }


            speedLimitText.text = speedLimit;
            roadNameText.text = directionTravelled + " on " + roadName;

            // Update for next interval
            lastLat = currentLat;
            lastLon = currentLon;
            lastTime = Time.time;
            counter++;
        }


    }


    //The "opposite" of LatLongToUnityPosition:
    // 1) Transform the given Unity world position to Earth-Centered Earth-Fixed (ECEF).
    // 2) Convert ECEF to (longitude, latitude, height).
    // 3) Reorder that to (latitude, longitude, height) in a double3.
    private double3 UnityPositionToLatLongHeight(Vector3 unityPos)
    {
        // 1) Unity -> ECEF.  Wrap the Vector3 into a double3 for proper type matching.
        double3 ecef = cesiumGeoreference.TransformUnityPositionToEarthCenteredEarthFixed(new double3(unityPos.x, unityPos.y, unityPos.z));

        // 2) ECEF -> (lon, lat, height)
        double3 lonLatHeight = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);

        // 3) Reorder to (lat, lon, height)
        return new double3(lonLatHeight.y, lonLatHeight.x, lonLatHeight.z);
    }


    // Returns the 2D "surface" distance (in meters) between two lat/lon points
    // using the Haversine formula. Assumes Earth is a sphere with radius ~6371 km.

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

    private IEnumerator GetSpeedDataCoroutine(double lat, double lon, Action<RoadData> callback)
    {
        RoadData roadData = new RoadData();
        string query = $@"
                        [out:json];
                        way(around:100,{lat},{lon})
                        ['highway'~'primary|secondary|tertiary|motorway|trunk|motorway_link|trunk_link|primary_link|secondary_link|tertiary_link|residential'];
                        out geom;
                        ";

        // URL-escape the query string.
        string url = overpassUrl + UnityWebRequest.EscapeURL(query);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                JObject json = JObject.Parse(jsonResponse);
                JArray elements = (JArray)json["elements"];
                JObject firstElement;

                if (elements != null && elements.Count > 0)
                {
                    if (elements.Count > 1 && (directionTravelled == "E" || directionTravelled == "W"))
                    {
                        firstElement = (JObject)elements[1];
                        Debug.Log("pos 1: " + firstElement);
                    }
                    else
                    {
                        firstElement = (JObject)elements[0];
                        Debug.Log("pos 0: " + firstElement);
                    }

                    JObject tags = (JObject)firstElement["tags"];
                    if (tags != null && tags.ContainsKey("maxspeed"))
                    {
                        roadData.maxspeed = (string)tags["maxspeed"];
                    }
                    if (tags != null && tags.ContainsKey("name"))
                    {
                        roadData.name = (string)tags["name"];
                    }
                    else
                    {
                        callback(new RoadData { name = "Unknown Road", maxspeed = null });
                    }
                }
                else
                {
                    callback(new RoadData { name = "Unknown Road", maxspeed = null });
                }
            }
            else
            {
                Debug.LogError("Failed to fetch road data: " + request.error);
                callback(new RoadData { name = "Unknown Road", maxspeed = null });
            }
        }

        callback(roadData);
    }

    // Returns the compass direction (e.g., North, South-East) from one lat/lon to another
    private string GetDirection(double lat1, double lon1, double lat2, double lon2)
    {
        // Convert degrees to radians
        double latRad1 = lat1 * Math.PI / 180.0;
        double latRad2 = lat2 * Math.PI / 180.0;
        double deltaLon = (lon2 - lon1) * Math.PI / 180.0;

        // Compute initial bearing
        double y = Math.Sin(deltaLon) * Math.Cos(latRad2);
        double x = Math.Cos(latRad1) * Math.Sin(latRad2) -
                Math.Sin(latRad1) * Math.Cos(latRad2) * Math.Cos(deltaLon);

        double bearingRad = Math.Atan2(y, x);
        double bearingDeg = (bearingRad * 180.0 / Math.PI + 360.0) % 360.0;

        return BearingToCompass(bearingDeg);
    }

    // Converts a bearing in degrees to a compass direction
    private string BearingToCompass(double bearing)
    {
        string[] directions = {
            "N", "NE", "E", "SE",
            "S", "SW", "W", "NW"
        };

        int index = (int)Math.Round(bearing / 45.0) % 8;
        return directions[index];
    }

}