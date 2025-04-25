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
                Debug.Log("Key Pressed: " + key);
            }
        }

        // Detect joystick axis movements
        float xAxis = Input.GetAxis("Horizontal"); // Default joystick X-axis
        float yAxis = Input.GetAxis("Vertical");   // Default joystick Y-axis

        if (Mathf.Abs(xAxis) > 0.1f)
            Debug.Log("Joystick X Axis: " + xAxis);

        if (Mathf.Abs(yAxis) > 0.1f)
            Debug.Log("Joystick Y Axis: " + yAxis);
    }
}
