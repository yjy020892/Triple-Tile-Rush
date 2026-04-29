using System;
using DG.Tweening;
using UnityEngine;

// DOTween 래퍼. UI 는 SetUpdate(true) 로 일시정지(팝업)에도 돌게 함.
// CanvasGroup = 전체 스크린 (딤) + 그 자손(첫 Rect) 을 흔히 스케일 인한다.
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

    /// <summary>전체 오버레이(딤) 페이드 + 첫 자식(패널) 스케일.</summary>
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
