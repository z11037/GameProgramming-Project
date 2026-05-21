using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class EditorOperation
{
    public enum OperationType { Place, Delete }
    public OperationType type;
    public GameObject prefab;
    public Vector2Int gridPos;
    public GridObject spawnedObject;
}
public class LevelEditorManager : MonoBehaviour
{
    public static LevelEditorManager Instance { get; private set; }

    public GameObject[] editableObjectPrefabs;
    public GameObject[] editableTextPrefabs;

    private GameObject selectedPrefab;
    private LevelData editingLevelData = new LevelData();
    private string customLevelSavePath;
    private Stack<EditorOperation> operationStack = new Stack<EditorOperation>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        customLevelSavePath = Path.Combine(Application.persistentDataPath, "CustomLevels");
        if (!Directory.Exists(customLevelSavePath))
        {
            Directory.CreateDirectory(customLevelSavePath);
        }
    }

    private void Start()
    {
        if (editingLevelData == null)
        {
            editingLevelData = new LevelData();
        }

        if (editingLevelData.textObjects == null)
        {
            editingLevelData.textObjects = new List<LevelTextData>();
        }
        if (editingLevelData.objects == null)
        {
            editingLevelData.objects = new List<LevelObjectData>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Editing) return;
        if (selectedPrefab == null) return;
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);

        if (Input.GetMouseButtonDown(0))
        {
            PlaceObject(gridPos);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            DeleteObject(gridPos);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            UndoLastOperation();
        }
    }

    public void SelectPrefab(GameObject prefab)
    {
        selectedPrefab = prefab;
        Debug.Log($"Selected prefab: {prefab.name}");
    }

    private void PlaceObject(Vector2Int gridPos)
    {
        GridObject existingObj = LevelManager.Instance.GetSingleObjectAtGrid(gridPos);
        if (existingObj != null)
        {
            operationStack.Push(new EditorOperation
            {
                type = EditorOperation.OperationType.Delete,
                prefab = existingObj.gameObject,
                gridPos = gridPos,
                spawnedObject = existingObj
            });

            LevelManager.Instance.UnregisterObject(existingObj);
            Destroy(existingObj.gameObject);
        }

        Vector3 spawnPos = GridManager.Instance.GridToWorld(gridPos);
        spawnPos.z = GetZOffsetForPrefab(selectedPrefab);
        GameObject newObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, LevelLoader.Instance.levelRoot);
        GridObject gridObj = newObj.GetComponent<GridObject>();

        if (gridObj != null)
        {
            gridObj.SetTargetGrid(gridPos);
            LevelManager.Instance.RegisterObject(gridObj);
            UpdateLevelData(gridObj, gridPos, true);

            operationStack.Push(new EditorOperation
            {
                type = EditorOperation.OperationType.Place,
                prefab = selectedPrefab,
                gridPos = gridPos,
                spawnedObject = gridObj
            });
        }
    }
    private void DeleteObject(Vector2Int gridPos)
    {
        GridObject objToDelete = LevelManager.Instance.GetSingleObjectAtGrid(gridPos);
        if (objToDelete != null)
        {
            operationStack.Push(new EditorOperation
            {
                type = EditorOperation.OperationType.Delete,
                prefab = objToDelete.gameObject,
                gridPos = gridPos,
                spawnedObject = objToDelete
            });

            UpdateLevelData(objToDelete, gridPos, false);
            LevelManager.Instance.UnregisterObject(objToDelete);
            Destroy(objToDelete.gameObject);
        }
    }

    public void UndoLastOperation()
    {
        if (operationStack.Count == 0)
        {
            return;
        }

        EditorOperation lastOp = operationStack.Pop();
        switch (lastOp.type)
        {
            case EditorOperation.OperationType.Place:
                if (lastOp.spawnedObject != null)
                {
                    UpdateLevelData(lastOp.spawnedObject, lastOp.gridPos, false);
                    LevelManager.Instance.UnregisterObject(lastOp.spawnedObject);
                    Destroy(lastOp.spawnedObject.gameObject);
                }
                break;
            case EditorOperation.OperationType.Delete:
                if (lastOp.prefab != null)
                {
                    Vector3 spawnPos = GridManager.Instance.GridToWorld(lastOp.gridPos);
                    spawnPos.z = GetZOffsetForPrefab(lastOp.prefab);
                    GameObject newObj = Instantiate(lastOp.prefab, spawnPos, Quaternion.identity, LevelLoader.Instance.levelRoot);
                    GridObject gridObj = newObj.GetComponent<GridObject>();

                    if (gridObj != null)
                    {
                        gridObj.SetTargetGrid(lastOp.gridPos);
                        LevelManager.Instance.RegisterObject(gridObj);
                        UpdateLevelData(gridObj, lastOp.gridPos, true);
                    }
                }
                break;
        }

    }

    private void UpdateLevelData(GridObject gridObj, Vector2Int gridPos, bool isAdd)
    {
        if (editingLevelData.textObjects == null) editingLevelData.textObjects = new List<LevelTextData>();
        if (editingLevelData.objects == null) editingLevelData.objects = new List<LevelObjectData>();
        if (gridObj.type == GridObject.ObjectType.Text)
        {
            if (isAdd)
            {
                LevelTextData textData = new LevelTextData
                {
                    textContent = gridObj.textContent.ToString(),
                    gridX = gridPos.x,
                    gridY = gridPos.y
                };
                editingLevelData.textObjects.Add(textData);
            }
            else
            {
                editingLevelData.textObjects.RemoveAll(t => t.gridX == gridPos.x && t.gridY == gridPos.y);
            }
        }
        else
        {
            if (isAdd)
            {
                LevelObjectData objData = new LevelObjectData
                {
                    objectType = gridObj.type.ToString(),
                    gridX = gridPos.x,
                    gridY = gridPos.y
                };
                editingLevelData.objects.Add(objData);
            }
            else
            {
                editingLevelData.objects.RemoveAll(o => o.gridX == gridPos.x && o.gridY == gridPos.y);
            }
        }
    }

    public void SaveCurrentLevel()
    {
        string json = JsonUtility.ToJson(editingLevelData, true);
        string savePath = Path.Combine(customLevelSavePath, "CustomLevel_01.json");
        File.WriteAllText(savePath, json);
        Debug.Log($"Level saved to: {savePath}");
    }

    public void LoadCustomLevel()
    {
        string loadPath = Path.Combine(customLevelSavePath, "CustomLevel_01.json");
        if (!File.Exists(loadPath))
        {
            Debug.LogError("No custom level found!");
            return;
        }

        GameManager.Instance.SwitchToEditingMode();
        string json = File.ReadAllText(loadPath);
        editingLevelData = JsonUtility.FromJson<LevelData>(json);
        LevelLoader.Instance.LoadEditorLevel(editingLevelData);
        Debug.Log("Custom level loaded");
    }

    public void TestEditedLevel()
    {
        LevelLoader.Instance.LoadTestLevel(editingLevelData);
        GameManager.Instance.SwitchToPlayingMode();
    }

    private float GetZOffsetForPrefab(GameObject prefab)
    {
        GridObject gridObj = prefab.GetComponent<GridObject>();
        if (gridObj == null) return 0;

        if (gridObj.type == GridObject.ObjectType.Text) return -0.5f;
        if (gridObj.type == GridObject.ObjectType.Man) return -1f;
        return 0;
    }

    public void ClearCurrentLevel()
    {
        if (LevelLoader.Instance != null && LevelLoader.Instance.levelRoot != null)
        {
            foreach (Transform child in LevelLoader.Instance.levelRoot)
            {
                Destroy(child.gameObject);
            }
        }

        if (editingLevelData != null)
        {
            editingLevelData.objects.Clear();
            editingLevelData.textObjects.Clear();
        }

        selectedPrefab = null;

        operationStack.Clear();

        if (RuleManager.Instance != null)
            RuleManager.Instance.ClearAllTextObjects();

    }
}