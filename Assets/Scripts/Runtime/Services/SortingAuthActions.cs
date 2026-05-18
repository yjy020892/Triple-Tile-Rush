using Google;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SortingAuthActions
{
    private const string BootSceneName = "Boot";

    public static void LinkGuestToGoogleAndRestart()
    {
        ISortingAuthService authService = SortingSdkBootstrapper.CreateAuthService();
        authService.LinkCurrentUserToGoogle((session, error) =>
        {
            if (session == null || !session.IsValid)
            {
                Debug.LogWarning("[Auth] Link guest to Google failed: " + (string.IsNullOrEmpty(error) ? "unknown" : error));
                return;
            }

            SceneManager.LoadScene(BootSceneName, LoadSceneMode.Single);
        });
    }

    public static void SignOutAndRestart()
    {
        try
        {
            if (GoogleSignIn.Configuration != null)
            {
                GoogleSignIn.DefaultInstance.SignOut();
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("[Auth] Google sign-out skipped: " + exception.Message);
        }

        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true);
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("[Auth] Unity sign-out skipped: " + exception.Message);
        }

        PlayerPrefs.DeleteKey(SortingPlayerPrefsAuthService.ProviderKey);
        PlayerPrefs.DeleteKey(SortingPlayerPrefsAuthService.UserIdKey);
        PlayerPrefs.DeleteKey(SortingPlayerPrefsAuthService.DisplayNameKey);
        PlayerPrefs.Save();
        SceneManager.LoadScene(BootSceneName, LoadSceneMode.Single);
    }
}
