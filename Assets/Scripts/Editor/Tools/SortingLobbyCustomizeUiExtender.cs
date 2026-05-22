using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SortingLobbyCustomizeUiExtender
{
    private const string LobbyScenePath = "Assets/Scenes/Lobby.unity";

    [MenuItem("Tools/Sorting Puzzle/Add Lobby Customize Tabs")]
    public static void AddLobbyCustomizeTabs()
    {
        Scene scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
        SortingLobbyController controller = Object.FindObjectOfType<SortingLobbyController>();
        if (controller == null)
        {
            GameObject controllerObject = new GameObject("SortingLobbyController");
            controller = controllerObject.AddComponent<SortingLobbyController>();
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("LobbyEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        Transform panel = FindTransform("ThemePanel");
        if (panel == null)
        {
            Debug.LogWarning("[Sorting] ThemePanel not found. Open Lobby scene after generating the base lobby UI.");
            return;
        }

        EnsureSummaryTexts();
        BuildTabs(panel, controller);
        BuildContents(panel, controller);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Sorting] Lobby customize tabs added.");
    }

    private static void EnsureSummaryTexts()
    {
        Transform levelScroll = FindTransform("LevelScroll");
        if (levelScroll == null)
        {
            return;
        }

        if (FindTransform("SelectedBackgroundText") == null)
        {
            TMP_Text text = CreateText(levelScroll, "SelectedBackgroundText", "Background  back1", 22f, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.24f));
            text.color = new Color(0.46f, 0.20f, 0.10f, 1f);
        }

    }

    private static void BuildTabs(Transform panel, SortingLobbyController controller)
    {
        Button theme = EnsureButton(panel, "CustomizeThemeTabButton", "Theme", new Vector2(0.04f, 0.75f), new Vector2(0.34f, 0.83f), new Color(0.74f, 0.22f, 0.10f, 1f));
        Button background = EnsureButton(panel, "CustomizeBackgroundTabButton", "Background", new Vector2(0.35f, 0.75f), new Vector2(0.65f, 0.83f), new Color(0.74f, 0.22f, 0.10f, 1f));

        Bind(theme, controller, nameof(SortingLobbyController.ShowThemeTab));
        Bind(background, controller, nameof(SortingLobbyController.ShowBackgroundTab));
    }

    private static void BuildContents(Transform panel, SortingLobbyController controller)
    {
        Transform themeContent = EnsureScrollContainer(panel, "ThemeContent");
        Transform backgroundContent = EnsureScrollContainer(panel, "BackgroundContent");
        Transform themeItems = EnsureScrollItems(themeContent);
        Transform backgroundItems = EnsureScrollItems(backgroundContent);

        MoveExistingThemeButtons(themeItems);
        LayoutThemeButtons(themeItems, controller);

        BuildIdButtons(backgroundItems, "Background_", SortingLobbyCatalog.BackgroundIds);

        SetContentVisible(themeContent, true);
        SetContentVisible(backgroundContent, false);
    }

    private static void MoveExistingThemeButtons(Transform themeContent)
    {
        foreach (SortingTheme theme in System.Enum.GetValues(typeof(SortingTheme)))
        {
            Transform existing = FindTransform("Theme_" + theme);
            if (existing != null && existing.parent != themeContent)
            {
                existing.SetParent(themeContent, false);
            }
        }
    }

    private static void LayoutThemeButtons(Transform parent, SortingLobbyController controller)
    {
        int index = 0;
        foreach (SortingTheme theme in System.Enum.GetValues(typeof(SortingTheme)))
        {
            Button button = EnsureButton(parent, "Theme_" + theme, theme.ToString(), GridMin(index), GridMax(index), ThemeColor(theme));
            button.onClick.RemoveAllListeners();
            EditorUtility.SetDirty(button);
            index++;
        }
    }

    private static void BuildIdButtons(Transform parent, string prefix, string[] ids)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i];
            Button button = EnsureButton(parent, prefix + id, id, GridMin(i), GridMax(i), BackgroundColor(i));
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(true);
            EditorUtility.SetDirty(button);
        }
    }

    private static Transform EnsureScrollContainer(Transform panel, string name)
    {
        Transform existing = FindTransform(name);
        if (existing != null)
        {
            EnsureScrollStructure(existing);
            return existing;
        }

        GameObject obj = CreateRect(panel, name, new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.73f));
        obj.AddComponent<CanvasGroup>();
        Image rootImage = obj.AddComponent<Image>();
        rootImage.color = new Color(0f, 0f, 0f, 0.01f);

        GameObject viewport = CreateRect(obj.transform, "Viewport", Vector2.zero, Vector2.one);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject items = CreateRect(viewport.transform, "Items", new Vector2(0f, 1f), new Vector2(1f, 1f));
        RectTransform itemsRect = items.GetComponent<RectTransform>();
        itemsRect.pivot = new Vector2(0.5f, 1f);
        itemsRect.anchoredPosition = Vector2.zero;
        itemsRect.sizeDelta = new Vector2(0f, 950f);

        ScrollRect scrollRect = obj.AddComponent<ScrollRect>();
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = itemsRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        return obj.transform;
    }

    private static Transform EnsureScrollItems(Transform container)
    {
        EnsureScrollStructure(container);
        Transform viewport = FindChildRecursive(container, "Viewport");
        Transform items = viewport != null ? FindChildRecursive(viewport, "Items") : null;
        if (items != null)
        {
            return items;
        }

        return container;
    }

    private static void EnsureScrollStructure(Transform container)
    {
        if (container == null)
        {
            return;
        }

        CanvasGroup group = container.GetComponent<CanvasGroup>();
        if (group == null)
        {
            container.gameObject.AddComponent<CanvasGroup>();
        }

        RectTransform containerRect = container as RectTransform;
        Transform viewport = FindChildRecursive(container, "Viewport");
        if (viewport == null)
        {
            GameObject viewportObject = CreateRect(container, "Viewport", Vector2.zero, Vector2.one);
            viewport = viewportObject.transform;
            Image viewportImage = viewportObject.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewportObject.AddComponent<Mask>().showMaskGraphic = false;
        }

        RectTransform viewportRect = viewport as RectTransform;
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Transform items = FindChildRecursive(viewport, "Items");
        if (items == null)
        {
            GameObject itemsObject = CreateRect(viewport, "Items", new Vector2(0f, 1f), new Vector2(1f, 1f));
            items = itemsObject.transform;
        }

        RectTransform itemsRect = items as RectTransform;
        itemsRect.anchorMin = new Vector2(0f, 1f);
        itemsRect.anchorMax = new Vector2(1f, 1f);
        itemsRect.pivot = new Vector2(0.5f, 1f);
        itemsRect.anchoredPosition = Vector2.zero;
        itemsRect.sizeDelta = new Vector2(0f, 950f);

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child != viewport &&
                (child.name.StartsWith("Background_", System.StringComparison.Ordinal) ||
                 child.name.StartsWith("Theme_", System.StringComparison.Ordinal)))
            {
                child.SetParent(items, false);
            }
        }

        ScrollRect scrollRect = container.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = container.gameObject.AddComponent<ScrollRect>();
        }

        scrollRect.viewport = viewportRect;
        scrollRect.content = itemsRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
    }

    private static Button EnsureButton(Transform parent, string name, string text, Vector2 min, Vector2 max, Color color)
    {
        Transform existing = FindChildRecursive(parent, name);
        if (existing == null)
        {
            GameObject obj = CreateRect(parent, name, min, max);
            Image image = obj.AddComponent<Image>();
            image.color = color;
            Button button = obj.AddComponent<Button>();
            TMP_Text label = CreateText(obj.transform, "Label", text, 28f, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.92f));
            label.color = Color.white;
            return button;
        }

        RectTransform rect = existing as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        TMP_Text existingText = existing.GetComponentInChildren<TMP_Text>();
        if (existingText != null)
        {
            existingText.text = text;
        }

        Image existingImage = existing.GetComponent<Image>();
        if (existingImage != null)
        {
            existingImage.color = color;
        }

        Button existingButton = existing.GetComponent<Button>();
        return existingButton != null ? existingButton : existing.gameObject.AddComponent<Button>();
    }

    private static void Bind(Button button, SortingLobbyController controller, string methodName)
    {
        button.onClick.RemoveAllListeners();
        System.Reflection.MethodInfo method = typeof(SortingLobbyController).GetMethod(methodName);
        UnityEventTools.AddPersistentListener(button.onClick, (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), controller, method));
        EditorUtility.SetDirty(button);
    }

    private static void SetContentVisible(Transform content, bool visible)
    {
        CanvasGroup group = content.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = content.gameObject.AddComponent<CanvasGroup>();
        }

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
        content.gameObject.SetActive(visible);
    }

    private static GameObject CreateRect(Transform parent, string name, Vector2 min, Vector2 max)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return obj;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, Vector2 min, Vector2 max)
    {
        GameObject obj = CreateRect(parent, name, min, max);
        TMP_Text label = obj.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        return label;
    }

    private static Vector2 GridMin(int index)
    {
        int row = index / 3;
        int col = index % 3;
        return new Vector2(0.02f + col * 0.33f, 0.70f - row * 0.24f);
    }

    private static Vector2 GridMax(int index)
    {
        int row = index / 3;
        int col = index % 3;
        return new Vector2(0.30f + col * 0.33f, 0.92f - row * 0.24f);
    }

    private static Transform FindTransform(string name)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform transform = transforms[i];
            if (transform == null || transform.name != name)
            {
                continue;
            }

            if (transform.gameObject.scene.path == LobbyScenePath)
            {
                return transform;
            }
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    private static Color ThemeColor(SortingTheme theme)
    {
        switch (theme)
        {
            case SortingTheme.Food: return new Color(0.90f, 0.38f, 0.12f, 1f);
            case SortingTheme.Sweet: return new Color(0.86f, 0.28f, 0.52f, 1f);
            case SortingTheme.Plant: return new Color(0.28f, 0.66f, 0.22f, 1f);
            case SortingTheme.Animal: return new Color(0.75f, 0.48f, 0.22f, 1f);
            case SortingTheme.Bug: return new Color(0.40f, 0.58f, 0.16f, 1f);
            case SortingTheme.Vehicle: return new Color(0.22f, 0.48f, 0.82f, 1f);
            case SortingTheme.Weather: return new Color(0.26f, 0.66f, 0.88f, 1f);
            case SortingTheme.Tool: return new Color(0.42f, 0.44f, 0.50f, 1f);
            case SortingTheme.Fantasy: return new Color(0.55f, 0.30f, 0.82f, 1f);
            default: return new Color(0.65f, 0.35f, 0.18f, 1f);
        }
    }

    private static Color BackgroundColor(int index)
    {
        Color[] colors =
        {
            new Color(0.75f, 0.48f, 0.25f, 1f),
            new Color(0.62f, 0.36f, 0.18f, 1f),
            new Color(0.22f, 0.66f, 0.82f, 1f),
            new Color(0.48f, 0.24f, 0.68f, 1f),
            new Color(0.66f, 0.52f, 0.30f, 1f)
        };
        return colors[index % colors.Length];
    }

}
