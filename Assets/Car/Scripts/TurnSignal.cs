using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TurnSignalController : MonoBehaviour
{
    public Image leftArrow;   // Drag Left Arrow UI Image
    public Image rightArrow;  // Drag Right Arrow UI Image
    private Coroutine leftCoroutine;
    private Coroutine rightCoroutine;

    private Vector3 startPosition;  // Store the car's initial position
    private Quaternion startRotation; // Store the car's initial rotation

    public static bool leftActive;
    public static bool rightActive;

    void Start()
    {
        // Store the car's starting position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Set arrows to be transparent but keep their original color
        SetAlpha(leftArrow, 0f);
        SetAlpha(rightArrow, 0f);
    }

    void Update()
    {
        leftActive = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.JoystickButton1);
        rightActive = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.JoystickButton0);

        if (leftActive && leftCoroutine == null)
            leftCoroutine = StartCoroutine(Blink(leftArrow));

        if (rightActive && rightCoroutine == null)
            rightCoroutine = StartCoroutine(Blink(rightArrow));

        if (!leftActive && leftCoroutine != null)
        {
            StopCoroutine(leftCoroutine);
            leftCoroutine = null;
            SetAlpha(leftArrow, 0f); // Hide left arrow
        }

        if (!rightActive && rightCoroutine != null)
        {
            StopCoroutine(rightCoroutine);
            rightCoroutine = null;
            SetAlpha(rightArrow, 0f); // Hide right arrow
        }

    }

    IEnumerator Blink(Image arrow)
    {
        while (true)
        {
            SetAlpha(arrow, 1f); // Fully visible
            yield return new WaitForSeconds(0.5f);
            SetAlpha(arrow, 0f); // Fully transparent
            yield return new WaitForSeconds(0.5f);
        }
    }

    void SetAlpha(Image img, float alpha)
    {
        Color color = img.color;
        color.a = alpha;  // Modify only the alpha channel
        img.color = color;
    }
}
