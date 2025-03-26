using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider brightnessSlider;

    public Slider volumeSlider;

    public AudioClip backButtonClip;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        brightnessSlider.value = PersistentOverlay.instance.GetBrightness();
        volumeSlider.value = PersistentOverlay.instance.GetVolume();
    }

    public void OnBrightnessChanged(float value)
    {
        PersistentOverlay.instance.SetBrightness(value);
    }

    public void OnVolumeChanged(float value)
    {
        PersistentOverlay.instance.SetVolume(value);
    }

    public void BackToMenu() {
        SceneManager.LoadScene("menu");
    }

    public void PlayTestAudio() {
        audioSource.PlayOneShot(backButtonClip);
    }
}
