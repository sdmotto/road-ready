using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using CesiumForUnity;
using TMPro;

public class RoadDetector : MonoBehaviour
{
    public Transform trackedObject; // Your Unity world object
    public CesiumGeoreference cesiumGeoreference;
    public float updateInterval = 0.1f;
    private string googleMapsApiKey;
    public TMP_Text text;
    private Vector3 lastPosition;
    public Vector3 travelDirection { get; private set; }  // Public getter if you need access elsewhere


    void Start()
    {
        googleMapsApiKey = EnvManager.Get("GOOGLE_API_KEY");
        StartCoroutine(GetRoadLoop());
    }

    IEnumerator GetRoadLoop()
    {
        while (true)
        {
            if (trackedObject != null && cesiumGeoreference != null)
            {
                Vector3 currentPosition = trackedObject.position;
                travelDirection = (currentPosition - lastPosition);
                if(!(travelDirection.x == 0 && travelDirection.z == 0)){
                    lastPosition = currentPosition;
                }
                

                double3 ecef = cesiumGeoreference.TransformUnityPositionToEarthCenteredEarthFixed(toDouble3(trackedObject.position));
                double3 latLonHeight = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
                double lat = latLonHeight.y;
                double lon = latLonHeight.x;

                yield return StartCoroutine(GetRoadName(lat, lon));
            }
            Debug.Log("sent response");
            yield return new WaitForSeconds(updateInterval);
        }
    }

    IEnumerator GetRoadName(double lat, double lon)
    {
        string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lon}&key={googleMapsApiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            try
            {
                JObject data = JObject.Parse(json);
                JArray results = (JArray)data["results"];

                foreach (JObject result in results)
                {
                    JArray components = (JArray)result["address_components"];
                    foreach (JObject comp in components)
                    {
                        JArray types = (JArray)comp["types"];
                        foreach (JToken t in types)
                        {
                            if (t.ToString() == "route")
                            {
                                string roadDirection = GetCardinalDirection(travelDirection) + " on " + comp["long_name"].ToString();
                                Debug.Log("Recieved response");

                                if (text != null)
                                {
                                    text.text = roadDirection;
                                }

                                yield break;
                            }
                        }
                    }
                }

                Debug.Log("Road name not found in response.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error parsing response: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Error retrieving road name: " + request.error);
        }
    }

    private static double3 toDouble3(Vector3 x) {
        return new double3(x.x, x.y, x.z);
    }

    private string GetCardinalDirection(Vector3 direction) {

    Vector2 dir2D = new Vector2(direction.x, direction.z);
        float angle = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f) return "E";
        if (angle >= 22.5f && angle < 67.5f) return "NE";
        if (angle >= 67.5f && angle < 112.5f) return "N";
        if (angle >= 112.5f && angle < 157.5f) return "NW";
        if (angle >= 157.5f && angle < 202.5f) return "W";
        if (angle >= 202.5f && angle < 247.5f) return "SW";
        if (angle >= 247.5f && angle < 292.5f) return "S";
        if (angle >= 292.5f && angle < 337.5f) return "SE";

        return "Unknown";
    }

}
