using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;

public class BuildHelper : EditorWindow
{
    private string apkName = "ToofanAlAqsa_FPS.apk";
    private BuildTarget buildTarget = BuildTarget.Android;
    private bool developmentBuild = false;

    [MenuItem("Tools/Build Android APK")]
    public static void ShowWindow()
    {
        GetWindow<BuildHelper>("Build Android APK");
    }

    void OnGUI()
    {
        GUILayout.Label("Android Build Settings", EditorStyles.boldLabel);
        apkName = EditorGUILayout.TextField("APK Name", apkName);
        developmentBuild = EditorGUILayout.Toggle("Development Build", developmentBuild);

        GUILayout.Space(10);
        if (GUILayout.Button("Build APK", GUILayout.Height(40)))
            PerformBuild(apkName, developmentBuild ? BuildOptions.Development : BuildOptions.None);

        if (GUILayout.Button("Build APK (Development)", GUILayout.Height(30)))
            PerformBuild(apkName, BuildOptions.Development);

        GUILayout.Space(15);
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Set Package Name in Player Settings");
        GUILayout.Label("2. Configure Keystore for signing");
        GUILayout.Label("3. Click Build APK below");
    }

    void PerformBuild(string name, BuildOptions options)
    {
        string result = CI_BuildAndroid(name, options);
        if (result != null)
        {
            EditorUtility.DisplayDialog("Success", $"APK built: {result}", "OK");
            System.Diagnostics.Process.Start(Path.GetDirectoryName(result));
        }
        else
        {
            EditorUtility.DisplayDialog("Failed", "Build failed. Check Console.", "OK");
        }
    }

    public static string CI_BuildAndroid(string apkName = "ToofanAlAqsa_FPS.apk", BuildOptions options = BuildOptions.None)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            string[] foundScenes = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
            if (foundScenes.Length > 0)
            {
                scenes = foundScenes.Select(s => "Assets" + s.Replace(Application.dataPath, "").Replace("\\", "/")).ToArray();
                Debug.Log($"Found {scenes.Length} scene(s): {string.Join(", ", scenes)}");
            }
            else
            {
                Debug.LogError("No scenes found in project! Create a scene first.");
                return null;
            }
        }

        string outputPath = Path.Combine(Application.dataPath, "..", "Builds", apkName);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            targetGroup = BuildTargetGroup.Android,
            target = BuildTarget.Android,
            options = options
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == BuildResult.Succeeded)
        {
            long size = new FileInfo(outputPath).Length;
            Debug.Log($"APK built successfully: {outputPath} ({size / (1024f * 1024f):F2} MB)");
            return outputPath;
        }
        else
        {
            Debug.LogError($"Build failed with {report.summary.totalErrors} errors.");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError(msg.content);
                }
            }
            return null;
        }
    }

    public static void CI_Build()
    {
        CI_BuildAndroid("ToofanAlAqsa_FPS.apk", BuildOptions.None);
    }

    public static void CI_BuildDevelopment()
    {
        CI_BuildAndroid("ToofanAlAqsa_FPS_Dev.apk", BuildOptions.Development);
    }
}
