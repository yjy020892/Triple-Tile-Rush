using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

[InitializeOnLoad]
public static class SortingProjectBootstrap
{
    private const string MainScenePath = "Assets/Scenes/Main.unity";

    static SortingProjectBootstrap()
    {
        EditorApplication.delayCall += EnsureProject;
    }

    private static void EnsureProject()
    {
        EnsureScene();
        EnsureBuildSettings();
        EnsurePlayerSettings();
    }

    [MenuItem("Tools/Sorting Puzzle/Open Main Scene")]
    public static void OpenMainScene()
    {
        EnsureProject();
        EditorSceneManager.OpenScene(MainScenePath);
    }

    [MenuItem("Tools/Sorting Puzzle/Reset Save Data")]
    public static void ResetSaveData()
    {
        PlayerPrefs.DeleteKey("SortingPuzzle_Level");
        PlayerPrefs.DeleteKey("SortingPuzzle_Coin");
        PlayerPrefs.Save();
        Debug.Log("Sorting Puzzle save data reset complete.");

        SortingGameController controller = Object.FindObjectOfType<SortingGameController>();
        if (controller != null)
        {
            controller.ResetProgressAndRestart();
        }
    }

    [MenuItem("Tools/Sorting Puzzle/Set Stage/Stage 1")]
    public static void SetStage1() => SetStageFromMenu(1);
    [MenuItem("Tools/Sorting Puzzle/Set Stage/Stage 5")]
    public static void SetStage5() => SetStageFromMenu(5);
    [MenuItem("Tools/Sorting Puzzle/Set Stage/Stage 8")]
    public static void SetStage8() => SetStageFromMenu(8);
    [MenuItem("Tools/Sorting Puzzle/Set Stage/Stage 10")]
    public static void SetStage10() => SetStageFromMenu(10);
    [MenuItem("Tools/Sorting Puzzle/Set Stage/Stage 12")]
    public static void SetStage12() => SetStageFromMenu(12);
    [MenuItem("Tools/Sorting Puzzle/Set Stage/Stage 20")]
    public static void SetStage20() => SetStageFromMenu(20);

    private static void SetStageFromMenu(int stage)
    {
        PlayerPrefs.SetInt("SortingPuzzle_Level", Mathf.Max(1, stage));
        PlayerPrefs.Save();
        Debug.Log($"Sorting Puzzle stage set to {stage}.");

        SortingGameController controller = Object.FindObjectOfType<SortingGameController>();
        if (controller != null)
        {
            controller.SetStageAndRestart(stage, controller.GetCurrentCoin());
        }
    }

    private static void EnsureScene()
    {
        string sceneDirectory = Path.GetDirectoryName(MainScenePath);
        if (!Directory.Exists(sceneDirectory))
        {
            Directory.CreateDirectory(sceneDirectory);
            AssetDatabase.Refresh();
        }

        if (File.Exists(MainScenePath))
        {
            return;
        }

        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.16f, 0.20f, 0.31f, 1f);
        camera.orthographic = true;
        camera.nearClipPlane = -10f;
        camera.farClipPlane = 100f;
        camera.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();

        GameObject bootstrapObject = new GameObject("SortingRuntimeBootstrap");
        bootstrapObject.AddComponent<SortingRuntimeBootstrap>();

        EditorSceneManager.SaveScene(newScene, MainScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Sorting Puzzle main scene created.");
    }

    private static void EnsureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainScenePath, true)
        };
    }

    private static void EnsurePlayerSettings()
    {
        // 릴스 제품/패키지 — 에디터마다 잘못된 값으로 덮어쓰지 않도록 Get 후 동일할 때만 생략하지 않고,
        // 부트스트랩 보장 값으로 고정( Play Console / AdMob / Firebase 와 일치시킬 것 ).
        const string bundleIdTripleMatchKing = "com.gameislife.triplematchking";
        PlayerSettings.companyName = "GameIsLife";
        PlayerSettings.productName = "Triple Tile Rush";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, bundleIdTripleMatchKing);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleIdTripleMatchKing);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.bundleVersion = "0.0.2";
        PlayerSettings.Android.bundleVersionCode = 2;
        PlayerSettings.Android.minifyDebug = false;
        PlayerSettings.Android.minifyRelease = true;
        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
        EditorUserBuildSettings.androidCreateSymbolsZip = true;
        EditorUserBuildSettings.buildAppBundle = true;
    }
}

