using UnityEngine;
using UnityEditor;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading;

public class ModelDownloader : EditorWindow
{
    private List<ModelEntry> models = new List<ModelEntry>
    {
        new ModelEntry { name = "Tank_Enemy", url = "https://studio.tripo3d.ai/3d-model/b87703d7-fb77-47cc-a0fb-e16531d98049", savePath = "Assets/Resources/Models/Enemies/Tank_Enemy.glb" },
        new ModelEntry { name = "AK47_Weapon", url = "https://studio.tripo3d.ai/3d-model/4b46e833-58ef-4d99-b889-882c777c1d92", savePath = "Assets/Resources/Models/Weapons/AK47_Weapon.glb" },
        new ModelEntry { name = "Rifle_Scoped", url = "https://studio.tripo3d.ai/3d-model/297e5939-9d5a-45a4-b0be-81ca0a0c8afe", savePath = "Assets/Resources/Models/Weapons/Rifle_Scoped.glb" }
    };

    private Vector2 scrollPos;
    private bool isDownloading;

    [MenuItem("Tools/Download 3D Models from Tripo3D")]
    public static void ShowWindow()
    {
        GetWindow<ModelDownloader>("Download 3D Models");
    }

    void OnGUI()
    {
        GUILayout.Label("Download Models from Tripo3D", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "These models will be downloaded from Tripo3D.\n" +
            "Alternatively, download manually from the links below and place in the specified paths.",
            MessageType.Info);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var model in models)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(model.name, GUILayout.Width(120));
            EditorGUILayout.LabelField(model.savePath);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button($"Open {model.name} in Browser", GUILayout.Height(20)))
            {
                Application.OpenURL(model.url);
            }

            if (File.Exists(model.savePath))
            {
                EditorGUILayout.HelpBox($"Exists: {new FileInfo(model.savePath).Length / 1024f:F1} KB", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Not downloaded yet", MessageType.Warning);
            }
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();

        GUI.enabled = !isDownloading;
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "MANUAL DOWNLOAD INSTRUCTIONS:\n" +
            "1. Click each 'Open in Browser' button above\n" +
            "2. On the Tripo3D page, press F12 > Console\n" +
            "3. Paste the script from Assets/Editor/TripoDownloader_Console.js\n" +
            "4. Save the .glb file to the path shown above\n" +
            "5. Unity will auto-import the model",
            MessageType.Info);
    }
}

[System.Serializable]
public class ModelEntry
{
    public string name;
    public string url;
    public string savePath;
}
