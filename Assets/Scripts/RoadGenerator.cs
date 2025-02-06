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

    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=[out:json];way(around:400,41.65962,-91.53464)[highway];out geom;";

    private double3 LatLongToUnityPosition(float lat, float lon, float height)
    {
        double3 earthPosition = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(lon, lat, height));
        return cesiumGeoreference.TransformEarthCenteredEarthFixedPositionToUnity(earthPosition);
    }

    void Start() {
        StartCoroutine(FetchRoadData(overpassUrl));
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
        Debug.Log("Full JSON Response: " + json);

        foreach (var way in json["elements"])
        {
            if ((string)way["type"] != "way") continue;

            List<Vector3> roadPoints = new List<Vector3>();

            Debug.Log($"\nWay ID: {way["id"]}");

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
        LineRenderer lineRenderer = roadObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = road.Count;
        lineRenderer.SetPositions(road.ToArray());

        // Customize LineRenderer
        lineRenderer.startWidth = 2f;
        lineRenderer.endWidth = 2f;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 5;

        // Make it pink
        Material pinkMaterial = new Material(Shader.Find("Unlit/Color"));
        pinkMaterial.color = Color.magenta;
        lineRenderer.material = pinkMaterial;

        Debug.Log($"Drawn road with {road.Count} points.");
    }

    private static Vector3 toVector3(double3 x) {
        return new Vector3((float)x.x, (float)x.y, (float)x.z);
    }
}
