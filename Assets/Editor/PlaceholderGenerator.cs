using UnityEngine;
using UnityEditor;

public class PlaceholderGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Placeholder Prefabs")]
    static void GeneratePlaceholders()
    {
        string[] paths = new string[]
        {
            "Assets/Resources/Models/Enemies",
            "Assets/Resources/Models/Weapons",
            "Assets/Resources/Models/Environment"
        };

        foreach (string path in paths)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path);
                string folder = System.IO.Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder("Assets/Resources", "Models");
                string parentFolder = parent.Replace("\\", "/");
                if (!AssetDatabase.IsValidFolder(path))
                    AssetDatabase.CreateFolder(parentFolder, folder);
            }
        }

        CreatePlaceholder("Assets/Resources/Models/Enemies/Tank_Enemy.prefab",
            PrimitiveType.Cylinder, new Vector3(2, 1, 3), Color.green,
            "Tank placeholder - replace with Tripo3D tank model");

        CreatePlaceholder("Assets/Resources/Models/Enemies/Soldier_Enemy.prefab",
            PrimitiveType.Capsule, new Vector3(0.5f, 1.8f, 0.5f), Color.red,
            "Soldier placeholder - replace with downloaded model");

        CreatePlaceholder("Assets/Resources/Models/Weapons/AssaultRifle_Weapon.prefab",
            PrimitiveType.Cube, new Vector3(0.8f, 0.1f, 0.1f), Color.gray,
            "Assault Rifle placeholder - replace with AK-47 model from Tripo3D");

        CreatePlaceholder("Assets/Resources/Models/Weapons/SniperRifle_Weapon.prefab",
            PrimitiveType.Cube, new Vector3(1.2f, 0.08f, 0.08f), Color.black,
            "Sniper Rifle placeholder");

        CreatePlaceholder("Assets/Resources/Models/Weapons/RPG_Weapon.prefab",
            PrimitiveType.Cylinder, new Vector3(0.08f, 0.8f, 0.08f), new Color(0.3f, 0.5f, 0.2f),
            "RPG placeholder");

        CreatePlaceholder("Assets/Resources/Models/Environment/Tunnel_Environment.prefab",
            PrimitiveType.Cube, new Vector3(4, 3, 10), new Color(0.3f, 0.2f, 0.1f),
            "Tunnel section placeholder");

        CreatePlaceholder("Assets/Resources/Models/Environment/Building_Rubble.prefab",
            PrimitiveType.Cube, new Vector3(3, 2, 3), new Color(0.5f, 0.4f, 0.3f),
            "Destroyed building placeholder");

        AssetDatabase.Refresh();
        Debug.Log("Placeholder prefabs created. Replace them with downloaded 3D models.");
    }

    static void CreatePlaceholder(string path, PrimitiveType type, Vector3 scale,
        Color color, string description)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = System.IO.Path.GetFileNameWithoutExtension(path);
        obj.transform.localScale = scale;

        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            r.material = mat;
        }

        PrefabUtility.SaveAsPrefabAsset(obj, path);
        DestroyImmediate(obj);
    }
}
