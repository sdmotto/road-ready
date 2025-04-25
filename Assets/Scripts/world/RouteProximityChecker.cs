using UnityEngine;

public class RouteProximityChecker : MonoBehaviour
{
    [Header("Proximity Settings")]
    public float maxDistanceFromRoute = 15f;
    public float checkInterval = 0.5f;

    private Transform currentRoute;
    private LineRenderer routeLine;

    private float lastCheckTime = 0f;

    void Start()
    {
        enabled = false;
    }

    void Update()
    {
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            CheckProximity();
        }
    }

    void CheckProximity()
    {
        if (currentRoute == null || routeLine == null) return;

        Vector3 closest = FindClosestPointOnRoute(transform.position);
        float dist = Vector3.Distance(transform.position, closest);

        if (dist > maxDistanceFromRoute)
        {
            Debug.LogWarning($"[RouteProximityChecker] Player is off-route! Distance: {dist:F2}");

            // Find the StartMarker inside the currentRoute
            Transform startMarker = null;
            foreach (Transform child in currentRoute)
            {
                if (child.CompareTag("StartMarker"))
                {
                    startMarker = child;
                    break;
                }
            }

            if (startMarker != null)
            {
                // Teleport with same logic as initial spawn
                if (routeLine.positionCount >= 2)
                {
                    Vector3 start = routeLine.GetPosition(0);
                    Vector3 next = routeLine.GetPosition(1);
                    Vector3 direction = (next - start).normalized;

                    float spawnOffset = 10f;
                    Vector3 offsetPosition = start - direction * spawnOffset + Vector3.up * 2.5f;
                    transform.position = offsetPosition;
                    GetComponent<PrometeoCarController>()?.resetRotation();

                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(direction);
                    }
                }
                else
                {
                    transform.position = startMarker.position + Vector3.up * 2.5f;
                }

                // Stop velocity
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                // Reset car state

                // Reset grading
                scoreScript scoring = GetComponent<scoreScript>();
                timerScript timer = GetComponent<timerScript>();
                if (scoring != null)
                {
                    Debug.Log("[RouteProximityChecker] Restarting route...");
                    scoring.ResetScore();
                    timer.StopTimer();
                }

                lastCheckTime = Time.time + 1f;
            }
            else
            {
                Debug.LogError("[RouteProximityChecker] StartMarker not found!");
            }
        }
    }

    Vector3 FindClosestPointOnRoute(Vector3 position)
    {
        float minDist = float.MaxValue;
        Vector3 closest = Vector3.zero;

        for (int i = 0; i < routeLine.positionCount - 1; i++)
        {
            Vector3 a = routeLine.GetPosition(i);
            Vector3 b = routeLine.GetPosition(i + 1);
            Vector3 point = ClosestPointOnSegment(a, b, position);
            float dist = Vector3.Distance(position, point);

            if (dist < minDist)
            {
                minDist = dist;
                closest = point;
            }
        }

        return closest;
    }

    // Utility: Closest point on a segment from A to B
    Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    // Call this when the route is loaded
    public void SetCurrentRoute(GameObject route)
    {
        currentRoute = route.transform;

        // Try to find the LineRenderer in this GameObject or its children
        routeLine = route.GetComponentInChildren<LineRenderer>();
    }
}
