using System;
using System.Text.RegularExpressions;
using Google;
using Unity.Services.Authentication;
using Unity.Services.Core;
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
    void TryAutoSignIn(Action<SortingAuthSession, string> onComplete);
    void SignInGuest(Action<SortingAuthSession, string> onComplete);
    void SignInGoogle(Action<SortingAuthSession, string> onComplete);
    void LinkCurrentUserToGoogle(Action<SortingAuthSession, string> onComplete);
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

    public void TryAutoSignIn(Action<SortingAuthSession, string> onComplete)
    {
        TryAutoSignInAsync(onComplete);
    }

    private async void TryAutoSignInAsync(Action<SortingAuthSession, string> onComplete)
    {
        currentSession = LoadSession();
        if (currentSession == null || !currentSession.IsValid)
        {
            onComplete?.Invoke(null, "No cached session.");
            return;
        }

        try
        {
            await EnsureUnityServicesAsync();
            if (!AuthenticationService.Instance.IsSignedIn && currentSession.Provider == SortingAuthProvider.Google)
            {
                try
                {
                    string webClientId = ResolveGoogleWebClientId();
                    if (!string.IsNullOrEmpty(webClientId))
                    {
                        ConfigureGoogleSignIn(webClientId);
                    }

                    GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignInSilently();
                    if (googleUser != null && !string.IsNullOrEmpty(googleUser.IdToken))
                    {
                        await AuthenticationService.Instance.SignInWithGoogleAsync(googleUser.IdToken);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[Auth] Silent Google sign-in failed: " + exception.Message);
                }
            }

            if (!AuthenticationService.Instance.IsSignedIn && currentSession.Provider == SortingAuthProvider.Guest)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            string playerId = AuthenticationService.Instance.PlayerId;
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                onComplete?.Invoke(null, "Cached session expired.");
                return;
            }

            if (!string.IsNullOrEmpty(playerId) && playerId != currentSession.UserId)
            {
                SortingCloudSaveService.MigrateScopedKeys(currentSession.Provider.ToString(), currentSession.UserId, currentSession.Provider.ToString(), playerId);
                currentSession.UserId = playerId;
                SaveSession(currentSession);
            }

            await SortingCloudSaveService.MergeCloudToLocalAndSaveAsync();
            onComplete?.Invoke(currentSession, string.Empty);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Auth] Auto sign-in failed: " + exception.Message);
            onComplete?.Invoke(null, exception.Message);
        }
    }

    public void SignInGuest(Action<SortingAuthSession, string> onComplete)
    {
        SignInGuestAsync(onComplete);
    }

    private async void SignInGuestAsync(Action<SortingAuthSession, string> onComplete)
    {
        try
        {
            await EnsureUnityServicesAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            string userId = AuthenticationService.Instance.PlayerId;
            SortingAuthSession session = new SortingAuthSession
            {
                Provider = SortingAuthProvider.Guest,
                UserId = userId,
                DisplayName = "Guest",
            };

            SaveSession(session);
            await SortingCloudSaveService.MergeCloudToLocalAndSaveAsync();
            onComplete?.Invoke(session, string.Empty);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Auth] Unity guest sign-in failed: " + exception.Message);
            string userId = PlayerPrefs.GetString(UserIdKey, string.Empty);
            if (string.IsNullOrEmpty(userId) || PlayerPrefs.GetString(ProviderKey, string.Empty) != SortingAuthProvider.Guest.ToString())
            {
                userId = "guest_" + Guid.NewGuid().ToString("N");
            }

            SortingAuthSession session = new SortingAuthSession
            {
                Provider = SortingAuthProvider.Guest,
                UserId = userId,
                DisplayName = "Guest",
            };

            SaveSession(session);
            onComplete?.Invoke(session, string.Empty);
        }
    }

    public void SignInGoogle(Action<SortingAuthSession, string> onComplete)
    {
        SignInGoogleAsync(onComplete);
    }

    public void LinkCurrentUserToGoogle(Action<SortingAuthSession, string> onComplete)
    {
        LinkCurrentUserToGoogleAsync(onComplete);
    }

    private async void LinkCurrentUserToGoogleAsync(Action<SortingAuthSession, string> onComplete)
    {
        try
        {
            await EnsureUnityServicesAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            string webClientId = ResolveGoogleWebClientId();
            if (string.IsNullOrEmpty(webClientId))
            {
                onComplete?.Invoke(null, "Google web client id is missing in google-services.json.");
                return;
            }

            string previousProvider = PlayerPrefs.GetString(ProviderKey, string.Empty);
            string previousUserId = PlayerPrefs.GetString(UserIdKey, string.Empty);

            ConfigureGoogleSignIn(webClientId);
            Debug.Log("[Auth] Link Google sign-in started.");
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();
            if (googleUser == null || string.IsNullOrEmpty(googleUser.IdToken))
            {
                onComplete?.Invoke(null, "Google sign-in did not return an id token.");
                return;
            }

            try
            {
                await AuthenticationService.Instance.LinkWithGoogleAsync(googleUser.IdToken);
            }
            catch (AuthenticationException exception)
            {
                Debug.LogWarning("[Auth] Link with Google failed: " + exception.Message);
                await AuthenticationService.Instance.SignInWithGoogleAsync(googleUser.IdToken);
            }

            string playerId = AuthenticationService.Instance.PlayerId;
            if (string.IsNullOrEmpty(playerId))
            {
                onComplete?.Invoke(null, "Unity Authentication did not return a player id.");
                return;
            }

            if (!string.IsNullOrEmpty(previousProvider) && !string.IsNullOrEmpty(previousUserId))
            {
                SortingCloudSaveService.MigrateScopedKeys(previousProvider, previousUserId, SortingAuthProvider.Google.ToString(), playerId);
            }

            currentSession = new SortingAuthSession
            {
                Provider = SortingAuthProvider.Google,
                UserId = playerId,
                DisplayName = !string.IsNullOrEmpty(googleUser.DisplayName) ? googleUser.DisplayName : googleUser.Email,
            };

            SaveSession(currentSession);
            await SortingCloudSaveService.MergeCloudToLocalAndSaveAsync();
            Debug.Log("[Auth] Guest linked to Google.");
            onComplete?.Invoke(currentSession, string.Empty);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Auth] Link guest to Google failed: " + exception.Message);
            onComplete?.Invoke(null, exception.Message);
        }
    }

    private async void SignInGoogleAsync(Action<SortingAuthSession, string> onComplete)
    {
        try
        {
            await EnsureUnityServicesAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[Auth] Unity Services initialize failed: " + exception.Message);
            onComplete?.Invoke(null, "Unity services are not available.");
            return;
        }

        string webClientId = ResolveGoogleWebClientId();
        if (string.IsNullOrEmpty(webClientId))
        {
            Debug.LogWarning("[Auth] Google web client id is missing.");
            onComplete?.Invoke(null, "Google web client id is missing in google-services.json.");
            return;
        }

        try
        {
            ConfigureGoogleSignIn(webClientId);
            Debug.Log("[Auth] Google sign-in started.");
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();

            Debug.Log("[Auth] Google sign-in returned.");
            if (googleUser == null || string.IsNullOrEmpty(googleUser.IdToken))
            {
                Debug.LogWarning("[Auth] Google sign-in returned without id token.");
                onComplete?.Invoke(null, "Google sign-in did not return an id token.");
                return;
            }

            string previousProvider = PlayerPrefs.GetString(ProviderKey, string.Empty);
            string previousUserId = PlayerPrefs.GetString(UserIdKey, string.Empty);
            string googleUserId = !string.IsNullOrEmpty(googleUser.UserId) ? googleUser.UserId : googleUser.Email;

            Debug.Log("[Auth] Unity Google sign-in started.");
            await AuthenticationService.Instance.SignInWithGoogleAsync(googleUser.IdToken);
            string unityPlayerId = AuthenticationService.Instance.PlayerId;
            if (string.IsNullOrEmpty(unityPlayerId))
            {
                onComplete?.Invoke(null, "Unity Authentication did not return a player id.");
                return;
            }

            string displayName = !string.IsNullOrEmpty(googleUser.DisplayName)
                ? googleUser.DisplayName
                : googleUser.Email;

            currentSession = new SortingAuthSession
            {
                Provider = SortingAuthProvider.Google,
                UserId = unityPlayerId,
                DisplayName = displayName,
            };

            if (!string.IsNullOrEmpty(googleUserId))
            {
                SortingCloudSaveService.MigrateScopedKeys(SortingAuthProvider.Google.ToString(), googleUserId, SortingAuthProvider.Google.ToString(), unityPlayerId);
            }

            if (!string.IsNullOrEmpty(previousProvider) && !string.IsNullOrEmpty(previousUserId))
            {
                SortingCloudSaveService.MigrateScopedKeys(previousProvider, previousUserId, SortingAuthProvider.Google.ToString(), unityPlayerId);
            }

            SaveSession(currentSession);
            await SortingCloudSaveService.MergeCloudToLocalAndSaveAsync();
            Debug.Log("[Auth] Google session saved.");
            onComplete?.Invoke(currentSession, string.Empty);
        }
        catch (Exception exception)
        {
            Debug.LogError("[Auth] Google sign-in start failed: " + exception);
            onComplete?.Invoke(null, exception.Message);
        }
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

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    AuthenticationService.Instance.SignOut();
                }
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

    private static async System.Threading.Tasks.Task EnsureUnityServicesAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }
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
