using UnityEngine;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    [Header("普通物体预制体")]
    public GameObject wallPrefab;
    public GameObject rockPrefab;
    public GameObject manPrefab;
    public GameObject cherryPrefab;

    [Header("文字方块预制体（List配置）")]
    public List<TextPrefabPair> textPrefabs; // 在Inspector里拖入所有文字预制体

    [Header("关卡设置")]
    public Transform levelRoot;
    public TextAsset levelsJsonFile;

    private LevelDataList allLevels;
    private int currentLevelIndex = 0;
    private List<GameObject> currentLevelObjects = new();
    // 内部用Dictionary快速查找文字预制体
    private Dictionary<GridObject.TextContent, GameObject> textPrefabDict = new();


    [System.Serializable]
    public class TextPrefabPair
    {
        public GridObject.TextContent textContent; // 文字内容枚举
        public GameObject prefab; // 对应的预制体
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ========== 新增：把List转成Dictionary，方便快速查找 ==========
        InitTextPrefabDict();
    }

    // 初始化文字预制体字典
    private void InitTextPrefabDict()
    {
        textPrefabDict.Clear();
        if (textPrefabs == null || textPrefabs.Count == 0)
        {
            Debug.LogWarning("【LevelLoader】未配置文字预制体List！");
            return;
        }

        foreach (var pair in textPrefabs)
        {
            if (pair.prefab != null && !textPrefabDict.ContainsKey(pair.textContent))
            {
                textPrefabDict.Add(pair.textContent, pair.prefab);
            }
        }

        Debug.Log($"【LevelLoader】初始化文字预制体字典完成，共 {textPrefabDict.Count} 个");
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
        if (Input.GetKeyDown(KeyCode.R)) ResetCurrentLevel();
    }

    private void LoadLevelsFromJson()
    {
        if (levelsJsonFile == null)
        {
            Debug.LogError("【LevelLoader】未找到Levels.json文件！");
            return;
        }

        allLevels = JsonUtility.FromJson<LevelDataList>(levelsJsonFile.text);
        Debug.Log($"【LevelLoader】成功加载 {allLevels.levels.Count} 个关卡");
    }

    public void LoadLevel(int levelIndex)
    {
        if (allLevels == null || levelIndex < 0 || levelIndex >= allLevels.levels.Count)
        {
            Debug.LogError($"【LevelLoader】关卡{levelIndex}不存在！");
            return;
        }

        Debug.Log($"【LevelLoader】开始加载关卡：{allLevels.levels[levelIndex].levelName}");
        Time.timeScale = 1f;

        // ========== 修复1：先清空RuleManager的旧文字列表 ==========
        RuleManager.Instance.ClearAllTextObjects();

        // ========== 修复2：完全清空旧物体 ==========
        ClearCurrentLevel();

        LevelData levelData = allLevels.levels[levelIndex];
        currentLevelIndex = levelIndex;

        // 加载普通物体
        foreach (var objData in levelData.objects)
        {
            SpawnLevelObject(objData);
        }

        // 加载文字方块
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

        Debug.Log($"【LevelLoader】关卡加载完成：{levelData.levelName}");
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
            Debug.LogError($"【LevelLoader】未找到物体类型：{objData.objectType} 的预制体！");
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

    // ========== 修复：根据textContent从Dictionary取对应的预制体 ==========
    private void SpawnTextObject(LevelTextData textData)
    {
        // 1. 解析textContent枚举
        if (!System.Enum.TryParse(textData.textContent, out GridObject.TextContent content))
        {
            Debug.LogError($"【LevelLoader】无法解析文字内容：{textData.textContent}");
            return;
        }

        // 2. 从Dictionary取对应的预制体
        if (!textPrefabDict.TryGetValue(content, out GameObject prefab))
        {
            Debug.LogError($"【LevelLoader】未找到文字内容：{content} 的预制体！");
            return;
        }

        // 3. 生成物体
        Vector2 worldPos = GridManager.Instance.GridToWorld(new Vector2Int(textData.gridX, textData.gridY));
        GameObject newObj = Instantiate(prefab, worldPos, Quaternion.identity, levelRoot);
        newObj.name = $"Text_{textData.textContent}_{textData.gridX}_{textData.gridY}";
        currentLevelObjects.Add(newObj);

        // 4. 注册到LevelManager（预制体已经配好GridObject，不用再改属性）
        GridObject gridObj = newObj.GetComponent<GridObject>();
        if (gridObj != null)
        {
            LevelManager.Instance.RegisterObject(gridObj);
        }
    }

    private void ClearCurrentLevel()
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
            Debug.Log("【LevelLoader】所有关卡通关！");
            return;
        }
        LoadLevel(nextLevelIndex);
    }
}