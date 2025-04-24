using UnityEngine;

public class DayNightToggle : MonoBehaviour
{
    public Light directionalLight;
    public Material SkyNoon;
    public Material SkyNight;
    public Color dayColor = new Color(255f, 244f, 214f); // Warm daylight color
    public Color nightColor = new Color(0.1f, 0.1f, 0.2f); // Dark blue night color
    public float dayIntensity = 1.0f;
    public float nightIntensity = 0.2f;
    
    public bool isDay = true; // public so can be accessed elsewhere

    void Start()
    {
        if (directionalLight == null)
        {
            directionalLight = GetComponent<Light>();
        }
        
        // Set initial skybox
        RenderSettings.skybox = SkyNoon;
        DynamicGI.UpdateEnvironment(); // Update lighting to reflect the skybox change
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown("joystick button 9"))
        {
            ToggleDayNight();
        }
    }

    void ToggleDayNight()
    {
        isDay = !isDay;

        if (isDay)
        {
            directionalLight.color = dayColor;
            directionalLight.intensity = dayIntensity;
            RenderSettings.skybox = SkyNoon;
        }
        else
        {
            directionalLight.color = nightColor;
            directionalLight.intensity = nightIntensity;
            RenderSettings.skybox = SkyNight;
        }

        DynamicGI.UpdateEnvironment(); // Refreshes global illumination to match the new skybox
    }
}
