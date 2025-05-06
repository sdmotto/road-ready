// This was used for initial testing when implenting the turn signal stalk, but it is not used in the actual game

using UnityEngine;

public class USBInputDebugger : MonoBehaviour
{
    void Update()
    {
        // Detect all keypresses
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                // Debug.Log("Key Pressed: " + key);
            }
        }

        // Detect joystick axis movements
        float xAxis = Input.GetAxis("Horizontal"); // Default joystick X-axis
        float yAxis = Input.GetAxis("Vertical");   // Default joystick Y-axis
    }
}
