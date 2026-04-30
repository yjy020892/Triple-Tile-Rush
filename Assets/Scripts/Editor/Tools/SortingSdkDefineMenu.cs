using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

//
//
public static class SortingSdkDefineMenu
{
    private const string DefineAdMob    = "SORTING_ADMOB";
    private const string DefineFirebase = "SORTING_FIREBASE";

    [MenuItem("Tools/Sorting/Define Symbols/Toggle SORTING_ADMOB")]
    public static void ToggleAdMob() => Toggle(DefineAdMob);

    [MenuItem("Tools/Sorting/Define Symbols/Toggle SORTING_FIREBASE")]
    public static void ToggleFirebase() => Toggle(DefineFirebase);

    [MenuItem("Tools/Sorting/Define Symbols/Show Current")]
    public static void ShowCurrent()
    {
        string defines = GetCurrentDefines(out NamedBuildTarget target);
        Debug.Log($"[Sorting] {target.TargetName} defines: {defines}");
    }

    private static void Toggle(string symbol)
    {
        string defines = GetCurrentDefines(out NamedBuildTarget target);
        System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>(defines.Split(';'));
        list.RemoveAll(string.IsNullOrWhiteSpace);
        bool on;
        if (list.Contains(symbol))
        {
            list.Remove(symbol);
            on = false;
        }
        else
        {
            list.Add(symbol);
            on = true;
        }
        string joined = string.Join(";", list);
        PlayerSettings.SetScriptingDefineSymbols(target, joined);
        Debug.Log($"[Sorting] Define {symbol} = {(on ? "ON" : "OFF")}  ({target.TargetName})");
    }

    private static string GetCurrentDefines(out NamedBuildTarget target)
    {
        BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
        target = NamedBuildTarget.FromBuildTargetGroup(group);
        return PlayerSettings.GetScriptingDefineSymbols(target);
    }
}
