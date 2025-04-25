using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonHandlerResults : MonoBehaviour
{
    [Header("Button References")]
    public Button menuButton;
    public Button moreResultsButton;

    private void Start()
    {
        // Wire up the buttons at runtime
        if (menuButton != null)
            menuButton.onClick.AddListener(() => SceneManager.LoadScene("menu"));

        if (moreResultsButton != null)
            moreResultsButton.onClick.AddListener(() => SceneManager.LoadScene("moreresults"));
    }
}
