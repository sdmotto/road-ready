using System;
using System.IO;
using System.Threading.Tasks;
using Supabase;
using Supabase.Gotrue;
using UnityEngine;

public class SupabaseManager : MonoBehaviour
{
    public static SupabaseManager Instance;

    private static string SupabaseUrl = EnvManager.Get("SUPABASE_URL");
    private static string SupabaseAnonKey = EnvManager.Get("SUPABASE_KEY");

    private const string CredsFile = "supabase_anon_credentials.json";
    public static bool IsAuthenticated => Instance.GetClient()?.Auth.CurrentUser != null;

    private Supabase.Client supabaseClient;

    private class LocalCreds
    {
        public string Email;
        public string Password;
    }

    async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeSupabase();
    }

    private async Task InitializeSupabase()
    {
        supabaseClient = new Supabase.Client(SupabaseUrl, SupabaseAnonKey, new SupabaseOptions
        {
            AutoRefreshToken = true
        });

        await supabaseClient.InitializeAsync();

        string path = Path.Combine(Application.persistentDataPath, CredsFile);
        if (File.Exists(path))
        {
            var creds = JsonUtility.FromJson<LocalCreds>(File.ReadAllText(path));
            bool success = await SignInOrSignUpAsync(creds.Email, creds.Password, remember: true);

            if (success)
            {
                Debug.Log($"[SupabaseManager] Auto-signed in as {supabaseClient.Auth.CurrentUser?.Email}");
                return;
            }

            Debug.LogWarning("[SupabaseManager] Failed to sign in with stored credentials.");
        }

        // If no creds or login failed, go to auth scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("auth");
    }

    public async Task<bool> SignInOrSignUpAsync(string email, string password, bool remember = true)
    {
        email = email.Trim().ToLower();
        password = password.Trim();

        try
        {
            var session = await supabaseClient.Auth.SignIn(email, password);
            Debug.Log($"[SupabaseManager] Signed in as: {session.User.Email}");

            if (remember)
                SaveCredentials(email, password);

            return true;
        }
        catch (Exception signInEx)
        {
            Debug.LogWarning($"[SupabaseManager] Sign-in failed: {signInEx.Message}");

            try
            {
                var session = await supabaseClient.Auth.SignUp(email, password);
                Debug.Log($"[SupabaseManager] Signed up new user: {session.User.Email}");

                if (remember)
                    SaveCredentials(email, password);

                return true;
            }
            catch (Exception signUpEx)
            {
                Debug.LogError($"[SupabaseManager] Sign-up failed: {signUpEx.Message}");
                return false;
            }
        }
    }

    public void ForgetCredentials()
    {
        string path = Path.Combine(Application.persistentDataPath, CredsFile);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[SupabaseManager] Credentials cleared.");
        }
    }

    private void SaveCredentials(string email, string password)
    {
        var creds = new LocalCreds { Email = email, Password = password };
        string path = Path.Combine(Application.persistentDataPath, CredsFile);
        File.WriteAllText(path, JsonUtility.ToJson(creds));
        Debug.Log("[SupabaseManager] Credentials saved.");
    }

    public Supabase.Client GetClient() => supabaseClient;
    public string UserId => supabaseClient.Auth.CurrentUser?.Id;
}
