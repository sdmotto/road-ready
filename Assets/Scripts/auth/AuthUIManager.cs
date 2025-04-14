using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthUIManager : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;

    [Header("Signup UI")]
    public TMP_InputField signupEmailInput;
    public TMP_InputField signupPasswordInput;
    public Button signupButton;

    [Header("Optional")]
    public Toggle rememberMeToggle;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        signupButton.onClick.AddListener(OnSignupClicked);
    }

    private async void OnLoginClicked()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text.Trim();
        bool remember = rememberMeToggle == null || rememberMeToggle.isOn;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Login: Email or password is empty.");
            return;
        }

        bool success = await SupabaseManager.Instance.SignInOrSignUpAsync(email, password, remember);
        if (success)
        {
            SceneManager.LoadScene("menu");
        }
        else
        {
            Debug.LogError("Login failed.");
        }
    }

    private async void OnSignupClicked()
    {
        string email = signupEmailInput.text.Trim();
        string password = signupPasswordInput.text.Trim();
        bool remember = rememberMeToggle == null || rememberMeToggle.isOn;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Signup: Email or password is empty.");
            return;
        }

        bool success = await SupabaseManager.Instance.SignInOrSignUpAsync(email, password, remember);
        if (success)
        {
            SceneManager.LoadScene("menu");
        }
        else
        {
            Debug.LogError("Signup failed.");
        }
    }
}
