using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonHandlerMenu : MonoBehaviour
{
    [Header("Button References")]
    public Button startButton;
    public Button parentalButton;
    public Button settingsButton;

    private void Start()
    {
        // Wire up the buttons at runtime
        if (startButton != null)
            startButton.onClick.AddListener(() => SceneManager.LoadScene("world"));

        if (parentalButton != null)
            parentalButton.onClick.AddListener(() => SceneManager.LoadScene("routes"));

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => SceneManager.LoadScene("settings"));
    }
}
