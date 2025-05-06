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
        loginButton.onClick.AddListener(OnLoginClicked); // listener for login button
        signupButton.onClick.AddListener(OnSignupClicked); // listener for signup button
    }

    private async void OnLoginClicked()
    {
        string email = loginEmailInput.text.Trim();  // get email input
        string password = loginPasswordInput.text.Trim(); //get password input
        bool remember = rememberMeToggle == null || rememberMeToggle.isOn; // defualt to true if toggle is missing 

        // check if email or password is empty
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Login: Email or password is empty.");
            return;
        }

        // call supabase login method
        bool success = await SupabaseManager.Instance.SignInOrSignUpAsync(email, password, remember);
        if (success)
        {
            SceneManager.LoadScene("menu"); // load menu if email and password are correct
        }
        else
        {
            Debug.LogError("Login failed.");
        }
    }

    private async void OnSignupClicked()
    {
        string email = signupEmailInput.text.Trim(); // get email input
        string password = signupPasswordInput.text.Trim(); // get password input
        bool remember = rememberMeToggle == null || rememberMeToggle.isOn; // defualt to true if toggle is missing 

        // check if email or password is empty
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Signup: Email or password is empty.");
            return;
        }

        // call supabase login method
        bool success = await SupabaseManager.Instance.SignInOrSignUpAsync(email, password, remember);
        if (success)
        {
            SceneManager.LoadScene("menu"); // load menu if email and password are correct
        }
        else
        {
            Debug.LogError("Signup failed.");
        }
    }
}
