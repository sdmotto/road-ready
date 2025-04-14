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
    private const string SessionFile = "supabase_session.json";

    private Supabase.Client supabaseClient;

    private class LocalCreds
    {
        public string Email;
        public string Password;
    }

    [Serializable]
    public class SerializableSession
    {
        public string access_token;
        public string refresh_token;

        public SerializableSession() { }

        public SerializableSession(Session session)
        {
            access_token = session.AccessToken;
            refresh_token = session.RefreshToken;
        }

        public Session ToSession()
        {
            return new Session
            {
                AccessToken = access_token,
                RefreshToken = refresh_token
            };
        }
    }

    public string UserId => supabaseClient.Auth.CurrentUser?.Id;

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

        Session saved = LoadSession();
        if (saved != null)
        {
            await supabaseClient.Auth.SetSession(saved.AccessToken, saved.RefreshToken);
        }

        if (supabaseClient.Auth.CurrentSession == null)
        {
            await SignInOrCreateAnonymousUser();
        }
        else
        {
            Debug.Log($"Session restored: {UserId}");
        }
    }

    private async Task SignInOrCreateAnonymousUser()
    {
        string path = Path.Combine(Application.persistentDataPath, CredsFile);
        LocalCreds creds;

        if (File.Exists(path))
        {
            creds = JsonUtility.FromJson<LocalCreds>(File.ReadAllText(path));
        }
        else
        {
            creds = new LocalCreds
            {
                Email = $"{Guid.NewGuid()}@anon.supabase",
                Password = Guid.NewGuid().ToString()
            };
            File.WriteAllText(path, JsonUtility.ToJson(creds));
        }

        try
        {
            var session = await supabaseClient.Auth.SignIn(creds.Email, creds.Password);
            Debug.Log($"Signed in anonymously: {session.User.Id}");
            SaveSession(session);
        }
        catch
        {
            try
            {
                var session = await supabaseClient.Auth.SignUp(creds.Email, creds.Password);
                Debug.Log($"Anonymous user created: {session.User.Id}");
                SaveSession(session);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Anonymous sign-up failed: {ex.Message}");
            }
        }
    }

    private void SaveSession(Session session)
    {
        string path = Path.Combine(Application.persistentDataPath, SessionFile);
        File.WriteAllText(path, JsonUtility.ToJson(new SerializableSession(session)));
    }

    private Session LoadSession()
    {
        string path = Path.Combine(Application.persistentDataPath, SessionFile);
        if (!File.Exists(path)) return null;

        return JsonUtility.FromJson<SerializableSession>(File.ReadAllText(path)).ToSession();
    }

    public Supabase.Client GetClient() => supabaseClient;
}
