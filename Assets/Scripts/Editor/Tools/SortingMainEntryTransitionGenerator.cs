using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SortingMainEntryTransitionGenerator
{
    private const string MainScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("Tools/Sorting Puzzle/Add Main Entry Clouds")]
    public static void AddMainEntryClouds()
    {
        Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        Transform existing = FindTransform("MainEntryCloudTransition");
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("[Sorting] Main entry cloud transition already exists. Keeping the scene version.");
            return;
        }

        Canvas canvas = FindCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[Sorting] Main Canvas not found.");
            return;
        }

        RectTransform overlay = CreateRect(canvas.transform, "MainEntryCloudTransition", Vector2.zero, Vector2.one);
        overlay.SetAsLastSibling();
        Image veil = overlay.gameObject.AddComponent<Image>();
        veil.color = new Color(0.82f, 0.95f, 1f, 1f);
        veil.raycastTarget = true;
        CanvasGroup group = overlay.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = false;
        overlay.gameObject.AddComponent<SortingMainEntryTransition>();

        RectTransform centerGlow = CreatePanel(overlay, "CenterGlow",
            new Vector2(0.12f, 0.18f), new Vector2(0.88f, 0.82f),
            new Color(1f, 1f, 1f, 0.38f));
        ApplyRoundedSprite(centerGlow.GetComponent<Image>());

        RectTransform leftBank = CreateRect(overlay, "LeftCloudBank", new Vector2(0f, 0f), new Vector2(0.58f, 1f));
        RectTransform rightBank = CreateRect(overlay, "RightCloudBank", new Vector2(0.42f, 0f), new Vector2(1f, 1f));
        BuildCloudBank(leftBank, false);
        BuildCloudBank(rightBank, true);

        EditorUtility.SetDirty(overlay.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = overlay.gameObject;
        Debug.Log("[Sorting] Main entry cloud transition added to Main scene.");
    }

    private static void BuildCloudBank(RectTransform parent, bool mirror)
    {
        CreateCloud(parent, "CloudTop", mirror ? 0.34f : 0.02f, 0.76f, 0.66f, 0.28f);
        CreateCloud(parent, "CloudUpper", mirror ? 0.02f : 0.20f, 0.58f, 0.72f, 0.30f);
        CreateCloud(parent, "CloudMiddle", mirror ? 0.20f : -0.04f, 0.35f, 0.78f, 0.34f);
        CreateCloud(parent, "CloudLower", mirror ? 0.00f : 0.18f, 0.10f, 0.74f, 0.32f);
    }

    private static void CreateCloud(RectTransform parent, string name, float x, float y, float width, float height)
    {
        RectTransform cloud = CreateRect(parent, name, new Vector2(x, y), new Vector2(x + width, y + height));
        CreatePuff(cloud, "PuffLarge", new Vector2(0.12f, 0.08f), new Vector2(0.66f, 0.96f));
        CreatePuff(cloud, "PuffHigh", new Vector2(0.36f, 0.20f), new Vector2(0.90f, 1.06f));
        CreatePuff(cloud, "PuffWide", new Vector2(0.00f, 0.00f), new Vector2(1.00f, 0.62f));
    }

    private static void CreatePuff(RectTransform parent, string name, Vector2 min, Vector2 max)
    {
        RectTransform puff = CreatePanel(parent, name, min, max, new Color(1f, 1f, 1f, 0.98f));
        Image image = puff.GetComponent<Image>();
        ApplyRoundedSprite(image);
        image.preserveAspect = false;
        image.raycastTarget = false;
    }

    private static RectTransform CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
    {
        RectTransform rect = CreateRect(parent, name, min, max);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static void ApplyRoundedSprite(Image image)
    {
        if (image == null)
        {
            return;
        }

        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null)
        {
            return;
        }

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
    }

    private static RectTransform CreateRect(Transform parent, string name, Vector2 min, Vector2 max)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static Canvas FindCanvas()
    {
        Transform canvas = FindTransform("Canvas");
        return canvas != null ? canvas.GetComponent<Canvas>() : null;
    }

    private static Transform FindTransform(string name)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform transform = transforms[i];
            if (transform != null && transform.name == name && transform.gameObject.scene.path == MainScenePath)
            {
                return transform;
            }
        }

        return null;
    }
}
