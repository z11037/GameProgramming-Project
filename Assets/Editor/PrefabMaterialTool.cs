using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class PrefabMaterialTool : EditorWindow
{
    private enum Mode { SingleMaterial, SmartMatch }
    private Mode currentMode = Mode.SingleMaterial;

    private Material targetMaterial;

    private Material playerMaterial;
    private Material wallMaterial;
    private Material rockMaterial;
    private Material cherryMaterial;
    private Material textMaterial;

    private enum Scope { SelectedPrefabs, AllProjectPrefabs }
    private Scope currentScope = Scope.SelectedPrefabs;

    private float progress = 0f;
    private string statusText = "";
    private bool isProcessing = false;

    [MenuItem("Tools/Batch Replace Prefab Materials")]
    public static void ShowWindow()
    {
        GetWindow<PrefabMaterialTool>("Batch Replace Materials");
    }

    void OnGUI()
    {
        GUILayout.Label("Batch Replace Prefab Materials", EditorStyles.boldLabel);
        GUILayout.Space(10);

        currentMode = (Mode)EditorGUILayout.EnumPopup("Replacement Mode", currentMode);
        GUILayout.Space(5);

        if (currentMode == Mode.SingleMaterial)
        {
            GUILayout.Label("Single Material Mode: Use one material for all prefabs", EditorStyles.helpBox);
            targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), false);
        }
        else
        {
            GUILayout.Label("Smart Match Mode: Auto-match materials by prefab name", EditorStyles.helpBox);
            GUILayout.Space(5);
            playerMaterial = (Material)EditorGUILayout.ObjectField("Player (Man) Material", playerMaterial, typeof(Material), false);
            wallMaterial = (Material)EditorGUILayout.ObjectField("Wall Material", wallMaterial, typeof(Material), false);
            rockMaterial = (Material)EditorGUILayout.ObjectField("Rock Material", rockMaterial, typeof(Material), false);
            cherryMaterial = (Material)EditorGUILayout.ObjectField("Cherry Material", cherryMaterial, typeof(Material), false);
            textMaterial = (Material)EditorGUILayout.ObjectField("Text Material", textMaterial, typeof(Material), false);
        }

        GUILayout.Space(10);

        currentScope = (Scope)EditorGUILayout.EnumPopup("Processing Scope", currentScope);
        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(statusText))
        {
            EditorGUILayout.HelpBox(statusText, MessageType.Info);
        }

        if (isProcessing)
        {
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, "Processing...");
        }

        GUILayout.Space(10);

        using (new EditorGUI.DisabledScope(isProcessing || !ValidateInputs()))
        {
            if (GUILayout.Button("Start Batch Replacement", GUILayout.Height(35)))
            {
                StartBatchReplace();
            }
        }

        if (GUILayout.Button("Reset Settings"))
        {
            ResetSettings();
        }
    }

    bool ValidateInputs()
    {
        if (currentMode == Mode.SingleMaterial)
        {
            return targetMaterial != null;
        }
        else
        {
            return playerMaterial != null || wallMaterial != null ||
                   rockMaterial != null || cherryMaterial != null || textMaterial != null;
        }
    }

    void ResetSettings()
    {
        currentMode = Mode.SingleMaterial;
        currentScope = Scope.SelectedPrefabs;
        targetMaterial = null;
        playerMaterial = null;
        wallMaterial = null;
        rockMaterial = null;
        cherryMaterial = null;
        textMaterial = null;
        progress = 0f;
        statusText = "";
        isProcessing = false;
    }

    async void StartBatchReplace()
    {
        isProcessing = true;
        progress = 0f;
        statusText = "Collecting prefabs...";
        Repaint();

        List<GameObject> prefabs = new List<GameObject>();
        if (currentScope == Scope.SelectedPrefabs)
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj is GameObject go && PrefabUtility.IsPartOfPrefabAsset(go))
                {
                    prefabs.Add(go);
                }
            }
        }
        else
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
            }
        }

        if (prefabs.Count == 0)
        {
            statusText = "No valid prefabs found!";
            isProcessing = false;
            Repaint();
            return;
        }

        statusText = $"Found {prefabs.Count} prefabs, starting processing...";
        Repaint();

        int modifiedCount = 0;
        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            progress = (float)(i + 1) / prefabs.Count;
            statusText = $"Processing: {prefab.name} ({i + 1}/{prefabs.Count})";
            Repaint();

            bool modified = ProcessPrefab(prefab);
            if (modified)
            {
                modifiedCount++;
                EditorUtility.SetDirty(prefab);
            }

            if (i % 10 == 0)
            {
                await System.Threading.Tasks.Task.Delay(10);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        statusText = $"Processing complete! Modified {modifiedCount} prefabs";
        isProcessing = false;
        Repaint();
        Debug.Log($"[Batch Material Tool] Processing complete, modified {modifiedCount} prefabs");
    }

    bool ProcessPrefab(GameObject prefab)
    {
        bool modified = false;
        string prefabName = prefab.name.ToLower();

        SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (SpriteRenderer renderer in renderers)
        {
            Material matToUse = null;

            if (currentMode == Mode.SingleMaterial)
            {
                matToUse = targetMaterial;
            }
            else
            {
                if (prefabName.Contains("man") || prefabName.Contains("player"))
                {
                    matToUse = playerMaterial;
                }
                else if (prefabName.Contains("wall"))
                {
                    matToUse = wallMaterial;
                }
                else if (prefabName.Contains("rock"))
                {
                    matToUse = rockMaterial;
                }
                else if (prefabName.Contains("cherry"))
                {
                    matToUse = cherryMaterial;
                }
                else if (prefabName.Contains("text"))
                {
                    matToUse = textMaterial;
                }
            }

            if (matToUse != null && renderer.material != matToUse)
            {
                renderer.material = matToUse;
                modified = true;
            }
        }

        return modified;
    }
}