using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.EventSystems;

public class menuScript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;
    public TMP_Text menuText;
    public GameObject notifPanel;

    private bool menuActive = false;
    private bool routesDisplayed = false;

    private List<string> routeNames = new List<string>();
    private int selectedIndex = 0;

    public Transform car;
    public PrometeoCarController prometeoCarController;

    private GameObject currentRouteInstance;

    private List<Route> loadedRoutes;

    async void Start()
    {
        loadedRoutes = await RouteManager.Instance.GetAllRoutesForUserAsync();
    }

    void Update()
    {
        if (menuActive && routesDisplayed)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedIndex--;
                if (selectedIndex < 0)
                    selectedIndex = routeNames.Count - 1;

                UpdateMenuDisplay();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedIndex++;
                if (selectedIndex >= routeNames.Count)
                    selectedIndex = 0;

                UpdateMenuDisplay();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (routeNames.Count > 0)
                {
                    string selectedRouteName = routeNames[selectedIndex];
                    Route selectedRoute = loadedRoutes.Find(r => r.RouteName == selectedRouteName);
                    if (selectedRoute != null)
                    {
                        if (currentRouteInstance != null)
                            Destroy(currentRouteInstance);

                        StartCoroutine(FlashNotification());
                        ToggleMenu();

                        currentRouteInstance = new GameObject("Route_" + selectedRoute.RouteName);

                        // Create red line
                        GameObject lineObj = new GameObject("LineRenderer_" + selectedRoute.RouteName);
                        lineObj.transform.SetParent(currentRouteInstance.transform);

                        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                        lr.material = new Material(Shader.Find("Sprites/Default")); // Basic material
                        lr.material.color = Color.red;
                        lr.widthMultiplier = 0.2f;
                        lr.useWorldSpace = true;
                        lr.positionCount = selectedRoute.linePoints.Count;

                        for (int i = 0; i < selectedRoute.linePoints.Count; i++)
                        {
                            lr.SetPosition(i, selectedRoute.linePoints[i]);
                        }

                        // Recreate red cylinders for markers
                        for (int i = 0; i < selectedRoute.markerPositions.Count; i++)
                        {
                            Vector3 pos = selectedRoute.markerPositions[i];
                            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            marker.transform.position = pos;
                            marker.transform.localScale = new Vector3(1f, 0.1f, 1f);
                            marker.GetComponent<Renderer>().material.color = Color.red;
                            marker.transform.SetParent(currentRouteInstance.transform);

                            Collider markerCollider = marker.GetComponent<Collider>();
                            if (markerCollider != null)
                                markerCollider.isTrigger = true;

                            // Tag the first and last marker
                            if (i == 0)
                                marker.tag = "StartMarker";
                            else if (i == selectedRoute.markerPositions.Count - 1)
                                marker.tag = "EndMarker";
                        }

                        // Move car to start
                        if (selectedRoute.linePoints.Count >= 2 && car != null)
                        {
                            Vector3 start = selectedRoute.linePoints[0];
                            Vector3 next = selectedRoute.linePoints[1];
                            Vector3 direction = (next - start).normalized;

                            float spawnOffset = 10f;
                            Vector3 offsetPosition = start - direction * spawnOffset + Vector3.up * 2.5f;
                            car.position = offsetPosition;
                            prometeoCarController.resetRotation();

                            if (direction != Vector3.zero)
                                car.rotation = Quaternion.LookRotation(direction);
                        }

                        // start checker
                        RouteProximityChecker checker = car.GetComponent<RouteProximityChecker>();
                        if (checker != null)
                        {
                            checker.SetCurrentRoute(currentRouteInstance);
                            checker.enabled = false; // Disable checking until StartMarker is hit
                        }
                    }
                    else
                    {
                        Debug.LogError("Route data not found for: " + selectedRouteName);
                    }
                }
            }
        }
    }

    private IEnumerator FlashNotification()
    {
        notifPanel.SetActive(true);
        yield return new WaitForSeconds(1f);
        notifPanel.SetActive(false);
    }

    public void ToggleMenu()
    {
        menuActive = !menuActive;
        if (menuPanel != null)
        {
            menuPanel.SetActive(menuActive);
            DisplayRoutes();
        }

        if (!menuActive && menuText != null)
        {
            menuText.text = "";
            routesDisplayed = false;
        }
    }

    public void DisplayRoutes()
    {
        if (!menuActive) return;

        routeNames.Clear();

        if (loadedRoutes.Count == 0)
        {
            Debug.LogWarning("No saved routes found in persistentDataPath.");
            return;
        }

        foreach (Route route in loadedRoutes)
        {
            routeNames.Add(route.RouteName);
        }

        selectedIndex = 0;
        routesDisplayed = true;
        UpdateMenuDisplay();

        string debugInfo = "Loaded Routes:\n";
        foreach (string route in routeNames)
        {
            debugInfo += route + "\n";
        }
        Debug.Log(debugInfo);

        EventSystem.current.SetSelectedGameObject(null);
    }

    void UpdateMenuDisplay()
    {
        if (menuText == null)
            return;

        string displayText = "";
        for (int i = 0; i < routeNames.Count; i++)
        {
            if (i == selectedIndex)
                displayText += "-> " + routeNames[i] + "\n";
            else
                displayText += "   " + routeNames[i] + "\n";
        }

        menuText.text = displayText;
    }
}
