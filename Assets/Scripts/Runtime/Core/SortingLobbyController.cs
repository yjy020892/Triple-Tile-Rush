using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SortingLobbyController : MonoBehaviour
{
    public const string LobbySceneName = "Lobby";
    public const string MainSceneName = "Main";

    private static bool sceneLoadHooked;
    private TMP_Text themeText;
    private TMP_Text backgroundText;
    private CanvasGroup themeOverlay;
    private CanvasGroup themeContent;
    private CanvasGroup backgroundContent;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoadHook()
    {
        if (sceneLoadHooked)
        {
            return;
        }

        sceneLoadHooked = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallIfInitialLobbyScene()
    {
        InstallIfLobbyScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallIfLobbyScene(scene);
    }

    private static void InstallIfLobbyScene(Scene scene)
    {
        if (scene.name != LobbySceneName)
        {
            return;
        }

        if (FindObjectOfType<SortingLobbyController>() != null)
        {
            return;
        }

        new GameObject("SortingLobbyController").AddComponent<SortingLobbyController>();
    }

    private void Awake()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("LobbyEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        CacheUi();
        BindRuntimeButtons();
        RefreshThemeText();
        HideThemeOverlay();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(MainSceneName, LoadSceneMode.Single);
    }

    public void ShowThemeOverlay()
    {
        if (themeOverlay == null)
        {
            return;
        }

        themeOverlay.gameObject.SetActive(true);
        themeOverlay.alpha = 1f;
        themeOverlay.interactable = true;
        themeOverlay.blocksRaycasts = true;
    }

    public void HideThemeOverlay()
    {
        if (themeOverlay == null)
        {
            return;
        }

        themeOverlay.alpha = 0f;
        themeOverlay.interactable = false;
        themeOverlay.blocksRaycasts = false;
        themeOverlay.gameObject.SetActive(false);
    }

    public void SelectThemeByName(string themeName)
    {
        if (System.Enum.TryParse(themeName, true, out SortingTheme theme))
        {
            SortingThemeSettings.SelectedTheme = theme;
            RefreshSelectionTexts();
        }
    }

    public void SelectBackgroundByName(string backgroundName)
    {
        SortingThemeSettings.SelectedBackground = backgroundName;
        RefreshSelectionTexts();
    }

    public void ShowThemeTab() => ShowCustomizeTab(themeContent);
    public void ShowBackgroundTab() => ShowCustomizeTab(backgroundContent);

    private void CacheUi()
    {
        GameObject themeTextObject = FindSceneObject("SelectedThemeText");
        themeText = themeTextObject != null ? themeTextObject.GetComponent<TMP_Text>() : null;
        backgroundText = GetText("SelectedBackgroundText");

        GameObject overlayObject = FindSceneObject("ThemeOverlay");
        themeOverlay = overlayObject != null ? overlayObject.GetComponent<CanvasGroup>() : null;
        themeContent = GetCanvasGroup("ThemeContent");
        backgroundContent = GetCanvasGroup("BackgroundContent");
    }

    private void BindRuntimeButtons()
    {
        BindButton("StartButton", StartGame);
        BindButton("ThemeButton", ShowThemeOverlay);
        BindButton("CloseButton", HideThemeOverlay);
        BindButton("CustomizeThemeTabButton", ShowThemeTab);
        BindButton("CustomizeBackgroundTabButton", ShowBackgroundTab);

        foreach (SortingTheme theme in System.Enum.GetValues(typeof(SortingTheme)))
        {
            string themeName = theme.ToString();
            BindButton("Theme_" + themeName, () => SelectThemeByName(themeName));
        }

        foreach (string backgroundId in SortingLobbyCatalog.BackgroundIds)
        {
            string id = backgroundId;
            BindButton("Background_" + id, () => SelectBackgroundByName(id));
        }

    }

    private static void BindButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = FindSceneObject(objectName);
        Button button = obj != null ? obj.GetComponent<Button>() : null;
        if (button == null || action == null)
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    private void RefreshSelectionTexts()
    {
        if (themeText != null)
        {
            themeText.text = "Theme  " + SortingThemeSettings.SelectedTheme;
        }

        if (backgroundText != null)
        {
            backgroundText.text = "Background  " + SortingThemeSettings.SelectedBackground;
        }

    }

    private void RefreshThemeText()
    {
        RefreshSelectionTexts();
    }

    private void ShowCustomizeTab(CanvasGroup active)
    {
        SetContentVisible(themeContent, active == themeContent);
        SetContentVisible(backgroundContent, active == backgroundContent);
    }

    private static void SetContentVisible(CanvasGroup group, bool visible)
    {
        if (group == null)
        {
            return;
        }

        group.gameObject.SetActive(visible);
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private static TMP_Text GetText(string objectName)
    {
        GameObject obj = FindSceneObject(objectName);
        return obj != null ? obj.GetComponent<TMP_Text>() : null;
    }

    private static CanvasGroup GetCanvasGroup(string objectName)
    {
        GameObject obj = FindSceneObject(objectName);
        return obj != null ? obj.GetComponent<CanvasGroup>() : null;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindChildRecursive(roots[i].transform, objectName);
            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == objectName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
