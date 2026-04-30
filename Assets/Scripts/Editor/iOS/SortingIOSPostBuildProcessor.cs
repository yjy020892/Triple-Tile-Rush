#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

//
//   - https://developers.google.com/admob/ios/quick-start
//   - https://developers.google.com/admob/ios/ios14
public static class SortingIOSPostBuildProcessor
{
    private static readonly string[] SkAdNetworkIds = new string[]
    {
        "cstr6suwn9.skadnetwork", // Google / AdMob
        "4fzdc2evr5.skadnetwork",
        "4pfyvq9l8r.skadnetwork",
        "2fnua5tdw4.skadnetwork",
        "ydx93a7ass.skadnetwork",
        "5a6flpkh64.skadnetwork",
        "p78axxw29g.skadnetwork",
        "v72qych5uu.skadnetwork",
        "c6k4g5qg8m.skadnetwork",
        "s39g8k73mm.skadnetwork",
        "3qy4746246.skadnetwork",
        "3sh42y64q3.skadnetwork",
        "f38h382jlk.skadnetwork",
        "hs6bdukanm.skadnetwork",
        "mlmmfzh3r3.skadnetwork",
        "prcb7njmu6.skadnetwork",
        "v4nxqhlyqp.skadnetwork",
        "wzmmz9fp6w.skadnetwork",
        "yclnxrl5pm.skadnetwork",
        "t38b2kh725.skadnetwork",
        "7ug5zh24hu.skadnetwork",
        "gta9lk7p23.skadnetwork",
        "vutu7akeur.skadnetwork",
        "y5ghdn5j9k.skadnetwork",
        "n6fk4nfna4.skadnetwork",
        "v9wttpbfk9.skadnetwork",
        "n38lu8286q.skadnetwork",
        "47vhws6wlr.skadnetwork",
        "kbd757ywx3.skadnetwork",
        "9t245vhmpl.skadnetwork",
    };

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict root = plist.root;

        root.SetString("GADApplicationIdentifier", "ca-app-pub-3940256099942544~1458002511");

        root.SetString("NSUserTrackingUsageDescription",
            "We use your advertising identifier to show relevant ads and measure campaign performance.");

        root.SetBoolean("GADDelayAppMeasurementInit", true);

        PlistElementArray skArray = root.values.ContainsKey("SKAdNetworkItems")
            ? root["SKAdNetworkItems"].AsArray()
            : root.CreateArray("SKAdNetworkItems");

        var existing = new System.Collections.Generic.HashSet<string>();
        foreach (PlistElement el in skArray.values)
        {
            PlistElementDict dict = el.AsDict();
            PlistElement id;
            if (dict.values.TryGetValue("SKAdNetworkIdentifier", out id))
            {
                existing.Add(id.AsString());
            }
        }
        foreach (string id in SkAdNetworkIds)
        {
            if (existing.Contains(id)) continue;
            PlistElementDict dict = skArray.AddDict();
            dict.SetString("SKAdNetworkIdentifier", id);
        }

        plist.WriteToFile(plistPath);

        Debug.Log("[SortingIOSPostBuild] Info.plist patched for AdMob / ATT / SKAdNetwork.");
    }
}
#endif
