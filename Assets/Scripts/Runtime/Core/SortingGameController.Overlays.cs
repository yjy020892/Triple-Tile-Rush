using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Overlay behavior for the baked scene UI.
public partial class SortingGameController
{

    private void OnMenuClicked()
    {
        SortingAudio.Play(SortingAudio.Sfx.Click);
        ShowOverlay(menuOverlay);
    }

    private void OnSoundClicked()
    {
        bool newValue = !SortingSettings.SoundOn;
        SortingSettings.SoundOn = newValue;
        SortingSettings.MusicOn = newValue;
        RefreshSoundButtonVisual();
        SortingAudio.Play(SortingAudio.Sfx.Click);
        analyticsService?.LogEvent("settings_sound_toggle", new Dictionary<string, object>
        {
            { "value", newValue ? 1 : 0 }
        });
    }

    private void RefreshSoundButtonVisual()
    {
        bool on = SortingSettings.SoundOn;
        if (soundMuteBar != null) soundMuteBar.gameObject.SetActive(!on);
        if (soundWave1 != null) soundWave1.gameObject.SetActive(on);
        if (soundWave2 != null) soundWave2.gameObject.SetActive(on);
    }

    // Overlay visibility.

    private void ShowOverlay(CanvasGroup overlay)
    {
        if (overlay == null) return;
        SortingAudio.Play(SortingAudio.Sfx.Whoosh);
        SortingUiTweens.AnimateShowModal(overlay, 0, 0.9f);
    }

    private void HideOverlay(CanvasGroup overlay)
    {
        if (overlay == null) return;
        SortingUiTweens.KillTweensOn(overlay);
        overlay.interactable = false;
        overlay.blocksRaycasts = false;
        overlay.alpha = 0f;
        if (overlay.transform.childCount > 0)
        {
            overlay.transform.GetChild(0).localScale = Vector3.one;
        }
        overlay.gameObject.SetActive(false);
        SortingAudio.Play(SortingAudio.Sfx.Whoosh);
    }

    private void HideOverlayImmediate(CanvasGroup overlay)
    {
        if (overlay == null) return;
        SortingUiTweens.KillTweensOn(overlay);
        overlay.interactable = false;
        overlay.blocksRaycasts = false;
        overlay.alpha = 0f;
        if (overlay.transform.childCount > 0)
        {
            overlay.transform.GetChild(0).localScale = Vector3.one;
        }
        overlay.gameObject.SetActive(false);
    }

    // Overlay visibility.

    private void RefreshSettingsPanel()
    {
        if (settingsSoundToggleLabel != null) settingsSoundToggleLabel.text = SortingSettings.SoundOn ? "ON" : "OFF";
        if (settingsMusicToggleLabel != null) settingsMusicToggleLabel.text = SortingSettings.MusicOn ? "ON" : "OFF";
        if (settingsVibrationToggleLabel != null) settingsVibrationToggleLabel.text = SortingSettings.VibrationOn ? "ON" : "OFF";
        if (settingsSoundToggleBg != null) settingsSoundToggleBg.color = SortingSettings.SoundOn ? new Color(0.36f, 0.78f, 0.42f, 1f) : new Color(0.58f, 0.52f, 0.46f, 1f);
        if (settingsMusicToggleBg != null) settingsMusicToggleBg.color = SortingSettings.MusicOn ? new Color(0.36f, 0.78f, 0.42f, 1f) : new Color(0.58f, 0.52f, 0.46f, 1f);
        if (settingsVibrationToggleBg != null) settingsVibrationToggleBg.color = SortingSettings.VibrationOn ? new Color(0.36f, 0.78f, 0.42f, 1f) : new Color(0.58f, 0.52f, 0.46f, 1f);
    }

    private void OnRestorePurchasesClicked()
    {
        SortingAudio.Play(SortingAudio.Sfx.Click);
        commerce?.Iap.RestorePurchases(success =>
        {
            analyticsService?.LogEvent("iap_restore", new Dictionary<string, object>
            {
                { "success", success ? 1 : 0 }
            });
        });
    }

