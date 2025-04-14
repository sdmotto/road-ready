using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonHandlerSettings : MonoBehaviour
{
    [Header("Button References")]
    public Button backButton;

    private void Start()
    {
        // Wire up the buttons at runtime
        if (backButton != null)
            backButton.onClick.AddListener(() => SceneManager.LoadScene("menu"));
    }
}
