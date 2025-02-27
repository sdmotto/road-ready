using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using CesiumForUnity;
using Unity.Mathematics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class sidewalkGenerator : MonoBehaviour
{
    public CesiumGeoreference cesiumGeoreference;

    private string overpassUrl = "https://overpass-api.de/api/interpreter?data=";

    private HashSet<long> processedWayIds = new HashSet<long>();

    private float baseLat = 41.65962f;
    private float baseLon = -91.53464f;
    private float gridSize = 0.01f; // Step size for sweeping (~1km per step)
    private int numStepsX = 2;
    private int numStepsY = 2;

    // Extrusion settings for the wall (both upward and downward)
    public float wallHeightUp = 50f;
    public float wallHeightDown = 50f;

    private double3 LatLongToUnityPosition(float lat, float lon, float height)
    {
        double3 earthPosition = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(lon, lat, height));
        return cesiumGeoreference.TransformEarthCenteredEarthFixedPositionToUnity(earthPosition);
    }

    void Start() {
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
                    way(around:500,{lat},{lon}) ['highway'~'footway'] ['footway'!~'crossing'];
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
            processedWayIds.Add(wayId);

            List<Vector3> roadPoints = new List<Vector3>();

            foreach (var node in way["geometry"])
            {
                float lat = (float)node["lat"];
                float lon = (float)node["lon"];

                // Compute an initial worldPosition using a rough height.
                double3 unityPos = LatLongToUnityPosition(lat, lon, 177.6583f + 10);
                Vector3 worldPosition = toVector3(unityPos);

                // Cast a ray from a fixed high y-value to determine the ground height.
                float fixedRayOriginY = 2000f;
                Vector3 rayOrigin = new Vector3(worldPosition.x, fixedRayOriginY, worldPosition.z);
                float rayDistance = fixedRayOriginY + 2000f; // Adjust as needed for your scene scale

                Ray ray = new Ray(rayOrigin, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
                {
                    worldPosition.y = hit.point.y;
                }
                
                // Store the sidewalk point.
                Data.Instance.sidewalkPoints.Add(worldPosition);
                roadPoints.Add(worldPosition);
            }
            
            // Once all nodes for this way are processed, if there’s more than one point, draw the wall.
            if (roadPoints.Count > 1)
            {
                DrawRoad(roadPoints);
            }
        }
    }

    /// <summary>
    /// Creates a tall wall mesh that extrudes both upward and downward along the sidewalk polyline.
    /// </summary>
    /// <param name="road">A list of sidewalk points for this way.</param>
    private void DrawRoad(List<Vector3> road)
    {
        if (road.Count < 2) return; // Need at least two points to form a segment

        // Create a new GameObject to hold the wall mesh.
        GameObject wallObject = new GameObject("SidewalkWallSegment");
        GameObject roadManager = GameObject.Find("SidewalkManager");
        if (roadManager != null)
        {
            wallObject.transform.SetParent(roadManager.transform, false);
        }

        MeshFilter meshFilter = wallObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wallObject.AddComponent<MeshRenderer>();

        // Create a material for visibility.
        Material wallMaterial = new Material(Shader.Find("Unlit/Color"));
        wallMaterial.color = Color.blue;  // Change the color if desired
        meshRenderer.material = wallMaterial;

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // For each segment, create a quad that extends downward and upward from the sidewalk.
        // Define the bottom vertices (extruded downward) and the top vertices (extruded upward).
        for (int i = 0; i < road.Count - 1; i++)
        {
            Vector3 p0 = road[i];
            Vector3 p1 = road[i + 1];

            // Bottom vertices (extruded downward)
            Vector3 b0 = p0 - Vector3.up * wallHeightDown;
            Vector3 b1 = p1 - Vector3.up * wallHeightDown;
            // Top vertices (extruded upward)
            Vector3 t0 = p0 + Vector3.up * wallHeightUp;
            Vector3 t1 = p1 + Vector3.up * wallHeightUp;

            int startIndex = vertices.Count;
            vertices.Add(b0); // 0: bottom left
            vertices.Add(b1); // 1: bottom right
            vertices.Add(t1); // 2: top right
            vertices.Add(t0); // 3: top left

            // Create two triangles for the quad.
            triangles.Add(startIndex);     // Triangle 1: b0, b1, t1
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);

            triangles.Add(startIndex);     // Triangle 2: b0, t1, t0
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 3);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    private static Vector3 toVector3(double3 x) {
        return new Vector3((float)x.x, (float)x.y, (float)x.z);
    }
}
