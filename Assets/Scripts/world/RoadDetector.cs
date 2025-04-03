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
    public float updateInterval = 5f;
    private string googleMapsApiKey;
    public TMP_Text text;

    void Start()
    {
        googleMapsApiKey = EnvLoader.GetEnv("GOOGLE_API_KEY");
        StartCoroutine(GetRoadLoop());
    }

    IEnumerator GetRoadLoop()
    {
        while (true)
        {
            if (trackedObject != null && cesiumGeoreference != null)
            {
                double3 ecef = cesiumGeoreference.TransformUnityPositionToEarthCenteredEarthFixed(toDouble3(trackedObject.position));
                double3 latLonHeight = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
                double lat = latLonHeight.y;
                double lon = latLonHeight.x;

                yield return StartCoroutine(GetRoadName(lat, lon));
            }

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
            //Debug.Log("Raw response: " + json);

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
                                string road = comp["long_name"].ToString();
                                //Debug.Log($"You're currently on: {road}");

                                if (text != null)
                                {
                                    text.text = road;
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
}
