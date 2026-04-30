using UnityEngine;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    [Header("EntityPrefabs")]
    public GameObject wallPrefab;
    public GameObject rockPrefab;
    public GameObject manPrefab;
    public GameObject cherryPrefab;

    [Header("TextPrefabs")]
    public List<TextPrefabPair> textPrefabs;

    [Header("LevelSetting")]
    public Transform levelRoot;
    public TextAsset levelsJsonFile;

    private LevelDataList allLevels;
    private int currentLevelIndex = 0;
    private List<GameObject> currentLevelObjects = new();
    private Dictionary<GridObject.TextContent, GameObject> textPrefabDict = new();


    [System.Serializable]
    public class TextPrefabPair
    {
        public GridObject.TextContent textContent;
        public GameObject prefab;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitTextPrefabDict();
    }

    private void InitTextPrefabDict()
    {
        textPrefabDict.Clear();
        if (textPrefabs == null || textPrefabs.Count == 0)
        {
            return;
        }

        foreach (var pair in textPrefabs)
        {
            if (pair.prefab != null && !textPrefabDict.ContainsKey(pair.textContent))
            {
                textPrefabDict.Add(pair.textContent, pair.prefab);
            }
        }
    }

    private void Start()
    {
        LoadLevelsFromJson();
        LoadLevel(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) LoadLevel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) LoadLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) LoadLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) LoadLevel(3);
        if (Input.GetKeyDown(KeyCode.R)) ResetCurrentLevel();
    }

    private void LoadLevelsFromJson()
    {
        if (levelsJsonFile == null)
        {
            return;
        }

        allLevels = JsonUtility.FromJson<LevelDataList>(levelsJsonFile.text);
    }

    public void LoadLevel(int levelIndex)
    {
        if (allLevels == null || levelIndex < 0 || levelIndex >= allLevels.levels.Count)
        {
            return;
        }

        Time.timeScale = 1f;
        RuleManager.Instance.ClearAllTextObjects();
        ClearCurrentLevel();

        LevelData levelData = allLevels.levels[levelIndex];
        currentLevelIndex = levelIndex;
        foreach (var objData in levelData.objects)
        {
            SpawnLevelObject(objData);
        }
        if (levelData.textObjects != null)
        {
            foreach (var textData in levelData.textObjects)
            {
                SpawnTextObject(textData);
            }
        }

        RuleManager.Instance.RefreshAllRules();
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = true;
        }
    }
    private void SpawnLevelObject(LevelObjectData objData)
    {
        GameObject prefab = null;
        switch (objData.objectType)
        {
            case "Wall": prefab = wallPrefab; break;
            case "Rock": prefab = rockPrefab; break;
            case "Man": prefab = manPrefab; break;
            case "Cherry": prefab = cherryPrefab; break;
        }

        if (prefab == null)
        {
            return;
        }

        Vector2 worldPos = GridManager.Instance.GridToWorld(new Vector2Int(objData.gridX, objData.gridY));
        GameObject newObj = Instantiate(prefab, worldPos, Quaternion.identity, levelRoot);
        newObj.name = $"{objData.objectType}_{objData.gridX}_{objData.gridY}";
        currentLevelObjects.Add(newObj);

        GridObject go = newObj.GetComponent<GridObject>();
        if (go != null)
        {
            LevelManager.Instance.RegisterObject(go);
        }
    }
    private void SpawnTextObject(LevelTextData textData)
    {
        if (!System.Enum.TryParse(textData.textContent, out GridObject.TextContent content))
        {
            return;
        }
        if (!textPrefabDict.TryGetValue(content, out GameObject prefab))
        {
            return;
        }

        Vector2 worldPos = GridManager.Instance.GridToWorld(new Vector2Int(textData.gridX, textData.gridY));
        GameObject newObj = Instantiate(prefab, worldPos, Quaternion.identity, levelRoot);
        newObj.name = $"Text_{textData.textContent}_{textData.gridX}_{textData.gridY}";
        currentLevelObjects.Add(newObj);

        GridObject gridObj = newObj.GetComponent<GridObject>();
        if (gridObj != null)
        {
            LevelManager.Instance.RegisterObject(gridObj);
        }
    }

    public void ClearCurrentLevel()
    {
        foreach (var obj in currentLevelObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        currentLevelObjects.Clear();
    }

    public void ResetCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex >= allLevels.levels.Count)
        {
            return;
        }
        LoadLevel(nextLevelIndex);
    }

    public void LoadEditorLevel(LevelData levelData)
    {
        ClearCurrentLevel();
        RuleManager.Instance.ClearAllTextObjects();

        foreach (var objData in levelData.objects)
        {
            SpawnLevelObject(objData);
        }

        if (levelData.textObjects != null)
        {
            foreach (var textData in levelData.textObjects)
            {
                SpawnTextObject(textData);
            }
        }
    }

    public void LoadTestLevel(LevelData levelData)
    {
        ClearCurrentLevel();
        RuleManager.Instance.ClearAllTextObjects();

        foreach (var objData in levelData.objects)
        {
            SpawnLevelObject(objData);
        }

        if (levelData.textObjects != null)
        {
            foreach (var textData in levelData.textObjects)
            {
                SpawnTextObject(textData);
            }
        }

        RuleManager.Instance.RefreshAllRules();
    }
}