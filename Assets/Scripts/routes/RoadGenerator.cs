using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using CesiumForUnity;
using Unity.Mathematics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class RoadGenerator : MonoBehaviour
{
    public CesiumGeoreference cesiumGeoreference;

    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=";

    private HashSet<long> processedWayIds = new HashSet<long>();

    private float baseLat = 41.65962f;
    private float baseLon = -91.53464f;
    private float gridSize = 0.01f; // Step size for sweeping (~1km per step)
    private int numStepsX = 2;
    private int numStepsY = 2;

    private double3 LatLongToUnityPosition(float lat, float lon, float height)
    {
        double3 earthPosition = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(lon, lat, height));
        return cesiumGeoreference.TransformEarthCenteredEarthFixedPositionToUnity(earthPosition);
    }

    void Start()
    {
        StartCoroutine(SweepArea());
    }

    private IEnumerator SweepArea()
    {
        for (int i = -numStepsX; i <= numStepsX; i++)
        {
            for (int j = -numStepsY; j <= numStepsY; j++)
            {
                float lat = baseLat + (j * gridSize);
                float lon = baseLon + (i * gridSize);

                string query = $@"
                    [out:json];
                    way(around:500,{lat},{lon})
                    ['highway'~'primary|secondary|tertiary|motorway|trunk|motorway_link|trunk_link|primary_link|secondary_link|tertiary_link|residential'];
                    out geom;
                ";

                Debug.Log($"Fetching roads for lat: {lat}, lon: {lon}");
                yield return StartCoroutine(FetchRoadData(overpassUrl + query));
            }
        }
    }

    private IEnumerator FetchRoadData(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                ParseAndDrawRoads(jsonResponse);
            }
            else
            {
                Debug.LogError("Failed to fetch road data: " + request.error);
            }
        }
    }

    private void ParseAndDrawRoads(string jsonResponse)
    {
        JObject json = JObject.Parse(jsonResponse);

        foreach (var way in json["elements"])
        {
            if ((string)way["type"] != "way") continue;

            long wayId = (long)way["id"];

            if (processedWayIds.Contains(wayId)) continue;

            List<Vector3> roadPoints = new List<Vector3>();

            foreach (var node in way["geometry"])
            {
                float lat = (float)node["lat"];
                float lon = (float)node["lon"];

                double3 unityPos = LatLongToUnityPosition(lat, lon, 177.6583f + 10);
                Vector3 worldPosition = toVector3(unityPos);
                roadPoints.Add(worldPosition);

                if (roadPoints.Count > 1)
                {
                    DrawRoad(roadPoints);
                }

            }
        }
    }

    private void DrawRoad(List<Vector3> road)
    {
        if (road.Count < 2) return; // Skip roads with only one point

        GameObject roadObject = new GameObject("RoadSegment");
        GameObject roadManager = GameObject.Find("RoadManager");

        LineRenderer lineRenderer = roadObject.AddComponent<LineRenderer>();
        roadObject.transform.SetParent(roadManager.transform, false);



        lineRenderer.positionCount = road.Count;
        lineRenderer.SetPositions(road.ToArray());

        // Customize LineRenderer
        lineRenderer.startWidth = 5f;
        lineRenderer.endWidth = 5f;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 5;

        // Make it pink
        Material pinkMaterial = new Material(Shader.Find("Unlit/Color"));
        pinkMaterial.color = Color.magenta;
        lineRenderer.material = pinkMaterial;
    }

    private static Vector3 toVector3(double3 x)
    {
        return new Vector3((float)x.x, (float)x.y, (float)x.z);
    }
}
