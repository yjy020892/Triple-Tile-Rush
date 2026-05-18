using System.Collections;
using UnityEngine;

#if SORTING_PLAY_APP_UPDATE && UNITY_ANDROID && !UNITY_EDITOR
using Google.Play.AppUpdate;
using Google.Play.Common;
#endif

public interface ISortingUpdateService
{
    IEnumerator CheckForUpdate(System.Action<SortingUpdateCheckResult> onComplete);
}

public sealed class SortingUpdateCheckResult
{
    public bool CanContinue;
    public bool UpdateRequired;
    public string Message;

    public static SortingUpdateCheckResult Continue(string message = "")
    {
        return new SortingUpdateCheckResult
        {
            CanContinue = true,
            UpdateRequired = false,
            Message = message ?? string.Empty,
        };
    }

    public static SortingUpdateCheckResult Block(string message)
    {
        return new SortingUpdateCheckResult
        {
            CanContinue = false,
            UpdateRequired = true,
            Message = message ?? "Update required.",
        };
    }
}

public sealed class SortingPlayUpdateService : ISortingUpdateService
{
    private const float CheckTimeoutSeconds = 3f;
    private const float UpdateFlowTimeoutSeconds = 120f;

    public IEnumerator CheckForUpdate(System.Action<SortingUpdateCheckResult> onComplete)
    {
#if SORTING_PLAY_APP_UPDATE && UNITY_ANDROID && !UNITY_EDITOR
        AppUpdateManager appUpdateManager = new AppUpdateManager();
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> infoOperation = appUpdateManager.GetAppUpdateInfo();
        yield return WaitForPlayOperation(infoOperation, CheckTimeoutSeconds);

        if (!infoOperation.IsDone)
        {
            Debug.LogWarning("[Update] Check timed out. Continuing startup.");
            onComplete?.Invoke(SortingUpdateCheckResult.Continue("Update check timed out."));
            yield break;
        }

        if (!infoOperation.IsSuccessful)
        {
            Debug.LogWarning("[Update] Check failed: " + infoOperation.Error);
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update check failed. Please try again."));
            yield break;
        }

        AppUpdateInfo updateInfo = infoOperation.GetResult();
        if (updateInfo == null)
        {
            onComplete?.Invoke(SortingUpdateCheckResult.Continue());
            yield break;
        }

        if (updateInfo.UpdateAvailability == UpdateAvailability.DeveloperTriggeredUpdateInProgress)
        {
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update is required to continue."));
            yield return StartImmediateUpdate(appUpdateManager, updateInfo, onComplete);
            yield break;
        }

        if (updateInfo.UpdateAvailability != UpdateAvailability.UpdateAvailable)
        {
            onComplete?.Invoke(SortingUpdateCheckResult.Continue());
            yield break;
        }

        AppUpdateOptions updateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
        if (updateInfo.IsUpdateTypeAllowed(updateOptions))
        {
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update is required to continue."));
            yield return StartImmediateUpdate(appUpdateManager, updateInfo, onComplete);
            yield break;
        }

        updateOptions = AppUpdateOptions.FlexibleAppUpdateOptions();
        if (updateInfo.IsUpdateTypeAllowed(updateOptions))
        {
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update is required to continue."));
            yield return StartFlexibleUpdate(appUpdateManager, updateInfo, onComplete);
            yield break;
        }

        Debug.Log("[Update] Update available, but no update flow is allowed.");
        onComplete?.Invoke(SortingUpdateCheckResult.Block("Update is required, but Google Play cannot start it."));
#else
        onComplete?.Invoke(SortingUpdateCheckResult.Continue());
        yield break;
#endif
    }

#if SORTING_PLAY_APP_UPDATE && UNITY_ANDROID && !UNITY_EDITOR
    private static IEnumerator StartImmediateUpdate(AppUpdateManager appUpdateManager, AppUpdateInfo updateInfo, System.Action<SortingUpdateCheckResult> onComplete)
    {
        AppUpdateOptions options = AppUpdateOptions.ImmediateAppUpdateOptions();
        AppUpdateRequest request = appUpdateManager.StartUpdate(updateInfo, options);
        yield return WaitForUpdateRequest(request, UpdateFlowTimeoutSeconds);

        if (!request.IsDone)
        {
            Debug.LogWarning("[Update] Immediate update timed out.");
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update is required to continue."));
            yield break;
        }

        if (request.Error != AppUpdateErrorCode.NoError)
        {
            Debug.LogWarning("[Update] Immediate update did not complete: " + request.Error);
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update is required to continue."));
            yield break;
        }

        onComplete?.Invoke(SortingUpdateCheckResult.Continue());
    }

    private static IEnumerator StartFlexibleUpdate(AppUpdateManager appUpdateManager, AppUpdateInfo updateInfo, System.Action<SortingUpdateCheckResult> onComplete)
    {
        AppUpdateOptions options = AppUpdateOptions.FlexibleAppUpdateOptions();
        AppUpdateRequest request = appUpdateManager.StartUpdate(updateInfo, options);
        yield return WaitForUpdateRequest(request, UpdateFlowTimeoutSeconds);

        if (!request.IsDone)
        {
            Debug.LogWarning("[Update] Flexible update timed out.");
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update is required to continue."));
            yield break;
        }

        if (request.Error != AppUpdateErrorCode.NoError)
        {
            Debug.LogWarning("[Update] Flexible update did not complete: " + request.Error);
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update is required to continue."));
            yield break;
        }

        PlayAsyncOperation<VoidResult, AppUpdateErrorCode> completeOperation = appUpdateManager.CompleteUpdate();
        yield return completeOperation;

        if (!completeOperation.IsSuccessful)
        {
            Debug.LogWarning("[Update] Complete update failed: " + completeOperation.Error);
            onComplete?.Invoke(SortingUpdateCheckResult.Block("Update install failed. Please try again."));
            yield break;
        }

        onComplete?.Invoke(SortingUpdateCheckResult.Continue());
    }

    private static IEnumerator WaitForPlayOperation<T, TError>(PlayAsyncOperation<T, TError> operation, float timeoutSeconds)
    {
        float startTime = Time.realtimeSinceStartup;
        while (operation != null && !operation.IsDone && Time.realtimeSinceStartup - startTime < timeoutSeconds)
        {
            yield return null;
        }
    }

    private static IEnumerator WaitForUpdateRequest(AppUpdateRequest request, float timeoutSeconds)
    {
        float startTime = Time.realtimeSinceStartup;
        while (request != null && !request.IsDone && Time.realtimeSinceStartup - startTime < timeoutSeconds)
        {
            yield return null;
        }
    }
#endif
}
