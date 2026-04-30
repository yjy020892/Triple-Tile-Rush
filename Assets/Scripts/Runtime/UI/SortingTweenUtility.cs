using System.Collections;
using UnityEngine;

public static class SortingTweenUtility
{
    public static IEnumerator ScalePunch(Transform target, float duration, float punchScale)
    {
        if (target == null)
        {
            yield break;
        }

        Vector3 baseScale = target.localScale;
        Vector3 enlargedScale = baseScale * punchScale;

        float halfDuration = Mathf.Max(0.01f, duration * 0.5f);
        float time = 0f;
        while (time < halfDuration)
        {
            if (target == null)
            {
                yield break;
            }

            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / halfDuration);
            target.localScale = Vector3.LerpUnclamped(baseScale, enlargedScale, EaseOutBack(t));
            yield return null;
        }

        time = 0f;
        while (time < halfDuration)
        {
            if (target == null)
            {
                yield break;
            }

            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / halfDuration);
            target.localScale = Vector3.LerpUnclamped(enlargedScale, baseScale, EaseOutCubic(t));
            yield return null;
        }

        if (target != null)
        {
            target.localScale = baseScale;
        }
    }

    public static IEnumerator MoveTo(RectTransform target, Vector3 worldTarget, float duration, System.Action onComplete = null)
    {
        if (target == null)
        {
            yield break;
        }

        Vector3 startPosition = target.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.position = Vector3.LerpUnclamped(startPosition, worldTarget, EaseOutCubic(t));
            yield return null;
        }

        if (target != null)
        {
            target.position = worldTarget;
        }
        onComplete?.Invoke();
    }

    public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.alpha = from;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, EaseOutCubic(t));
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = to;
        }
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private static float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
