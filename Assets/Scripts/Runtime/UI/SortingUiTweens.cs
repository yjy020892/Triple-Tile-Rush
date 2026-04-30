using System;
using DG.Tweening;
using UnityEngine;

public static class SortingUiTweens
{
    public static void KillTweensOn(CanvasGroup group)
    {
        if (group == null) return;
        group.DOKill();
        group.transform.DOKill();
    }

    public static void ResetModalState(CanvasGroup group)
    {
        if (group == null) return;
        group.alpha = 0f;
        if (group.transform.childCount > 0)
        {
            group.transform.GetChild(0).localScale = Vector3.one;
        }
    }

    public static Tween AnimateShowModal(CanvasGroup group, int scaleChildIndex = 0, float fromScale = 0.9f, Action onComplete = null)
    {
        if (group == null) return null;
        KillTweensOn(group);
        group.gameObject.SetActive(true);
        group.interactable = false;
        group.blocksRaycasts = true;
        group.alpha = 0f;
        Transform toScale = group.transform;
        if (group.transform.childCount > scaleChildIndex)
        {
            toScale = group.transform.GetChild(scaleChildIndex);
        }
        toScale.localScale = Vector3.one * fromScale;

        Sequence s = DOTween.Sequence().SetUpdate(true);
        s.Append(group.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).SetId(group));
        s.Join(toScale.DOScale(1f, 0.32f).SetEase(Ease.OutBack, 0.4f).SetId(group));
        s.OnComplete(() =>
        {
            if (group != null)
            {
                group.interactable = true;
            }
            onComplete?.Invoke();
        });
        return s;
    }
}
