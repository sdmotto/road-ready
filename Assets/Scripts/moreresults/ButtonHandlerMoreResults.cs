using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonHandlerMoreResults : MonoBehaviour
{
    [Header("Button References")]
    public Button menuButton;

    private void Start()
    {
        // Wire up the buttons at runtime
        if (menuButton != null)
            menuButton.onClick.AddListener(() => SceneManager.LoadScene("menu"));
    }
}
