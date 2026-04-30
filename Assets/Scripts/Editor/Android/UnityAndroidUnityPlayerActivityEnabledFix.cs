using System.IO;
using UnityEditor.Android;

/// <summary>
/// </summary>
public class UnityAndroidUnityPlayerActivityEnabledFix : IPostGenerateGradleAndroidProject
{
    private const string UnityPlayerActivityFqn = "com.unity3d.player.UnityPlayerActivity";
    private const string NameMarker = "android:name=\"" + UnityPlayerActivityFqn + "\"";

    public int callbackOrder => 999;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var manifestPath = Path.Combine(path, "unityLibrary", "src", "main", "AndroidManifest.xml");
        if (!File.Exists(manifestPath))
            return;

        var xml = File.ReadAllText(manifestPath);
        if (!xml.Contains(NameMarker, System.StringComparison.Ordinal))
            return;

        if (!TryPatchOpeningActivityTag(ref xml, NameMarker))
            return;

        File.WriteAllText(manifestPath, xml);
    }

    private static bool TryPatchOpeningActivityTag(ref string xml, string marker)
    {
        int nameIdx = xml.IndexOf(marker, System.StringComparison.Ordinal);
        if (nameIdx < 0)
            return false;

        int open = xml.LastIndexOf("<activity", nameIdx, System.StringComparison.Ordinal);
        if (open < 0)
            return false;

        int openEnd = IndexOfOpeningTagClosingAngleBracket(xml, open + "<activity".Length);
        if (openEnd < 0)
            return false;

        string openTag = xml.Substring(open, openEnd - open + 1);
        string patchedOpen = ReplaceFirst(openTag, "android:enabled=\"false\"", "android:enabled=\"true\"");
        if (patchedOpen == openTag)
            return false;

        xml = xml.Substring(0, open) + patchedOpen + xml.Substring(openEnd + 1);
        return true;
    }

    private static int IndexOfOpeningTagClosingAngleBracket(string s, int startFrom)
    {
        bool inDq = false;
        for (int i = startFrom; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '"')
                inDq = !inDq;
            else if (c == '>' && !inDq)
                return i;
        }
        return -1;
    }

    private static string ReplaceFirst(string haystack, string needle, string insert)
    {
        int idx = haystack.IndexOf(needle, System.StringComparison.Ordinal);
        if (idx < 0)
            return haystack;
        return haystack.Substring(0, idx) + insert + haystack.Substring(idx + needle.Length);
    }
}