    // Settings overlay.

    private void OnShopBuyClicked(string sku, TMP_Text priceLabel)
    {
        SortingAudio.Play(SortingAudio.Sfx.Click);
        if (commerce == null) return;
        string originalText = priceLabel != null ? priceLabel.text : "";
        if (priceLabel != null) priceLabel.text = "...";
        commerce.Purchase(sku,
            purchasedSku =>
            {
                SortingAudio.Play(SortingAudio.Sfx.Purchase);
                RefreshTopTexts();
                if (priceLabel != null) priceLabel.text = "OWNED";
            },
            (failedSku, reason) =>
            {
                SortingAudio.Play(SortingAudio.Sfx.Error);
                if (priceLabel != null) priceLabel.text = originalText;
            });
    }

    private void OnWatchRewardedAdClicked()
    {
        SortingAudio.Play(SortingAudio.Sfx.Click);
        if (commerce == null) return;
        bool shown = commerce.TryShowRewarded(SortingAdPlacements.RewardedDoubleCoin, () =>
        {
            const int RewardAmount = 50;
            wallet.Add(RewardAmount);
            SortingAudio.Play(SortingAudio.Sfx.Coin);
            analyticsService?.LogEvent("shop_rewarded_grant", new Dictionary<string, object>
            {
                { "amount", RewardAmount }
            });
        });
        if (!shown)
        {
            analyticsService?.LogEvent("shop_rewarded_unavailable", null);
        }
    }

    // Shop overlay.

    private void ShowTutorialText(string text)
    {
        if (tutorialText != null) tutorialText.text = text;
        ShowOverlay(tutorialOverlay);

        StopTutorialFingerPulseIfNeeded();
        if (tutorialLayoutCoroutine != null)
        {
            StopCoroutine(tutorialLayoutCoroutine);
            tutorialLayoutCoroutine = null;
        }

        if (!string.IsNullOrEmpty(currentTutorialKey))
        {
            tutorialLayoutCoroutine = StartCoroutine(TutorialLayoutAndPulseRoutine());
        }
        else if (tutorialArrow != null)
        {
            tutorialArrow.gameObject.SetActive(false);
        }
    }

