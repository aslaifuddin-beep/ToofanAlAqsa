using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using System.IO;
using System.Linq;

public class BuildSetup : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;

    public void OnPreprocessBuild(BuildReport report)
    {
        EnsureURPConfig();
        EnsureSceneHasRequiredComponents();
        EnsurePlayerSettings();
    }

    static void EnsureURPConfig()
    {
        var pipelineType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
        if (pipelineType == null)
        {
            Debug.LogError("URP package not installed. Cannot configure rendering.");
            return;
        }

        string urpPath = "Assets/Settings/URP_HighQuality.asset";
        var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(urpPath);

        if (pipeline == null)
        {
            Directory.CreateDirectory("Assets/Settings");
            pipeline = ScriptableObject.CreateInstance(pipelineType) as RenderPipelineAsset;
            if (pipeline != null)
            {
                AssetDatabase.CreateAsset(pipeline, urpPath);
                Debug.Log("Created fresh URP asset.");
            }
        }

        if (pipeline != null)
        {
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            AssetDatabase.SaveAssets();
        }
    }

    static void EnsureSceneHasRequiredComponents()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "GameManager" && root.GetComponent<GameManager>() == null)
                root.AddComponent<GameManager>();

            if (root.name == "PlayerController" || root.name == "Player")
            {
                if (root.GetComponent<CharacterController>() == null)
                    root.AddComponent<CharacterController>();
                if (root.GetComponent<FPSController>() == null)
                    root.AddComponent<FPSController>();
                if (root.GetComponent<WeaponManager>() == null)
                    root.AddComponent<WeaponManager>();
                if (root.GetComponent<HealthSystem>() == null)
                    root.AddComponent<HealthSystem>();
                if (root.GetComponent<UpgradeSystem>() == null)
                    root.AddComponent<UpgradeSystem>();
            }

            if (root.name == "Main Camera")
            {
                if (root.GetComponent<CameraEffects>() == null)
                    root.AddComponent<CameraEffects>();
            }

            if (root.name == "MissionManager" && root.GetComponent<MissionManager>() == null)
                root.AddComponent<MissionManager>();

            if (root.name == "AudioManager" && root.GetComponent<AudioManager>() == null)
                root.AddComponent<AudioManager>();

            if (root.name == "UIManager" && root.GetComponent<UIManager>() == null)
                root.AddComponent<UIManager>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void EnsurePlayerSettings()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);

        PlayerSettings.applicationIdentifier = "com.qassam.toofanalaqsa";
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;

        Debug.Log("Player settings configured for Android.");
    }
}
