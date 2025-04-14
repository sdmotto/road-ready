using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogoutButtonHandler : MonoBehaviour
{
    public Button logoutButton;

    private void Start()
    {
        logoutButton.onClick.AddListener(Logout);
    }

    private async void Logout()
    {
        var client = SupabaseManager.Instance?.GetClient();
        if (client != null)
        {
            await client.Auth.SignOut();
            SupabaseManager.Instance.ForgetCredentials();
            Debug.Log("[Logout] User signed out.");
            SceneManager.Instance.LoadScene("auth");
        }
        else
        {
            Debug.LogWarning("[Logout] Supabase client not ready.");
        }
    }
}
