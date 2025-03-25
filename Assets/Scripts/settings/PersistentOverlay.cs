using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PersistentOverlay : MonoBehaviour
{
    public static PersistentOverlay instance;

    [Header("Overlay Image that dims the screen")]
    public Image brightnessOverlay;

    // A stored brightness value (0 to 1), so we can restore it if needed
    private float currentBrightness = 0.5f;

    private void Awake()
    {
        // Singleton pattern to avoid duplicates
        if (instance == null)
        {
            instance = this;
            SetBrightness(currentBrightness);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBrightness(float brightness)
    {
        currentBrightness = brightness;
        if (brightnessOverlay != null)
        {
            // For a slider range of 0 (dark) to 1 (bright):
            // alpha = 1 - brightness
            Color overlayColor = brightnessOverlay.color;
            overlayColor.a = 1f - brightness;
            brightnessOverlay.color = overlayColor;
        }
    }

    public float GetBrightness()
    {
        return currentBrightness;
    }

    public void BackButton()
    {
       SceneManager.LoadScene("menu");
    }
}
