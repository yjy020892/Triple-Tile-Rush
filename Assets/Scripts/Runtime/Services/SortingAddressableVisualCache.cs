using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class SortingAddressableVisualCache
{
    private static readonly Dictionary<string, Sprite> backgroundById = new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, Sprite> tileBackById = new Dictionary<string, Sprite>();
    private static readonly Dictionary<SortingTheme, Dictionary<string, Sprite>> iconByTheme = new Dictionary<SortingTheme, Dictionary<string, Sprite>>();
    private static readonly List<AsyncOperationHandle> retainedHandles = new List<AsyncOperationHandle>();
    private static readonly HashSet<string> loadingBackgrounds = new HashSet<string>();
    private static readonly HashSet<string> loadingTileBacks = new HashSet<string>();
    private static readonly HashSet<SortingTheme> loadingThemes = new HashSet<SortingTheme>();

    public static IEnumerator PreloadForLevel(SortingTheme theme, string backgroundId)
    {
        if (!IsBackgroundReady(backgroundId)) yield return LoadBackground(backgroundId);
        if (!IsTileBackReady("orange")) yield return LoadTileBack("orange");
        if (!IsTileBackReady("purple")) yield return LoadTileBack("purple");
        if (!IsTileBackReady("blue")) yield return LoadTileBack("blue");
        if (!IsThemeReady(theme)) yield return LoadThemeIcons(theme);
    }

    public static IEnumerator PreloadBackground(string backgroundId)
    {
        if (!IsBackgroundReady(backgroundId))
        {
            yield return LoadBackground(backgroundId);
        }
    }

    public static IEnumerator PreloadTheme(SortingTheme theme)
    {
        if (!IsThemeReady(theme))
        {
            yield return LoadThemeIcons(theme);
        }
    }

    public static bool IsLevelReady(SortingTheme theme, string backgroundId)
    {
        return IsBackgroundReady(backgroundId) &&
               IsTileBackReady("orange") &&
               IsTileBackReady("purple") &&
               IsTileBackReady("blue") &&
               IsThemeReady(theme);
    }

    public static Sprite GetBackground(string backgroundId)
    {
        return !string.IsNullOrEmpty(backgroundId) && backgroundById.TryGetValue(backgroundId, out Sprite sprite)
            ? sprite
            : null;
    }

    public static Sprite GetTileBack(string tileName)
    {
        string id = NormalizeTileBackId(tileName);
        return !string.IsNullOrEmpty(id) && tileBackById.TryGetValue(id, out Sprite sprite)
            ? sprite
            : null;
    }

    public static Sprite GetThemeIcon(SortingTheme theme, int iconIndex)
    {
        if (iconIndex <= 0 || !iconByTheme.TryGetValue(theme, out Dictionary<string, Sprite> sprites))
        {
            return null;
        }

        string name = "tileicon_" + theme.ToString().ToLowerInvariant() + iconIndex;
        return sprites.TryGetValue(name, out Sprite sprite) ? sprite : null;
    }

    private static IEnumerator LoadBackground(string id)
    {
        if (string.IsNullOrEmpty(id) || backgroundById.ContainsKey(id))
        {
            yield break;
        }

        while (loadingBackgrounds.Contains(id))
        {
            yield return null;
        }
        if (backgroundById.ContainsKey(id))
        {
            yield break;
        }

        loadingBackgrounds.Add(id);
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>("background/" + id);
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
        {
            backgroundById[id] = handle.Result;
            retainedHandles.Add(handle);
        }
        else
        {
            Addressables.Release(handle);
            Debug.LogWarning("[Addressables] Background load failed: " + id);
        }
        loadingBackgrounds.Remove(id);
    }

    private static IEnumerator LoadTileBack(string id)
    {
        if (string.IsNullOrEmpty(id) || tileBackById.ContainsKey(id))
        {
            yield break;
        }

        while (loadingTileBacks.Contains(id))
        {
            yield return null;
        }
        if (tileBackById.ContainsKey(id))
        {
            yield break;
        }

        loadingTileBacks.Add(id);
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>("tileback/" + id);
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
        {
            tileBackById[id] = handle.Result;
            retainedHandles.Add(handle);
        }
        else
        {
            Addressables.Release(handle);
            Debug.LogWarning("[Addressables] Tile back load failed: " + id);
        }
        loadingTileBacks.Remove(id);
    }

    private static IEnumerator LoadThemeIcons(SortingTheme theme)
    {
        if (iconByTheme.ContainsKey(theme))
        {
            yield break;
        }

        while (loadingThemes.Contains(theme))
        {
            yield return null;
        }
        if (iconByTheme.ContainsKey(theme))
        {
            yield break;
        }

        loadingThemes.Add(theme);
        string label = "theme_" + theme.ToString().ToLowerInvariant();
        AsyncOperationHandle<IList<Sprite>> handle = Addressables.LoadAssetsAsync<Sprite>(label, null);
        yield return handle;
        if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null || handle.Result.Count == 0)
        {
            Addressables.Release(handle);
            Debug.LogWarning("[Addressables] Theme icon load failed: " + label);
            loadingThemes.Remove(theme);
            yield break;
        }

        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        for (int i = 0; i < handle.Result.Count; i++)
        {
            Sprite sprite = handle.Result[i];
            if (sprite != null && !sprites.ContainsKey(sprite.name))
            {
                sprites[sprite.name] = sprite;
            }
        }

        iconByTheme[theme] = sprites;
        retainedHandles.Add(handle);
        loadingThemes.Remove(theme);
    }

    private static bool IsBackgroundReady(string id)
    {
        return !string.IsNullOrEmpty(id) && backgroundById.ContainsKey(id);
    }

    private static bool IsTileBackReady(string id)
    {
        return !string.IsNullOrEmpty(id) && tileBackById.ContainsKey(id);
    }

    private static bool IsThemeReady(SortingTheme theme)
    {
        return iconByTheme.ContainsKey(theme);
    }

    private static string NormalizeTileBackId(string tileName)
    {
        if (string.IsNullOrEmpty(tileName))
        {
            return string.Empty;
        }

        const string prefix = "tile_";
        return tileName.StartsWith(prefix, System.StringComparison.Ordinal)
            ? tileName.Substring(prefix.Length)
            : tileName;
    }
}
