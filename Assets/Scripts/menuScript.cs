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
    // Assign the UI Text element that will display the menu options.
    public TMP_Text menuText;

    private bool menuActive = false;
    private bool routesDisplayed = false;
    
    void Update()
    {
        // Toggle the menu visibility when M is pressed.
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMenu();
        }
        
        // When the menu is active and R is pressed, display the list of routes.
        if (menuActive && Input.GetKeyDown(KeyCode.R))
        {
            DisplayRoutes();
        }
        
        // If the menu is open and the routes are displayed,
        // use the arrow keys and Enter for navigation (demo purpose).
        if (menuActive && routesDisplayed)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("Up Arrow pressed in Routes menu.");
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Debug.Log("Down Arrow pressed in Routes menu.");
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Enter pressed. Route selected (dummy selection).");
            }
        }
    }
    
    /// <summary>
    /// Toggles the visibility of the menu panel.
    /// </summary>
    void ToggleMenu()
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
    void DisplayRoutes()
    {
        // Get the full path to the "Assets/Routes" folder.
        string routesFolderPath = Path.Combine(Application.dataPath, "Routes");
        
        if (Directory.Exists(routesFolderPath))
        {
            // Look for files matching the routeMap pattern.
            string[] files = Directory.GetFiles(routesFolderPath, "routeMap*.prefab");
            string routesInfo = "Available Routes:\n";
            
            foreach (string file in files)
            {
                // Extract and append the file name (without extension).
                routesInfo += Path.GetFileNameWithoutExtension(file) + "\n";
            }
            
            if (menuText != null)
            {
                menuText.text = routesInfo;
            }
            
            Debug.Log(routesInfo);
            routesDisplayed = true;
        }
        else
        {
            Debug.LogWarning("Routes folder not found at: " + routesFolderPath);
        }
    }
}
