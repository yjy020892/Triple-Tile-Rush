using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using UnityEngine;

public enum SortingAuthProvider
{
    None = 0,
    Guest = 1,
    Google = 2,
}

public sealed class SortingAuthSession
{
    public SortingAuthProvider Provider;
    public string UserId;
    public string DisplayName;

    public bool IsValid => Provider != SortingAuthProvider.None && !string.IsNullOrEmpty(UserId);
}

public interface ISortingAuthService
{
    bool HasCachedSession { get; }
    SortingAuthSession CurrentSession { get; }
    void Initialize(Action<bool, string> onComplete);
    void SignInGuest(Action<SortingAuthSession, string> onComplete);
    void SignInGoogle(Action<SortingAuthSession, string> onComplete);
    void SignOut();
}

public sealed class SortingPlayerPrefsAuthService : ISortingAuthService
{
    public const string ProviderKey = "SortingPuzzle_AuthProvider";
    public const string UserIdKey = "SortingPuzzle_AuthUserId";
    public const string DisplayNameKey = "SortingPuzzle_AuthDisplayName";
    private const string EmbeddedGoogleWebClientId = "132946506425-fhnurf41p4t6sfj26prkafmp70b8h34f.apps.googleusercontent.com";

    private SortingAuthSession currentSession;

    public bool HasCachedSession => CurrentSession != null && CurrentSession.IsValid;

    public SortingAuthSession CurrentSession
    {
        get
        {
            if (currentSession == null)
            {
                currentSession = LoadSession();
            }

            return currentSession;
        }
    }

    public void Initialize(Action<bool, string> onComplete)
    {
        currentSession = LoadSession();
        onComplete?.Invoke(true, string.Empty);
    }

    public void SignInGuest(Action<SortingAuthSession, string> onComplete)
    {
        string userId = PlayerPrefs.GetString(UserIdKey, string.Empty);
        if (string.IsNullOrEmpty(userId) || PlayerPrefs.GetString(ProviderKey, string.Empty) != SortingAuthProvider.Guest.ToString())
        {
            userId = "guest_" + Guid.NewGuid().ToString("N");
        }

        currentSession = new SortingAuthSession
        {
            Provider = SortingAuthProvider.Guest,
            UserId = userId,
            DisplayName = "Guest",
        };
        SaveSession(currentSession);
        onComplete?.Invoke(currentSession, string.Empty);
    }