    private IEnumerator TutorialLayoutAndPulseRoutine()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null;
        if (tutorialOverlay == null || tutorialOverlay.alpha < 0.01f) yield break;
        LayoutTutorialFingerCue(currentTutorialKey);
        PulseTutorialArrow();
        tutorialLayoutCoroutine = null;
    }

    private void PulseTutorialArrow()
    {
        if (tutorialArrow == null) return;
        tutorialArrow.DOKill(false);
        tutorialArrow.localScale = Vector3.one;
        tutorialArrow.DOScale(1.12f, 0.4f).SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad).SetUpdate(true);
    }

    private void StopTutorialFingerPulseIfNeeded()
    {
        if (tutorialArrow == null)
        {
            return;
        }

        tutorialArrow.DOKill(false);
        tutorialArrow.localScale = Vector3.one;
    }

    private void LayoutTutorialFingerCue(string key)
    {
        if (tutorialArrow == null) return;
        tutorialArrow.gameObject.SetActive(true);

        RectTransform tray = slotRoot != null ? slotRoot : bottomRoot;

        switch (key)
        {
            case SortingTutorialKeys.MatchThree:
            case SortingTutorialKeys.FullTray:
                if (tray != null)
                {
                    float lift = Mathf.Clamp(currentTileSize * 0.4f, 32f, 72f);
                    tutorialArrow.position = tray.TransformPoint(new Vector3(0f, tray.rect.height * 0.5f + lift, 0f));
                }
                else if (boardRoot != null)
                {
                    float yCue = Mathf.Clamp(boardRoot.rect.height * 0.15f + currentTileSize * 1.05f,
                        currentTileSize * 1f, Mathf.Max(boardRoot.rect.height * 0.5f, 180f));
                    tutorialArrow.position = boardRoot.TransformPoint(new Vector3(0f, yCue, 0f));
                }
                break;
            case SortingTutorialKeys.LayersIntro:
                if (boardRoot != null)
                {
                    float yCue = Mathf.Clamp(boardRoot.rect.height * 0.15f + currentTileSize * 1.05f,
                        currentTileSize * 1f, Mathf.Max(boardRoot.rect.height * 0.5f, 180f));
                    tutorialArrow.position = boardRoot.TransformPoint(new Vector3(0f, yCue, 0f));
                }
                break;
            case SortingTutorialKeys.TapTile:
                SortingItemView cue = TutorialPickCueTileForTap();
                if (cue != null)
                {
                    float off = Mathf.Clamp(currentTileSize * 0.55f, 72f, 160f);
                    tutorialArrow.position = cue.RectTransform.position + Vector3.up * off;
                    break;
                }

                goto case SortingTutorialKeys.LayersIntro;
            default:
                if (boardRoot != null)
                {
                    tutorialArrow.position = boardRoot.TransformPoint(new Vector3(0f,
                        Mathf.Clamp(boardRoot.rect.height * 0.2f + currentTileSize, 140f, 420f), 0f));
                }

                break;
        }
    }

    private SortingItemView TutorialPickCueTileForTap()
    {
        if (boardItems == null || boardItems.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < boardItems.Count; i++)
        {
            SortingItemView iv = boardItems[i];
            if (iv != null && !iv.IsRemoved && iv.IsExposed)
            {
                return iv;
            }
        }

        for (int j = 0; j < boardItems.Count; j++)
        {
            SortingItemView iv2 = boardItems[j];
            if (iv2 != null && !iv2.IsRemoved)
            {
                return iv2;
            }
        }

        return boardItems[0];
    }

    private void MaybeShowTutorialForLevel(int level)
    {
        if (tutorialOverlay == null)
        {
            return;
        }

        string key = null;
        string text = null;

        if (level == 1 && !SortingSettings.IsTutorialSeen(SortingTutorialKeys.TapTile))
        {
            key = SortingTutorialKeys.TapTile;
            text = "갈 수 있는 타일을 누르면 아래 바에 쌓여요.";
        }
        else if (level == 2 && !SortingSettings.IsTutorialSeen(SortingTutorialKeys.MatchThree))
        {
            key = SortingTutorialKeys.MatchThree;
            text = "같은 종류 타일 세 개가 같은 바 안에 모이면 매치되어 사라져요.";
        }
        else if (level == 3 && !SortingSettings.IsTutorialSeen(SortingTutorialKeys.FullTray))
        {
            key = SortingTutorialKeys.FullTray;
            text = "바 칸을 전부 채우면 패배예요.\n부스터와 힌트로 순서를 조절하세요.";
        }
        else if (!SortingSettings.IsTutorialSeen(SortingTutorialKeys.LayersIntro)
                 && currentDefinition != null && currentDefinition.layerCount >= 2)
        {
            key = SortingTutorialKeys.LayersIntro;
            text = "맨 위에 있는 타일을 먼저 치워야 아래 줄을 선택할 수 있어요.\n층 위에서부터 순서대로 정리하면서 진행해요!";
        }

        if (key == null || text == null)
        {
            return;
        }

        currentTutorialKey = key;
        ShowTutorialText(text);
    }

    private void DismissTutorial()
    {
        StopTutorialFingerPulseIfNeeded();
        if (tutorialLayoutCoroutine != null)
        {
            StopCoroutine(tutorialLayoutCoroutine);
            tutorialLayoutCoroutine = null;
        }

        if (!string.IsNullOrEmpty(currentTutorialKey))
        {
            SortingSettings.MarkTutorialSeen(currentTutorialKey);
            analyticsService?.LogEvent("tutorial_complete", new Dictionary<string, object>
            {
                { "key", currentTutorialKey }
            });
            currentTutorialKey = null;
        }
        HideOverlay(tutorialOverlay);
    }
}
