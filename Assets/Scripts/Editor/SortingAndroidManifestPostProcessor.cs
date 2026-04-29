using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

// Unity 6 에서 UnityPlayerActivity 가 제거되고 UnityPlayerGameActivity 로 대체됐다.
// launcher 매니페스트에 올바른 클래스명으로 LAUNCHER 인텐트 필터를 추가해 앱이 실행되도록 한다.
public class SortingAndroidManifestPostProcessor : IPostGenerateGradleAndroidProject
{
    private const string AndroidNs   = "http://schemas.android.com/apk/res/android";
    private const string ToolsNs     = "http://schemas.android.com/tools";
    private const string OldActivity = "com.unity3d.player.UnityPlayerActivity";
    private const string NewActivity = "com.unity3d.player.UnityPlayerGameActivity";

    public int callbackOrder => 99;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        // path = unityLibrary 디렉터리
        string launcherManifest = Path.GetFullPath(
            Path.Combine(path, "..", "launcher", "src", "main", "AndroidManifest.xml"));

        if (!File.Exists(launcherManifest))
        {
            Debug.LogError($"[ManifestFix] launcher manifest not found: {launcherManifest}");
            return;
        }

        PatchLauncherManifest(launcherManifest);
    }

    private static void PatchLauncherManifest(string manifestPath)
    {
        var doc = new XmlDocument();
        doc.Load(manifestPath);

        var nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("android", AndroidNs);
        nsMgr.AddNamespace("tools", ToolsNs);

        XmlNode application = doc.SelectSingleNode("/manifest/application");
        if (application == null)
        {
            Debug.LogError("[ManifestFix] <application> not found in launcher manifest.");
            return;
        }

        // 잘못된 UnityPlayerActivity 선언이 남아있으면 제거
        XmlNode oldNode = doc.SelectSingleNode(
            $"/manifest/application/activity[@android:name='{OldActivity}']", nsMgr);
        if (oldNode != null)
        {
            application.RemoveChild(oldNode);
            Debug.Log("[ManifestFix] Removed stale UnityPlayerActivity declaration.");
        }

        // 이미 올바르게 패치됐으면 스킵
        XmlNode existing = doc.SelectSingleNode(
            $"/manifest/application/activity[@android:name='{NewActivity}']", nsMgr);
        if (existing != null)
        {
            Debug.Log("[ManifestFix] Already patched with UnityPlayerGameActivity.");
            return;
        }

        // <activity android:name="UnityPlayerGameActivity" android:enabled="true"
        //           android:exported="true" tools:replace="android:enabled,android:exported">
        //   <intent-filter>
        //     <action android:name="android.intent.action.MAIN"/>
        //     <category android:name="android.intent.category.LAUNCHER"/>
        //   </intent-filter>
        // </activity>
        XmlElement activity = doc.CreateElement("activity");
        activity.Attributes.Append(Attr(doc, "name",     AndroidNs, NewActivity));
        activity.Attributes.Append(Attr(doc, "theme",    AndroidNs, "@style/BaseUnityGameActivityTheme"));
        activity.Attributes.Append(Attr(doc, "enabled",  AndroidNs, "true"));
        activity.Attributes.Append(Attr(doc, "exported", AndroidNs, "true"));
        activity.Attributes.Append(Attr(doc, "replace",  ToolsNs,   "android:enabled,android:exported,android:theme"));

        XmlElement intentFilter = doc.CreateElement("intent-filter");

        XmlElement action = doc.CreateElement("action");
        action.Attributes.Append(Attr(doc, "name", AndroidNs, "android.intent.action.MAIN"));
        intentFilter.AppendChild(action);

        XmlElement category = doc.CreateElement("category");
        category.Attributes.Append(Attr(doc, "name", AndroidNs, "android.intent.category.LAUNCHER"));
        intentFilter.AppendChild(category);

        activity.AppendChild(intentFilter);
        application.AppendChild(activity);

        // tools 네임스페이스 루트에 없으면 추가
        if (string.IsNullOrEmpty(doc.DocumentElement.GetAttribute("xmlns:tools")))
            doc.DocumentElement.SetAttribute("xmlns:tools", ToolsNs);

        doc.Save(manifestPath);
        Debug.Log($"[ManifestFix] Patched: UnityPlayerGameActivity + LAUNCHER intent-filter added.");
    }

    private static XmlAttribute Attr(XmlDocument doc, string localName, string ns, string value)
    {
        string prefix = ns == ToolsNs ? "tools" : "android";
        XmlAttribute attr = doc.CreateAttribute(prefix, localName, ns);
        attr.Value = value;
        return attr;
    }
}
