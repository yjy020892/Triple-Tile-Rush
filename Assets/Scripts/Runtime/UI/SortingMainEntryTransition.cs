using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SortingMainEntryTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup overlayGroup;
    [SerializeField] private RectTransform leftCloudBank;
    [SerializeField] private RectTransform rightCloudBank;
    [SerializeField] private float holdSeconds = 0.08f;
    [SerializeField] private float revealSeconds = 0.62f;
    [SerializeField] private float moveDistance = 760f;

    private Vector2 leftCoveredPosition;
    private Vector2 rightCoveredPosition;
    private Coroutine revealRoutine;

    private void Awake()
    {
        CacheSceneReferences();
        CaptureCoveredPositions();
    }

    public void PrepareCovered()
    {
        CacheSceneReferences();
        CaptureCoveredPositions();

        if (overlayGroup != null)
        {
            overlayGroup.gameObject.SetActive(true);
            overlayGroup.alpha = 1f;
            overlayGroup.interactable = false;
            overlayGroup.blocksRaycasts = true;
        }

        SetCoveredPositions();
    }

    public void BeginReveal()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
        }

        revealRoutine = StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        CacheSceneReferences();
        if (overlayGroup == null)
        {
            yield break;
        }

        if (holdSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(holdSeconds);
        }

        Vector2 leftTarget = leftCoveredPosition + Vector2.left * moveDistance;
        Vector2 rightTarget = rightCoveredPosition + Vector2.right * moveDistance;
        float duration = Mathf.Max(0.01f, revealSeconds);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseInOutCubic(t);
            if (leftCloudBank != null)
            {
                leftCloudBank.anchoredPosition = Vector2.LerpUnclamped(leftCoveredPosition, leftTarget, eased);
            }
            if (rightCloudBank != null)
            {
                rightCloudBank.anchoredPosition = Vector2.LerpUnclamped(rightCoveredPosition, rightTarget, eased);
            }

            overlayGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }

        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;
        overlayGroup.gameObject.SetActive(false);
        revealRoutine = null;
    }

    private void CacheSceneReferences()
    {
        if (overlayGroup == null)
        {
            overlayGroup = GetComponent<CanvasGroup>();
        }

        if (leftCloudBank == null)
        {
            leftCloudBank = FindNamedRect(transform, "LeftCloudBank");
        }

        if (rightCloudBank == null)
        {
            rightCloudBank = FindNamedRect(transform, "RightCloudBank");
        }
    }

    private void CaptureCoveredPositions()
    {
        if (leftCloudBank != null)
        {
            leftCoveredPosition = leftCloudBank.anchoredPosition;
        }

        if (rightCloudBank != null)
        {
            rightCoveredPosition = rightCloudBank.anchoredPosition;
        }
    }

    private void SetCoveredPositions()
    {
        if (leftCloudBank != null)
        {
            leftCloudBank.anchoredPosition = leftCoveredPosition;
        }

        if (rightCloudBank != null)
        {
            rightCloudBank.anchoredPosition = rightCoveredPosition;
        }
    }

    private static RectTransform FindNamedRect(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == objectName)
        {
            return root as RectTransform;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform found = FindNamedRect(root.GetChild(i), objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
    }
}
