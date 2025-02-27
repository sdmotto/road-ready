using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SidewalkDetector : MonoBehaviour
{
    // Adjustable threshold in Unity units (distance in XZ plane) for sidewalk detection.
    public float threshold = 0.2f;
    
    // The amount of time (in seconds) the detection must be continuously true.
    public float detectionTimeThreshold = 0.5f;
    
    // Additional margin to ensure the sample point is clearly closer to the sidewalk than to the road.
    public float margin = 0.1f;
    
    // Timer to track consecutive detection time.
    private float sidewalkTimer = 0f;

    // Sample offsets (in local space) to check around the car.
    // Adjust these offsets based on your car's dimensions.
    public Vector3[] sampleOffsets = new Vector3[]
    {
        new Vector3(0f, 0f, 0f),         // center
        new Vector3(0.5f, 0f, 0.5f),       // front-right
        new Vector3(-0.5f, 0f, 0.5f),      // front-left
        new Vector3(0.5f, 0f, -0.5f),      // rear-right
        new Vector3(-0.5f, 0f, -0.5f)      // rear-left
    };

    void Update()
    {
        // Ensure that both sidewalk and road data are available.
        if (Data.Instance != null &&
            Data.Instance.sidewalkPoints != null && Data.Instance.sidewalkPoints.Count > 1 &&
            Data.Instance.roadPoints != null && Data.Instance.roadPoints.Count > 1)
        {
            // Get the current detection result using both sidewalk and road data.
            bool currentDetection = IsCarOnSidewalk(transform.position, Data.Instance.sidewalkPoints, Data.Instance.roadPoints, threshold);

            // If detection is positive, accumulate delta time; otherwise, reset.
            if (currentDetection)
            {
                sidewalkTimer += Time.deltaTime;
            }
            else
            {
                sidewalkTimer = 0f;
            }

            // Only consider the car on the sidewalk if the detection has been active for at least the threshold time.
            if (sidewalkTimer >= detectionTimeThreshold)
            {
                Debug.Log("sidewalk");
                // Additional behavior for sidewalk detection.
            }
            else
            {
                Debug.Log("not sidewalk");
            }
        }
        else
        {
            Debug.LogWarning("Sidewalk or road data is not available");
        }
    }

    /// <summary>
    /// Checks for each sample point around the car whether it's close to the sidewalk and not near the road.
    /// </summary>
    private bool IsCarOnSidewalk(Vector3 carPos, List<Vector3> sidewalkPoints, List<Vector3> roadPoints, float threshold)
    {
        foreach (Vector3 offset in sampleOffsets)
        {
            Vector3 samplePoint = carPos + offset;
            Vector2 sampleXZ = new Vector2(samplePoint.x, samplePoint.z);

            // Get minimum distance from the sample to the sidewalk and road polylines.
            float ds = GetMinDistanceToPolyline(sampleXZ, sidewalkPoints);
            float dr = GetMinDistanceToPolyline(sampleXZ, roadPoints);

            // Debug output (optional)
            // Debug.Log($"Sample: {samplePoint}, ds: {ds}, dr: {dr}");

            // If the sample point is closer to the sidewalk than the threshold,
            // and it is also clearly farther from the road (by at least 'margin'),
            // consider it a sidewalk detection.
            if (ds < threshold && ds < dr - margin)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Computes the minimum distance (in XZ plane) from a point to a polyline defined by a list of Vector3.
    /// </summary>
    private float GetMinDistanceToPolyline(Vector2 point, List<Vector3> polyline)
    {
        float minDist = float.MaxValue;
        for (int i = 0; i < polyline.Count - 1; i++)
        {
            Vector2 a = new Vector2(polyline[i].x, polyline[i].z);
            Vector2 b = new Vector2(polyline[i + 1].x, polyline[i + 1].z);
            float d = DistancePointToSegment(point, a, b);
            if (d < minDist)
                minDist = d;
        }
        return minDist;
    }

    /// <summary>
    /// Returns the shortest distance from point p to the segment ab (all in 2D).
    /// </summary>
    private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float abSqr = ab.sqrMagnitude;
        if (abSqr == 0f)
            return Vector2.Distance(p, a);

        // Projection factor t of point p onto the line ab.
        float t = Vector2.Dot(p - a, ab) / abSqr;
        t = Mathf.Clamp01(t);
        Vector2 projection = a + t * ab;
        return Vector2.Distance(p, projection);
    }
}
