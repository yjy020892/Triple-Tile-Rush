using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SortingBootController : MonoBehaviour
{
    private const string BootSceneName = "Boot";
    private const string MainSceneName = "Main";

    private ISortingAuthService authService;
    private ISortingUpdateService updateService;
    private CanvasGroup choiceGroup;
    private TMP_Text statusText;
    private Button googleButton;
    private Button guestButton;
    private GameObject eventSystemObject;
    private bool isBusy;
    private bool isUpdateBlocked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallIfBootScene()
    {
        if (SceneManager.GetActiveScene().name != BootSceneName)
        {
            return;
        }

        if (FindObjectOfType<SortingBootController>() != null)
        {
            return;
        }

        new GameObject("SortingBootController").AddComponent<SortingBootController>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        authService = SortingSdkBootstrapper.CreateAuthService();
        updateService = SortingSdkBootstrapper.CreateUpdateService();
        BuildUi();
    }

    private IEnumerator Start()
    {
        yield return RunUpdateCheck();
        if (isUpdateBlocked)
        {
            yield break;
        }

        SetBusy(true, "Loading...");
        bool initialized = false;
        string initError = string.Empty;
        authService.Initialize((success, error) =>
        {
            initialized = success;
            initError = error;
        });

        yield return null;

        if (!initialized)
        {
            SetBusy(false, string.IsNullOrEmpty(initError) ? "Login setup failed." : initError);
            yield break;
        }

        string status = authService.HasCachedSession
            ? "Choose account to continue."
            : "Choose how to start.";
        SetBusy(false, status);
    }

    private IEnumerator RunUpdateCheck()
    {
        isUpdateBlocked = false;
        SetBusy(true, "Checking for updates...");
        if (updateService == null)
        {
            yield break;
        }

        SortingUpdateCheckResult result = null;
        yield return updateService.CheckForUpdate(updateResult => result = updateResult);
        if (result == null || result.CanContinue)
        {
            yield break;
        }

        isUpdateBlocked = true;
        SetBusy(false, string.IsNullOrEmpty(result.Message) ? "Update is required to continue." : result.Message);
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("BootCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<EventSystem>() == null)
        {
            eventSystemObject = new GameObject("BootEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(transform, false);
        }

        GameObject background = CreateRect(canvasObject.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.92f, 0.70f, 0.38f, 1f);

        GameObject panel = CreateRect(canvasObject.transform, "Panel", new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.82f), Vector2.zero, Vector2.zero);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1f, 0.88f, 0.62f, 0.96f);

        TMP_Text title = CreateText(panel.transform, "Title", "Triple Tile Rush", 54f, FontStyles.Bold, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.92f));
        title.color = new Color(0.32f, 0.18f, 0.07f, 1f);

        TMP_Text subtitle = CreateText(panel.transform, "Subtitle", "Sign in to save your progress.", 28f, FontStyles.Normal, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.72f));
        subtitle.color = new Color(0.45f, 0.28f, 0.12f, 1f);

        GameObject choices = CreateRect(panel.transform, "Choices", new Vector2(0.12f, 0.27f), new Vector2(0.88f, 0.56f), Vector2.zero, Vector2.zero);
        choiceGroup = choices.AddComponent<CanvasGroup>();

        googleButton = CreateButton(choices.transform, "GoogleButton", "Continue with Google", new Vector2(0f, 0.52f), new Vector2(1f, 1f), new Color(1f, 1f, 1f, 1f), new Color(0.25f, 0.25f, 0.25f, 1f));
        googleButton.onClick.AddListener(OnGoogleClicked);

        guestButton = CreateButton(choices.transform, "GuestButton", "Play as Guest", new Vector2(0f, 0f), new Vector2(1f, 0.48f), new Color(0.90f, 0.45f, 0.12f, 1f), Color.white);
        guestButton.onClick.AddListener(OnGuestClicked);

        statusText = CreateText(panel.transform, "Status", string.Empty, 24f, FontStyles.Normal, new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.22f));
        statusText.color = new Color(0.40f, 0.23f, 0.10f, 1f);
    }

    private void OnGoogleClicked()
    {
        if (isBusy)
        {
            return;
        }

        if (isUpdateBlocked)
        {
            StartCoroutine(RetryUpdateCheck());
            return;
        }

        SetBusy(true, "Signing in with Google...");
        authService.SignInGoogle((session, error) =>
        {
            if (session == null || !session.IsValid)
            {
                SetBusy(false, string.IsNullOrEmpty(error) ? "Google sign-in failed." : error);
                return;
            }

            StartCoroutine(LoadMainScene());
        });
    }

    private void OnGuestClicked()
    {
        if (isBusy)
        {
            return;
        }

        if (isUpdateBlocked)
        {
            StartCoroutine(RetryUpdateCheck());
            return;
        }

        SetBusy(true, "Starting as guest...");
        authService.SignInGuest((session, error) =>
        {
            if (session == null || !session.IsValid)
            {
                SetBusy(false, string.IsNullOrEmpty(error) ? "Guest sign-in failed." : error);
                return;
            }

            StartCoroutine(LoadMainScene());
        });
    }

    private IEnumerator RetryUpdateCheck()
    {
        yield return RunUpdateCheck();
        if (!isUpdateBlocked)
        {
            SetBusy(false, authService.HasCachedSession ? "Choose account to continue." : "Choose how to start.");
        }
    }

    private IEnumerator LoadMainScene()
    {
        SetBusy(true, "Loading game...");
        if (eventSystemObject != null)
        {
            Destroy(eventSystemObject);
            eventSystemObject = null;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Single);
        while (operation != null && !operation.isDone)
        {
            yield return null;
        }

        Destroy(gameObject);
    }

    private void SetBusy(bool busy, string message)
    {
        isBusy = busy;
        if (choiceGroup != null)
        {
            choiceGroup.interactable = !busy;
            choiceGroup.blocksRaycasts = !busy;
            choiceGroup.alpha = busy ? 0.55f : 1f;
        }

        if (googleButton != null) googleButton.interactable = !busy;
        if (guestButton != null) guestButton.interactable = !busy;
        if (statusText != null) statusText.text = message;

        if (!busy && isUpdateBlocked)
        {
            if (googleButton != null)
            {
                googleButton.interactable = true;
                SetButtonLabel(googleButton, "Update");
            }

            if (guestButton != null)
            {
                guestButton.interactable = false;
            }
        }
        else
        {
            if (googleButton != null) SetButtonLabel(googleButton, "Continue with Google");
            if (guestButton != null) SetButtonLabel(guestButton, "Play as Guest");
        }
    }

    private static GameObject CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return obj;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, FontStyles fontStyle, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = CreateRect(parent, name, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        TMP_Text label = obj.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(12f, fontSize * 0.55f);
        label.fontSizeMax = fontSize;
        return label;
    }

    private static Button CreateButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Color background, Color foreground)
    {
        GameObject obj = CreateRect(parent, name, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = obj.AddComponent<Image>();
        image.color = background;
        Button button = obj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = background;
        colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
        colors.pressedColor = Color.Lerp(background, Color.black, 0.10f);
        colors.disabledColor = new Color(background.r, background.g, background.b, 0.55f);
        button.colors = colors;

        TMP_Text label = CreateText(obj.transform, "Label", text, 32f, FontStyles.Bold, Vector2.zero, Vector2.one);
        label.color = foreground;
        return button;
    }

    private static void SetButtonLabel(Button button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = text;
        }
    }
}
