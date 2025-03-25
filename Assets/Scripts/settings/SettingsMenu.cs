using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider brightnessSlider;

    private void Start()
    {
        brightnessSlider.value = PersistentOverlay.instance.GetBrightness();
    }

    public void OnBrightnessChanged(float value)
    {
        PersistentOverlay.instance.SetBrightness(value);
    }
}
