using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteToPrefab : EditorWindow
{
    private string spriteFolderPath = "Assets/Sprites";
    private string prefabSavePath = "Assets/Prefabs";
    private int pixelsPerUnit = 64;

    [MenuItem("Tools/Move Box/Batch Generate Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<SpriteToPrefab>("Batch Create Prefabs");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite To Prefab Settings", EditorStyles.boldLabel);

        spriteFolderPath = EditorGUILayout.TextField("Sprite Folder Path", spriteFolderPath);
        prefabSavePath = EditorGUILayout.TextField("Prefab Save Path", prefabSavePath);
        pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit);

        if (GUILayout.Button("Start Generate Prefabs"))
        {
            GeneratePrefabs();
        }
    }

    private void GeneratePrefabs()
    {
        if (!Directory.Exists(spriteFolderPath))
        {
            EditorUtility.DisplayDialog("Error", "Sprite folder path not found!", "OK");
            return;
        }

        if (!Directory.Exists(prefabSavePath))
        {
            Directory.CreateDirectory(prefabSavePath);
        }

        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolderPath });

        foreach (string guid in spriteGuids)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            GameObject go = new GameObject(sprite.name);
            go.AddComponent<SpriteRenderer>().sprite = sprite;

            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            string prefabPath = Path.Combine(prefabSavePath, sprite.name + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            DestroyImmediate(go);

            Debug.Log("Prefab created: " + prefabPath);
        }

        EditorUtility.DisplayDialog("Success", "All prefabs generated completed!", "OK");
        AssetDatabase.Refresh();
    }
}