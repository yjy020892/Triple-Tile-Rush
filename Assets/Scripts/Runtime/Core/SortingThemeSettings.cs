using UnityEngine;

public static class SortingThemeSettings
{
    private const string SelectedThemeKey = "SortingPuzzle_SelectedTheme";
    private const string SelectedBackgroundKey = "SortingPuzzle_SelectedBackground";

    public static SortingTheme SelectedTheme
    {
        get
        {
            string raw = PlayerPrefs.GetString(SelectedThemeKey, SortingTheme.Food.ToString());
            return System.Enum.TryParse(raw, true, out SortingTheme theme) ? theme : SortingTheme.Food;
        }
        set
        {
            PlayerPrefs.SetString(SelectedThemeKey, value.ToString());
            PlayerPrefs.Save();
        }
    }

    public static string SelectedBackground
    {
        get => PlayerPrefs.GetString(SelectedBackgroundKey, SortingLobbyCatalog.BackgroundIds[0]);
        set
        {
            PlayerPrefs.SetString(SelectedBackgroundKey, string.IsNullOrEmpty(value) ? SortingLobbyCatalog.BackgroundIds[0] : value);
            PlayerPrefs.Save();
        }
    }

}

public static class SortingLobbyCatalog
{
    public static readonly string[] BackgroundIds =
    {
        "back1",
        "back2",
        "back3",
        "back4"
    };

}