    public void SignInGoogle(Action<SortingAuthSession, string> onComplete)
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(dependencyTask =>
        {
            if (dependencyTask.IsCanceled || dependencyTask.IsFaulted)
            {
                onComplete?.Invoke(null, GetTaskError(dependencyTask.Exception, "Firebase dependency check failed."));
                return;
            }

            if (dependencyTask.Result != DependencyStatus.Available)
            {
                onComplete?.Invoke(null, "Firebase dependencies are not available: " + dependencyTask.Result);
                return;
            }

            string webClientId = ResolveGoogleWebClientId();
            if (string.IsNullOrEmpty(webClientId))
            {
                onComplete?.Invoke(null, "Google web client id is missing in google-services.json.");
                return;
            }

            ConfigureGoogleSignIn(webClientId);
            GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(googleTask =>
            {
                if (googleTask.IsCanceled || googleTask.IsFaulted)
                {
                    onComplete?.Invoke(null, GetTaskError(googleTask.Exception, "Google sign-in was canceled or failed."));
                    return;
                }

                GoogleSignInUser googleUser = googleTask.Result;
                if (googleUser == null || string.IsNullOrEmpty(googleUser.IdToken))
                {
                    onComplete?.Invoke(null, "Google sign-in did not return an id token.");
                    return;
                }

                Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
                FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
                {
                    if (authTask.IsCanceled || authTask.IsFaulted)
                    {
                        onComplete?.Invoke(null, GetTaskError(authTask.Exception, "Firebase Google sign-in failed."));
                        return;
                    }

                    FirebaseUser firebaseUser = ExtractFirebaseUser(authTask.Result);
                    if (firebaseUser == null)
                    {
                        onComplete?.Invoke(null, "Firebase Google sign-in did not return a user.");
                        return;
                    }

                    string displayName = !string.IsNullOrEmpty(firebaseUser.DisplayName)
                        ? firebaseUser.DisplayName
                        : (!string.IsNullOrEmpty(googleUser.DisplayName) ? googleUser.DisplayName : googleUser.Email);

                    currentSession = new SortingAuthSession
                    {
                        Provider = SortingAuthProvider.Google,
                        UserId = !string.IsNullOrEmpty(firebaseUser.UserId) ? firebaseUser.UserId : googleUser.UserId,
                        DisplayName = displayName,
                    };

                    SaveSession(currentSession);
                    onComplete?.Invoke(currentSession, string.Empty);
                });
            });
        });
    }

    public void SignOut()
    {
        if (currentSession != null && currentSession.Provider == SortingAuthProvider.Google)
        {
            try
            {
                if (GoogleSignIn.Configuration != null)
                {
                    GoogleSignIn.DefaultInstance.SignOut();
                }

                FirebaseAuth.DefaultInstance.SignOut();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Auth] Google sign-out failed: " + exception.Message);
            }
        }

        currentSession = null;
        PlayerPrefs.DeleteKey(ProviderKey);
        PlayerPrefs.DeleteKey(UserIdKey);
        PlayerPrefs.DeleteKey(DisplayNameKey);
        PlayerPrefs.Save();
    }

    private static void ConfigureGoogleSignIn(string webClientId)
    {
        GoogleSignInConfiguration configuration = GoogleSignIn.Configuration;
        if (configuration == null)
        {
            configuration = new GoogleSignInConfiguration();
            GoogleSignIn.Configuration = configuration;
        }

        configuration.UseGameSignIn = false;
        configuration.WebClientId = webClientId;
        configuration.RequestEmail = true;
        configuration.RequestProfile = true;
        configuration.RequestIdToken = true;
    }

    private static string ResolveGoogleWebClientId()
    {
        if (!string.IsNullOrEmpty(EmbeddedGoogleWebClientId))
        {
            return EmbeddedGoogleWebClientId;
        }

        TextAsset googleServices = Resources.Load<TextAsset>("google-services");
        if (googleServices != null)
        {
            string clientId = ExtractWebClientId(googleServices.text);
            if (!string.IsNullOrEmpty(clientId))
            {
                return clientId;
            }
        }

        return string.Empty;
    }

    private static string ExtractWebClientId(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return string.Empty;
        }

        Match match = Regex.Match(json, "\"client_id\"\\s*:\\s*\"([^\"]+)\"\\s*,\\s*\"client_type\"\\s*:\\s*3");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static FirebaseUser ExtractFirebaseUser(object result)
    {
        if (result is FirebaseUser user)
        {
            return user;
        }

        PropertyInfo userProperty = result?.GetType().GetProperty("User", BindingFlags.Instance | BindingFlags.Public);
        return userProperty?.GetValue(result) as FirebaseUser;
    }

    private static string GetTaskError(Exception exception, string fallback)
    {
        Exception baseException = exception?.GetBaseException();
        return baseException != null && !string.IsNullOrEmpty(baseException.Message)
            ? baseException.Message
            : fallback;
    }

    private static SortingAuthSession LoadSession()
    {
        string providerText = PlayerPrefs.GetString(ProviderKey, string.Empty);
        if (!Enum.TryParse(providerText, out SortingAuthProvider provider))
        {
            provider = SortingAuthProvider.None;
        }

        return new SortingAuthSession
        {
            Provider = provider,
            UserId = PlayerPrefs.GetString(UserIdKey, string.Empty),
            DisplayName = PlayerPrefs.GetString(DisplayNameKey, string.Empty),
        };
    }

    private static void SaveSession(SortingAuthSession session)
    {
        if (session == null || !session.IsValid)
        {
            return;
        }

        PlayerPrefs.SetString(ProviderKey, session.Provider.ToString());
        PlayerPrefs.SetString(UserIdKey, session.UserId);
        PlayerPrefs.SetString(DisplayNameKey, session.DisplayName ?? string.Empty);
        PlayerPrefs.Save();
    }
}

public static class SortingAuthProfileKeys
{
    public static string Scoped(string key)
    {
        string provider = PlayerPrefs.GetString(SortingPlayerPrefsAuthService.ProviderKey, SortingAuthProvider.Guest.ToString());
        string userId = PlayerPrefs.GetString(SortingPlayerPrefsAuthService.UserIdKey, "local_guest");
        if (string.IsNullOrEmpty(userId))
        {
            userId = "local_guest";
        }

        return string.Concat(key, "_", provider, "_", userId);
    }
}
