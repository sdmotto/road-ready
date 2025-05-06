using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// script for settings scene functionality
public class SettingsMenu : MonoBehaviour
{
    public Slider brightnessSlider;

    public Slider volumeSlider;

    public AudioClip backButtonClip;
    private AudioSource audioSource;

    // initialize slider values
    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        brightnessSlider.value = PersistentOverlay.instance.GetBrightness();
        volumeSlider.value = PersistentOverlay.instance.GetVolume();
    }

    // set brightness when changesd
    public void OnBrightnessChanged(float value)
    {
        PersistentOverlay.instance.SetBrightness(value);
    }

    // set volume when changed
    public void OnVolumeChanged(float value)
    {
        PersistentOverlay.instance.SetVolume(value);
    }

    // play test audio to check sound level
    public void PlayTestAudio() {
        audioSource.PlayOneShot(backButtonClip);
    }
}
