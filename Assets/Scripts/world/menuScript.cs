using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class menuScript : MonoBehaviour
{
    [Header("UI References")]
    // Assign the UI Panel that will serve as your drop‑down menu.
    public GameObject menuPanel;
    // Assign the UI Text element (using TextMeshPro) that will display the menu options.
    public TMP_Text menuText;

    private bool menuActive = false;
    private bool routesDisplayed = false;

    // List of route names (extracted from the Routes folder).
    private List<string> routeNames = new List<string>();
    // Tracks which route is currently selected.
    private int selectedIndex = 0;

    void Update()
    {

        // If the menu is open and the routes are displayed, use the arrow keys and Enter for navigation.
        if (menuActive && routesDisplayed)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // Move selection up.
                selectedIndex--;
                if (selectedIndex < 0)
                {
                    selectedIndex = routeNames.Count - 1;
                }
                UpdateMenuDisplay();
                Debug.Log("Up Arrow pressed. Selected index: " + selectedIndex);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Move selection down.
                selectedIndex++;
                if (selectedIndex >= routeNames.Count)
                {
                    selectedIndex = 0;
                }
                UpdateMenuDisplay();
                Debug.Log("Down Arrow pressed. Selected index: " + selectedIndex);
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (routeNames.Count > 0)
                {
                    string selectedRouteName = routeNames[selectedIndex];
                    Debug.Log("Enter pressed. Route selected: " + selectedRouteName);
                    
                    // Load the prefab from the Resources/Routes folder.
                    GameObject routePrefab = Resources.Load<GameObject>("Routes/" + selectedRouteName);
                    if (routePrefab != null)
                    {
                        // Instantiate the prefab into the world.
                        // Adjust the spawn position and rotation as needed.
                        Instantiate(routePrefab, Vector3.zero, Quaternion.identity);
                    }
                    else
                    {
                        Debug.LogError("Could not load route prefab: " + selectedRouteName);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Toggles the visibility of the menu panel.
    /// </summary>
    public void ToggleMenu()
    {
        menuActive = !menuActive;
        if (menuPanel != null)
        {
            menuPanel.SetActive(menuActive);
        }

        // When closing the menu, clear any displayed text.
        if (!menuActive && menuText != null)
        {
            menuText.text = "";
            routesDisplayed = false;
        }
    }

    /// <summary>
    /// Reads the Routes folder and displays the list of route prefab names.
    /// </summary>
    public void DisplayRoutes()
    {
        if(menuActive)
        {
            // Load all route prefabs from the Resources/Routes folder.
            GameObject[] routePrefabs = Resources.LoadAll<GameObject>("Routes");

            // Clear any existing route names.
            routeNames.Clear();

            // Loop through each loaded prefab and add its name to the list.
            foreach (GameObject routePrefab in routePrefabs)
            {
                routeNames.Add(routePrefab.name);
            }

            // Check if any routes were found.
            if (routeNames.Count == 0)
            {
                Debug.LogWarning("No route prefabs found in Resources/Routes folder.");
                return;
            }

            // Reset the selected index and mark that routes are displayed.
            selectedIndex = 0;
            routesDisplayed = true;
            UpdateMenuDisplay();

            // Optionally, print all available routes to the console.
            string debugInfo = "Available Routes:\n";
            foreach (string route in routeNames)
            {
                debugInfo += route + "\n";
            }
            Debug.Log(debugInfo);
        }
        
    }

    /// <summary>
    /// Updates the menu text display to show available routes and an arrow next to the currently selected one.
    /// </summary>
    void UpdateMenuDisplay()
    {
        if (menuText == null)
            return;

        string displayText = "Available Routes:\n\n";
        for (int i = 0; i < routeNames.Count; i++)
        {
            if (i == selectedIndex)
            {
                // Add an arrow (or any visual indicator) before the selected route.
                displayText += "-> " + routeNames[i] + "\n";
            }
            else
            {
                displayText += "   " + routeNames[i] + "\n";
            }
        }
        menuText.text = displayText;
    }
}
