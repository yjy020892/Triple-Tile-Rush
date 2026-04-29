using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// SDK define 심볼을 메뉴에서 켜고 끌 수 있게 하는 편의 도구.
//
// 사용:
//   Tools > Sorting > Define Symbols > (Toggle 메뉴)
//
// 각 심볼 상세:
//   - SORTING_ADMOB    : Google Mobile Ads SDK 가 import 된 상태에서 켤 것
//   - SORTING_FIREBASE : Firebase Analytics Unity SDK 가 import 된 상태에서 켤 것
//   - UNITY_PURCHASING : com.unity.purchasing 패키지가 설치되면 Unity 가 자동으로 켠다 (수동 설정 불필요)
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
