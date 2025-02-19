using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CesiumForUnity;    // Or whatever namespace Cesium uses in your setup
using Unity.Mathematics; // For double3, math.sin, etc.
using System;            // For Math.Sqrt, Math.Sin, etc.

public class speedScript : MonoBehaviour
{
    [SerializeField] private CesiumGeoreference cesiumGeoreference;
    [SerializeField] private TMP_Text speedText;
    
    // How often (in seconds) to update the speed reading.
    [SerializeField] private float updateInterval = 0.5f; 

    // We'll store the previous latitude/longitude and time 
    private double lastLat;
    private double lastLon;
    private float lastTime;

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

            // Update for next interval
            lastLat = currentLat;
            lastLon = currentLon;
            lastTime = Time.time;
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
}
