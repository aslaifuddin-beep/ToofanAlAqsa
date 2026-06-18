using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildSetup : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;

    public void OnPreprocessBuild(BuildReport report)
    {
        EnsureSceneHasRequiredComponents();
    }

    static void EnsureSceneHasRequiredComponents()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "GameManager" && root.GetComponent<GameManager>() == null)
                root.AddComponent<GameManager>();

            if (root.name == "PlayerController")
            {
                if (root.GetComponent<CharacterController>() == null)
                    root.AddComponent<CharacterController>();
                if (root.GetComponent<FPSController>() == null)
                    root.AddComponent<FPSController>();
                if (root.GetComponent<WeaponManager>() == null)
                    root.AddComponent<WeaponManager>();
                if (root.GetComponent<HealthSystem>() == null)
                    root.AddComponent<HealthSystem>();
            }

            if (root.name == "Main Camera")
            {
                if (root.GetComponent<CameraEffects>() == null)
                    root.AddComponent<CameraEffects>();
                if (root.GetComponent<AudioReverbFilter>() == null)
                    root.AddComponent<AudioReverbFilter>();
            }

            if (root.name == "MissionManager" && root.GetComponent<MissionManager>() == null)
                root.AddComponent<MissionManager>();

            if (root.name == "AudioManager" && root.GetComponent<AudioManager>() == null)
                root.AddComponent<AudioManager>();

            if (root.name == "UIManager" && root.GetComponent<UIManager>() == null)
                root.AddComponent<UIManager>();

            if (root.name == "UpgradeSystem" && root.GetComponent<UpgradeSystem>() == null)
                root.AddComponent<UpgradeSystem>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
