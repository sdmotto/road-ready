using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class ButtonHandlerMenu : MonoBehaviour
{
    [Header("Button References")]
    public Button startButton;
    public Button parentalButton;
    public Button settingsButton;

    [Header("Parental Control Panel")]
    public GameObject parentalPanel;
    public TMP_InputField passwordInput;
    public Button goButton;

    public Button exitButton;

    private string passwordFilePath;
    private string savedPassword;

    private void Start()
    {
        passwordFilePath = Path.Combine(Application.persistentDataPath, "parental_pass.txt");

        // Try to load saved password
        if (File.Exists(passwordFilePath))
        {
            savedPassword = File.ReadAllText(passwordFilePath).Trim();
        }

        // Wire up the buttons at runtime
        if (startButton != null)
            startButton.onClick.AddListener(() => SceneManager.LoadScene("world"));

        if (parentalButton != null)
            parentalButton.onClick.AddListener(ShowParentalPanel);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => SceneManager.LoadScene("settings"));

        if (goButton != null)
            goButton.onClick.AddListener(ValidateParentalPassword);

        if (exitButton != null)
            exitButton.onClick.AddListener(QuitApp);
    }

    private void ShowParentalPanel()
    {
        if (parentalPanel != null)
            parentalPanel.SetActive(true);
    }

    private void ValidateParentalPassword()
    {
        if (passwordInput == null || string.IsNullOrEmpty(passwordInput.text))
        {
            Debug.LogWarning("No password entered.");
            return;
        }

        string entered = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(savedPassword))
        {
            // first time — save the password
            File.WriteAllText(passwordFilePath, entered);
            savedPassword = entered;
            Debug.Log("Password saved.");
            SceneManager.LoadScene("routes");
        }
        else if (entered == savedPassword)
        {
            SceneManager.LoadScene("routes");
        }
        else
        {
            Debug.LogWarning("Incorrect password.");
        }
    }

    private void QuitApp()
    {
        Debug.Log("Quitting application...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
