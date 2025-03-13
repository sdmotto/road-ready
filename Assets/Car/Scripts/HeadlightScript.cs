using UnityEngine;


public class HeadlightControl : MonoBehaviour {
    public Light headlightLeft;
    public Light headlightRight;
    public Light parkLightLeft;
    public Light parkLightRight;
    public Light brightLeft;
    public Light brightRight;

    public DayNightToggle dayNightToggle; // Reference to DayNightToggle script so we can turn headlights on automatically if on auto and night time




    void Update() {
        // Read button states
        bool offMode = Input.GetKey("joystick button 2");
        bool parkMode = Input.GetKey("joystick button 3");
        bool headlightMode = Input.GetKey("joystick button 4");
        bool flashMode = Input.GetKey("joystick button 5");
        //bool normalMode = Input.GetKey("joystick button 6");


        // Headlight logic
        if(offMode) {
            SetLights(false, false, false);
        }
        else if(headlightMode) {
            SetLights(true, false, false);
        }
        else if(parkMode) {
            SetLights(false, true, false);
        }
        else {
            bool isNight = dayNightToggle.isDay == false; // if it is night time set is night to true
            SetLights(isNight, false, false);
        }

        if(flashMode) {
            SetLights(false, false, true);
        }
        // if(!normalMode) {
        //     SetLights(false, false, true);
        // }



    }

    void SetLights(bool headlightsOn, bool parkLightsOn, bool brightsOn) {
        // Control left and right headlights
        headlightLeft.enabled = headlightsOn;
        headlightRight.enabled = headlightsOn;

        // Control left and right park lights
        parkLightLeft.enabled = parkLightsOn;
        parkLightRight.enabled = parkLightsOn;

        // Control left and right park lights
        brightLeft.enabled = brightsOn;
        brightRight.enabled = brightsOn;
    }
}