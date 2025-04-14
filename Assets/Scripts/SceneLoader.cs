using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneLoader : MonoBehaviour
{
    // public GameObject passwordPanel;
    // public TMP_InputField passwordInputField;

    // private string correctPassword;
    // private bool isPasswordSet = false;
    // private string passwordFilePath;

    // void Start()
    // {
    //     passwordFilePath = Path.Combine(Application.persistentDataPath, "password.txt");
    //     LoadPasswordFromFile();
    // }

    // void LoadPasswordFromFile()
    // {
    //     if (File.Exists(passwordFilePath))
    //     {
    //         correctPassword = File.ReadAllText(passwordFilePath).Trim();
    //         isPasswordSet = !string.IsNullOrEmpty(correctPassword);
    //         Debug.Log("Password loaded from: " + passwordFilePath);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Password file not found. A new one will be created on first entry.");
    //         isPasswordSet = false;
    //     }
    // }

    // public void ValidatePasswordInput()
    // {
    //     string input = passwordInputField.text.Trim();

    //     if (!isPasswordSet)
    //     {
    //         SavePassword(input);
    //         correctPassword = input;
    //         isPasswordSet = true;
    //         Debug.Log("Password set successfully.");
    //         LoadRoutesScene();
    //     }
    //     else if (input == correctPassword)
    //     {
    //         LoadRoutesScene();
    //     }
    //     else
    //     {
    //         Debug.Log("Incorrect password.");
    //         // You could add UI feedback here too
    //     }
    // }

    // void SavePassword(string newPassword)
    // {
    //     File.WriteAllText(passwordFilePath, newPassword);
    //     Debug.Log("Password saved to: " + passwordFilePath);
    // }

    // public void ShowPasswordPanel()
    // {
    //     passwordPanel.SetActive(true);
    // }

    // public void LoadWorldScene() => SceneManager.LoadScene("world");
    // public void LoadRoutesScene() => SceneManager.LoadScene("routes");
    // public void LoadMenuScene() => SceneManager.LoadScene("menu");
    // public void LoadSettingsScene() => SceneManager.LoadScene("settings");
    // public void LoadMoreResultsScene() => SceneManager.LoadScene("moreresults");
}