public class SortingDebugToolsWindow : EditorWindow
{
    private int stageValue = 1;
    private int coinValue = 0;

    [MenuItem("Tools/Sorting Puzzle/Debug Tools")]
    public static void OpenWindow()
    {
        GetWindow<SortingDebugToolsWindow>("Sorting Debug Tools");
    }

    private void OnGUI()
    {
        GUILayout.Space(8f);
        EditorGUILayout.LabelField("Save / Stage Debug", EditorStyles.boldLabel);
        stageValue = EditorGUILayout.IntField("Stage", Mathf.Max(1, stageValue));
        coinValue = EditorGUILayout.IntField("Coin", Mathf.Max(0, coinValue));

        GUILayout.Space(8f);
        if (GUILayout.Button("Reset Save To Stage 1 / Coin 0"))
        {
            ApplySaveData(1, 0);
        }

        if (GUILayout.Button("Apply Stage / Coin And Reload"))
        {
            ApplySaveData(Mathf.Max(1, stageValue), Mathf.Max(0, coinValue));
        }

        GUILayout.Space(12f);
        EditorGUILayout.HelpBox("Play Mode hotkeys: R = reset to stage 1, [ = previous stage, ] = next stage", MessageType.Info);
    }

    private static void ApplySaveData(int stage, int coin)
    {
        PlayerPrefs.SetInt("SortingPuzzle_Level", Mathf.Max(1, stage));
        PlayerPrefs.SetInt("SortingPuzzle_Coin", Mathf.Max(0, coin));
        PlayerPrefs.Save();
        Debug.Log($"Sorting Puzzle save set. Stage={stage}, Coin={coin}");

        SortingGameController controller = Object.FindObjectOfType<SortingGameController>();
        if (controller != null)
        {
            controller.SetStageAndRestart(stage, coin);
        }
    }
}

public static class SortingBuildMenu
{
    [MenuItem("Build/Sorting Puzzle/Build Android AAB")]
    public static void BuildAndroidAab()
    {
        SortingProjectBootstrap.OpenMainScene();
        string buildDirectory = "Builds/Android/aab";
        Directory.CreateDirectory(buildDirectory);

        EditorUserBuildSettings.buildAppBundle = true;
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            target = BuildTarget.Android,
            locationPathName = Path.Combine(buildDirectory, "SortingPuzzleMVP.aab"),
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            CopyLatestMappingFile(buildDirectory, "SortingPuzzleMVP-mapping.txt");
        }
        Debug.Log($"Android AAB build result: {report.summary.result}  →  {buildDirectory}");
    }

    [MenuItem("Build/Sorting Puzzle/Build Android APK")]
    public static void BuildAndroidApk()
    {
        SortingProjectBootstrap.OpenMainScene();
        string buildDirectory = "Builds/Android/apk";
        Directory.CreateDirectory(buildDirectory);

        EditorUserBuildSettings.buildAppBundle = false;
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            target = BuildTarget.Android,
            locationPathName = Path.Combine(buildDirectory, "SortingPuzzleMVP.apk"),
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            CopyLatestMappingFile(buildDirectory, "SortingPuzzleMVP-mapping.txt");
        }
        Debug.Log($"Android APK build result: {report.summary.result}  →  {buildDirectory}");
    }

    private static void CopyLatestMappingFile(string buildDirectory, string outputName)
    {
        string gradleRoot = Path.Combine("Library", "Bee", "Android", "Prj", "IL2CPP", "Gradle");
        if (!Directory.Exists(gradleRoot))
        {
            return;
        }

        string latestPath = null;
        System.DateTime latestWriteTime = System.DateTime.MinValue;
        foreach (string path in Directory.GetFiles(gradleRoot, "mapping.txt", SearchOption.AllDirectories))
        {
            System.DateTime writeTime = File.GetLastWriteTimeUtc(path);
            if (writeTime > latestWriteTime)
            {
                latestWriteTime = writeTime;
                latestPath = path;
            }
        }

        if (string.IsNullOrEmpty(latestPath))
        {
            return;
        }

        Directory.CreateDirectory(buildDirectory);
        string outputPath = Path.Combine(buildDirectory, outputName);
        File.Copy(latestPath, outputPath, true);
        Debug.Log($"Android R8 mapping copied: {outputPath}");
    }
}
