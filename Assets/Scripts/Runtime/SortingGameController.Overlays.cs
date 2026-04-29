using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 메뉴/설정/상점/튜토리얼 오버레이. 기존 컨트롤러의 팝업과 같은 modal 패턴으로 구성.
public partial class SortingGameController
{
    // =============== 메뉴 / 사운드 버튼 핸들러 ===============

    private void OnMenuClicked()
    {
        SortingAudio.Play(SortingAudio.Sfx.Click);
        ShowOverlay(menuOverlay);
    }

    private void OnSoundClicked()
    {
        bool newValue = !SortingSettings.SoundOn;
        SortingSettings.SoundOn = newValue;
        SortingSettings.MusicOn = newValue;     // 간단한 통합 토글: 한 버튼으로 사운드 + 음악 동시에
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
        // 웨이브(스피커 음파 표시)는 사운드 ON일 때만 표시
        if (soundWave1 != null) soundWave1.gameObject.SetActive(on);
        if (soundWave2 != null) soundWave2.gameObject.SetActive(on);
    }

    // =============== 오버레이 전체 빌드 ===============

    private void CreateOverlays(RectTransform parent)
    {
        menuOverlay = BuildMenuOverlay(parent);
        settingsOverlay = BuildSettingsOverlay(parent);
        shopOverlay = BuildShopOverlay(parent);
        tutorialOverlay = BuildTutorialOverlay(parent);

        HideOverlayImmediate(menuOverlay);
        HideOverlayImmediate(settingsOverlay);
        HideOverlayImmediate(shopOverlay);
        HideOverlayImmediate(tutorialOverlay);

        RefreshSoundButtonVisual();
    }

    // =============== 모달 헬퍼 ===============

    private CanvasGroup BuildModalPanel(
        RectTransform parent, string name, string title,
        Vector2 panelSize, out RectTransform content, out Button closeButton)
    {
        RectTransform overlay = CreatePanel(parent, name,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.60f));
        CanvasGroup group = overlay.gameObject.AddComponent<CanvasGroup>();
        Image overlayImage = overlay.GetComponent<Image>();
        if (overlayImage != null) overlayImage.raycastTarget = true;

        RectTransform panel = CreatePanel(overlay, "Panel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, panelSize, new Color(0.97f, 0.92f, 0.82f, 1f));
        ApplyRounded(panel, 38);
        AddPanelEdge(panel, new Color(0.35f, 0.20f, 0.06f, 1f), 5f, new Vector2(1f, -1f));

        // 상단 나무 헤더
        RectTransform header = CreatePanel(panel, "Header",
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, 0f), new Vector2(0f, 108f), new Color(0.90f, 0.55f, 0.20f, 1f));
        ApplyRounded(header, 38);
        AddPanelEdge(header, new Color(0.40f, 0.20f, 0.04f, 1f), 4f, new Vector2(1f, -1f));
        RectTransform headerGloss = CreatePanel(header, "HeaderGloss",
            new Vector2(0.04f, 0.60f), new Vector2(0.96f, 0.94f), new Vector2(0.5f, 1f),
            Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.32f));
        ApplyRounded(headerGloss, 20);
        headerGloss.GetComponent<Image>().raycastTarget = false;
        TMP_Text titleText = CreateText(header, "Title", title, 46,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        titleText.color = new Color(1f, 0.98f, 0.90f, 1f);

        // 콘텐츠 영역 (헤더 아래)
        content = CreatePanel(panel, "Content",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -54f), new Vector2(-60f, -160f), new Color(0f, 0f, 0f, 0f));
        content.GetComponent<Image>().raycastTarget = false;

        // 오른쪽 위 닫기(X) 버튼
        RectTransform closeRect = CreatePanel(header, "CloseButton",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-20f, 0f), new Vector2(68f, 68f), new Color(0.42f, 0.22f, 0.08f, 1f));
        ApplyCircle(closeRect);
        AddPanelEdge(closeRect, new Color(0.20f, 0.10f, 0.03f, 1f), 3f, new Vector2(1f, -1f));
        Image closeImage = closeRect.GetComponent<Image>();
        closeImage.raycastTarget = true;
        closeButton = closeRect.gameObject.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        TMP_Text closeText = CreateText(closeRect, "X", "×", 48,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        closeText.color = new Color(1f, 0.96f, 0.82f, 1f);

        overlay.gameObject.SetActive(false);
        return group;
    }

    private void ShowOverlay(CanvasGroup overlay)
    {
        if (overlay == null) return;
        SortingAudio.Play(SortingAudio.Sfx.Whoosh);
        // 첫 자식=모달 패널(또는 튜토리얼 카드)만 스케일, 딤은 CanvasGroup 페이드만
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

    // =============== 메뉴 오버레이 ===============

    private CanvasGroup BuildMenuOverlay(RectTransform parent)
    {
        Button closeBtn;
        CanvasGroup overlay = BuildModalPanel(parent, "MenuOverlay", "MENU",
            new Vector2(820f, 880f), out RectTransform content, out closeBtn);
        closeBtn.onClick.AddListener(() => HideOverlay(menuOverlay));

        // 세로로 정렬된 큰 버튼들
        CreateMenuItemButton(content, "Resume", "\u25B6  Resume", 0,
            new Color(0.36f, 0.78f, 0.42f, 1f),
            () => { HideOverlay(menuOverlay); SortingAudio.Play(SortingAudio.Sfx.Click); });

        CreateMenuItemButton(content, "Shop", "\u2605  Shop", 1,
            new Color(1f, 0.72f, 0.22f, 1f),
            () => { HideOverlay(menuOverlay); ShowOverlay(shopOverlay); });

        CreateMenuItemButton(content, "Settings", "\u2699  Settings", 2,
            new Color(0.46f, 0.64f, 0.94f, 1f),
            () => { HideOverlay(menuOverlay); RefreshSettingsPanel(); ShowOverlay(settingsOverlay); });

        CreateMenuItemButton(content, "HowToPlay", "?  How to Play", 3,
            new Color(0.78f, 0.54f, 0.92f, 1f),
            () =>
            {
                HideOverlay(menuOverlay);
                currentTutorialKey = null; // 메뉴에서 본 튜토리얼은 저장하지 않음
                ShowTutorialText("타일은 누르면 아래 칸으로 옮아요.\n"
                               + "같은 종류를 3개 모으면 매치!\n"
                               + "바가 다 차면 패배이니 칸 여유와 부스터를 활용해요.");
            });

        CreateMenuItemButton(content, "Restart", "\u21BA  Restart Stage", 4,
            new Color(0.82f, 0.42f, 0.36f, 1f),
            () =>
            {
                HideOverlay(menuOverlay);
                SortingAudio.Play(SortingAudio.Sfx.Click);
                StartLevel();
            });

        return overlay;
    }

    private void CreateMenuItemButton(RectTransform parent, string name, string label, int rowIndex, Color color, Action onClick)
    {
        float rowHeight = 108f;
        float spacing = 22f;
        float y = -(rowHeight * 0.5f + rowIndex * (rowHeight + spacing));
        RectTransform btnRect = CreatePanel(parent, name,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, y), new Vector2(660f, rowHeight), color);
        ApplyRounded(btnRect, 28);
        AddPanelEdge(btnRect, new Color(0.24f, 0.12f, 0.03f, 1f), 3f, new Vector2(1f, -1f));
        RectTransform gloss = CreatePanel(btnRect, "Gloss",
            new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.94f), new Vector2(0.5f, 1f),
            Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.28f));
        ApplyRounded(gloss, 18);
        gloss.GetComponent<Image>().raycastTarget = false;
        Image img = btnRect.GetComponent<Image>();
        img.raycastTarget = true;
        Button btn = btnRect.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor = color;
        cb.highlightedColor = new Color(Mathf.Min(1f, color.r * 1.08f), Mathf.Min(1f, color.g * 1.08f), Mathf.Min(1f, color.b * 1.08f), 1f);
        cb.pressedColor = color * 0.82f;
        btn.colors = cb;
        btn.onClick.AddListener(() => onClick?.Invoke());
        btn.onClick.AddListener(() =>
        {
            btnRect.DOKill();
            btnRect.DOPunchScale(new Vector3(0.04f, 0.04f, 0f), 0.16f, 4, 0.2f).SetUpdate(true);
        });
        TMP_Text text = CreateText(btnRect, "Label", label, 38,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        text.color = new Color(1f, 0.98f, 0.90f, 1f);
    }

    // =============== 설정 오버레이 ===============

    private CanvasGroup BuildSettingsOverlay(RectTransform parent)
    {
        Button closeBtn;
        CanvasGroup overlay = BuildModalPanel(parent, "SettingsOverlay", "SETTINGS",
            new Vector2(820f, 860f), out RectTransform content, out closeBtn);
        closeBtn.onClick.AddListener(() => HideOverlay(settingsOverlay));

        CreateSettingsToggleRow(content, "SoundRow", "Sound", 0, SortingSettings.SoundOn,
            out settingsSoundToggleBg, out settingsSoundToggleLabel,
            on =>
            {
                SortingSettings.SoundOn = on;
                RefreshSettingsPanel();
                RefreshSoundButtonVisual();
            });
        CreateSettingsToggleRow(content, "MusicRow", "Music", 1, SortingSettings.MusicOn,
            out settingsMusicToggleBg, out settingsMusicToggleLabel,
            on =>
            {
                SortingSettings.MusicOn = on;
                SortingAudio.PlayBgm(on);
                RefreshSettingsPanel();
            });
        CreateSettingsToggleRow(content, "VibrationRow", "Vibration", 2, SortingSettings.VibrationOn,
            out settingsVibrationToggleBg, out settingsVibrationToggleLabel,
            on =>
            {
                SortingSettings.VibrationOn = on;
                if (on) SortingAudio.Vibrate(SortingAudio.HapticStyle.Light);
                RefreshSettingsPanel();
            });

        // Restore Purchases 버튼 (하단)
        RectTransform restoreRect = CreatePanel(content, "RestoreButton",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 130f), new Vector2(540f, 82f), new Color(0.42f, 0.22f, 0.08f, 1f));
        ApplyRounded(restoreRect, 22);
        AddPanelEdge(restoreRect, new Color(0.20f, 0.10f, 0.03f, 1f), 3f, new Vector2(1f, -1f));
        Image restoreImg = restoreRect.GetComponent<Image>();
        restoreImg.raycastTarget = true;
        Button restoreBtn = restoreRect.gameObject.AddComponent<Button>();
        restoreBtn.targetGraphic = restoreImg;
        restoreBtn.onClick.AddListener(OnRestorePurchasesClicked);
        TMP_Text restoreLabel = CreateText(restoreRect, "Label", "Restore Purchases", 28,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        restoreLabel.color = new Color(1f, 0.96f, 0.82f, 1f);

        // 하단 안내 (credits)
        TMP_Text credit = CreateText(content, "Credit",
            $"Tile Master v0.9 — highest cleared Lv {SortingProgress.GetHighestClearedLevel()}",
            18, FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 60f), new Vector2(0f, 84f));
        credit.color = new Color(0.40f, 0.22f, 0.05f, 0.7f);

        return overlay;
    }

    private void CreateSettingsToggleRow(
        RectTransform parent, string name, string label, int rowIndex, bool initialOn,
        out Image knob, out TMP_Text valueLabel, Action<bool> onChanged)
    {
        float rowHeight = 96f;
        float spacing = 22f;
        float y = -(rowHeight * 0.5f + 14f + rowIndex * (rowHeight + spacing));
        RectTransform row = CreatePanel(parent, name,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, y), new Vector2(660f, rowHeight), new Color(0.90f, 0.78f, 0.58f, 0.35f));
        ApplyRounded(row, 22);

        TMP_Text nameText = CreateText(row, "Name", label, 34,
            FontStyles.Bold, TextAlignmentOptions.Left,
            new Vector2(0f, 0f), new Vector2(0.5f, 1f),
            new Vector2(32f, 0f), Vector2.zero);
        nameText.color = new Color(0.28f, 0.15f, 0.04f, 1f);

        // 토글 바디 — 큰 알약 모양
        RectTransform toggle = CreatePanel(row, "Toggle",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-28f, 0f), new Vector2(136f, 62f),
            initialOn ? new Color(0.36f, 0.78f, 0.42f, 1f) : new Color(0.58f, 0.52f, 0.46f, 1f));
        ApplyRounded(toggle, 32);
        AddPanelEdge(toggle, new Color(0.20f, 0.10f, 0.03f, 0.6f), 2f, new Vector2(1f, -1f));
        Image toggleImg = toggle.GetComponent<Image>();
        toggleImg.raycastTarget = true;
        knob = toggleImg; // 배경 이미지를 토글 상태 색으로 쓴다
        Button toggleBtn = toggle.gameObject.AddComponent<Button>();
        toggleBtn.targetGraphic = toggleImg;

        valueLabel = CreateText(toggle, "Value", initialOn ? "ON" : "OFF", 24,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        valueLabel.color = Color.white;

        // 상태 저장(클릭 클로저용)
        bool state = initialOn;
        TMP_Text labelRef = valueLabel;
        Image bgRef = toggleImg;
        toggleBtn.onClick.AddListener(() =>
        {
            state = !state;
            labelRef.text = state ? "ON" : "OFF";
            bgRef.color = state ? new Color(0.36f, 0.78f, 0.42f, 1f) : new Color(0.58f, 0.52f, 0.46f, 1f);
            SortingAudio.Play(SortingAudio.Sfx.Click);
            onChanged?.Invoke(state);
        });
    }

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

    // =============== 상점 오버레이 ===============

    private CanvasGroup BuildShopOverlay(RectTransform parent)
    {
        Button closeBtn;
        CanvasGroup overlay = BuildModalPanel(parent, "ShopOverlay", "SHOP",
            new Vector2(900f, 1360f), out RectTransform content, out closeBtn);
        closeBtn.onClick.AddListener(() => HideOverlay(shopOverlay));

        // 상단: 무료 Rewarded 영상 광고로 코인 획득
        RectTransform rewardRect = CreatePanel(content, "RewardedRow",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -52f), new Vector2(740f, 100f), new Color(0.36f, 0.78f, 0.42f, 1f));
        ApplyRounded(rewardRect, 26);
        AddPanelEdge(rewardRect, new Color(0.14f, 0.38f, 0.18f, 1f), 3f, new Vector2(1f, -1f));
        Image rewardImg = rewardRect.GetComponent<Image>();
        rewardImg.raycastTarget = true;
        Button rewardBtn = rewardRect.gameObject.AddComponent<Button>();
        rewardBtn.targetGraphic = rewardImg;
        rewardBtn.onClick.AddListener(OnWatchRewardedAdClicked);
        TMP_Text rewardText = CreateText(rewardRect, "Label",
            "\u25B6  Watch Ad  +50 Coins", 32,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        rewardText.color = new Color(1f, 0.98f, 0.90f, 1f);

        // 상품 카드 리스트 — 세로 스택
        List<SortingIapProductDef> products = new List<SortingIapProductDef>
        {
            SortingIapCatalog.Find(SortingIapCatalog.SkuStarterPack),
            SortingIapCatalog.Find(SortingIapCatalog.SkuRemoveAds),
            SortingIapCatalog.Find(SortingIapCatalog.SkuCoinSmall),
            SortingIapCatalog.Find(SortingIapCatalog.SkuCoinMedium),
            SortingIapCatalog.Find(SortingIapCatalog.SkuCoinLarge),
            SortingIapCatalog.Find(SortingIapCatalog.SkuCoinHuge),
        };

        float cardStartY = -180f;
        float cardHeight = 150f;
        float cardSpacing = 18f;
        for (int i = 0; i < products.Count; i++)
        {
            if (products[i] == null) continue;
            float y = cardStartY - i * (cardHeight + cardSpacing);
            CreateShopProductCard(content, products[i], y, cardHeight);
        }

        return overlay;
    }

    private void CreateShopProductCard(RectTransform parent, SortingIapProductDef def, float y, float height)
    {
        bool isPremium = def.kind == SortingIapKind.StarterPack;
        bool isRemoveAds = def.sku == SortingIapCatalog.SkuRemoveAds;
        Color cardColor = isPremium
            ? new Color(1f, 0.82f, 0.30f, 1f)
            : (isRemoveAds ? new Color(0.46f, 0.64f, 0.94f, 1f) : new Color(0.97f, 0.88f, 0.72f, 1f));
        Color titleColor = isPremium ? new Color(0.32f, 0.15f, 0.02f, 1f)
                                     : (isRemoveAds ? new Color(1f, 0.98f, 0.90f, 1f) : new Color(0.26f, 0.14f, 0.03f, 1f));

        RectTransform card = CreatePanel(parent, $"Product_{def.sku}",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, y), new Vector2(760f, height), cardColor);
        ApplyRounded(card, 24);
        AddPanelEdge(card, new Color(0.26f, 0.14f, 0.03f, 1f), 3f, new Vector2(1f, -1f));

        // 왼쪽 아이콘 박스
        RectTransform iconRect = CreatePanel(card, "Icon",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(28f, 0f), new Vector2(100f, 100f),
            isRemoveAds ? new Color(0.28f, 0.44f, 0.70f, 1f) : new Color(1f, 0.68f, 0.24f, 1f));
        ApplyCircle(iconRect);
        AddPanelEdge(iconRect, new Color(0.20f, 0.10f, 0.03f, 0.6f), 2f, new Vector2(1f, -1f));
        string iconGlyph = isRemoveAds ? "\u2715" : (isPremium ? "\u2605" : "$");
        TMP_Text iconText = CreateText(iconRect, "Glyph", iconGlyph, 54,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        iconText.color = new Color(1f, 0.98f, 0.90f, 1f);

        // 가운데 타이틀/설명
        TMP_Text titleText = CreateText(card, "Title", def.title, 30,
            FontStyles.Bold, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0.5f), new Vector2(1f, 1f),
            new Vector2(150f, 10f), new Vector2(-180f, -14f));
        titleText.color = titleColor;
        TMP_Text descText = CreateText(card, "Desc", def.description, 20,
            FontStyles.Normal, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0.5f),
            new Vector2(150f, 10f), new Vector2(-180f, -6f));
        descText.color = new Color(titleColor.r, titleColor.g, titleColor.b, 0.85f);

        // 오른쪽 구매 버튼
        RectTransform buyRect = CreatePanel(card, "BuyButton",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-20f, 0f), new Vector2(150f, 88f), new Color(0.32f, 0.62f, 0.30f, 1f));
        ApplyRounded(buyRect, 22);
        AddPanelEdge(buyRect, new Color(0.10f, 0.32f, 0.14f, 1f), 3f, new Vector2(1f, -1f));
        Image buyImg = buyRect.GetComponent<Image>();
        buyImg.raycastTarget = true;
        Button buyBtn = buyRect.gameObject.AddComponent<Button>();
        buyBtn.targetGraphic = buyImg;
        string priceText = def.fallbackPriceText;
        TMP_Text priceLabel = CreateText(buyRect, "Price", priceText, 28,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        priceLabel.color = new Color(1f, 0.98f, 0.90f, 1f);

        string skuRef = def.sku;
        buyBtn.onClick.AddListener(() => OnShopBuyClicked(skuRef, priceLabel));

        // 이미 보유한 항목은 비활성 + "OWNED"
        if (commerce != null && commerce.Iap.IsOwned(def.sku))
        {
            buyImg.color = new Color(0.55f, 0.55f, 0.55f, 1f);
            priceLabel.text = "OWNED";
            buyBtn.interactable = false;
        }
    }

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

    // =============== 튜토리얼 코치마크 ===============

    private CanvasGroup BuildTutorialOverlay(RectTransform parent)
    {
        RectTransform overlay = CreatePanel(parent, "TutorialOverlay",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.55f));
        CanvasGroup group = overlay.gameObject.AddComponent<CanvasGroup>();
        Image overlayImage = overlay.GetComponent<Image>();
        if (overlayImage != null) overlayImage.raycastTarget = true;

        // 오버레이 자체 클릭으로도 닫히도록
        Button overlayBtn = overlay.gameObject.AddComponent<Button>();
        overlayBtn.targetGraphic = overlayImage;
        overlayBtn.transition = Selectable.Transition.None;
        overlayBtn.onClick.AddListener(DismissTutorial);

        // 텍스트 카드 — 중앙 상단
        RectTransform card = CreatePanel(overlay, "Card",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 120f), new Vector2(840f, 320f), new Color(1f, 0.98f, 0.90f, 0.98f));
        ApplyRounded(card, 28);
        AddPanelEdge(card, new Color(0.35f, 0.20f, 0.06f, 1f), 4f, new Vector2(1f, -1f));
        card.GetComponent<Image>().raycastTarget = false;

        tutorialText = CreateText(card, "Text",
            "타일을 누르면 아래 수집 바로 옮겨져요.", 32,
            FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0.04f, 0.22f), new Vector2(0.96f, 0.96f),
            Vector2.zero, Vector2.zero);
        tutorialText.color = new Color(0.26f, 0.14f, 0.03f, 1f);

        TMP_Text tapHint = CreateText(card, "TapHint", "아무 곳이나 눌러 계속하기", 20,
            FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(1f, 0.24f),
            Vector2.zero, Vector2.zero);
        tapHint.color = new Color(0.40f, 0.22f, 0.05f, 0.75f);

        // 아래 방향 화살표 (코치 표시 위치는 LayoutTutorialFingerCue에서 갱신)
        tutorialArrow = CreatePanel(overlay, "Arrow",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -80f), new Vector2(88f, 88f), new Color(1f, 0.84f, 0.28f, 1f));
        ApplyCircle(tutorialArrow);
        AddPanelEdge(tutorialArrow, new Color(0.50f, 0.28f, 0.04f, 1f), 3f, new Vector2(1f, -1f));
        tutorialArrow.GetComponent<Image>().raycastTarget = false;
        TMP_Text arrowText = CreateText(tutorialArrow, "Glyph", "\u25BC", 40,
            FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        arrowText.color = new Color(0.30f, 0.15f, 0.03f, 1f);

        overlay.gameObject.SetActive(false);
        return group;
    }

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

    /// <summary> Tile Master류: 첫 줄은 탭, 둘째는 3매치, 셋째는 실패 조건 → 이후 레이어 첫 진입에서 층 설명.</summary>
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
            text = "바 칸을 전부 채우면 패배예요.\n부스터·힌트로 순서를 조절하세요.";
        }
        else if (!SortingSettings.IsTutorialSeen(SortingTutorialKeys.LayersIntro)
                 && currentDefinition != null && currentDefinition.layerCount >= 2)
        {
            key = SortingTutorialKeys.LayersIntro;
            text = "맨 위에 있는 타일을 먼저 치워야 아래 줄이 선택돼요.\n층 위에서부터 순서대로 정리하면서 진행해요!";
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
