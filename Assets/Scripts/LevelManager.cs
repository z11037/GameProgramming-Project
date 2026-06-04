using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
                if (_instance == null)
                {
                    GameObject temp = new GameObject("LevelManager");
                    _instance = temp.AddComponent<LevelManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Grid Settings")]
    public int maxGridBound = 20;

    [Header("Level Setting")]
    public Transform levelRoot;
    private List<GridObject> levelOriginalObjects = new();
    private Dictionary<GridObject, Vector3> originalPositions = new();
    private Dictionary<GridObject, Quaternion> originalRotations = new();

    private Dictionary<Vector2Int, List<GridObject>> gridDic = new();
    private List<GridObject> allObjectsList = new();

    public void SaveLevelOriginalState()
    {
        levelOriginalObjects.Clear();
        originalPositions.Clear();
        originalRotations.Clear();

        if (levelRoot == null) levelRoot = transform;

        GridObject[] allObjects = levelRoot.GetComponentsInChildren<GridObject>();
        foreach (var obj in allObjects)
        {
            levelOriginalObjects.Add(obj);
            originalPositions[obj] = obj.transform.position;
            originalRotations[obj] = obj.transform.rotation;
        }
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;

        foreach (var obj in levelOriginalObjects)
        {
            if (obj == null) continue;
            obj.transform.position = originalPositions[obj];
            obj.transform.rotation = originalRotations[obj];
            obj.ForceAlignToGrid();
            obj.ResetRuleProperties();

            if (obj.TryGetComponent<SpriteRenderer>(out var sr))
                sr.color = Color.white;

            RegisterObject(obj, skipForceAlign: true);
        }

        RuleManager.Instance.RefreshAllRules();
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ResetMoveLock();
            PlayerController.Instance.ResetWinState();
            PlayerController.Instance.enabled = true;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void RegisterObject(GridObject obj, bool skipForceAlign = false)
    {
        if (obj == null) return;

        if (!skipForceAlign)
            obj.ForceAlignToGrid();

        Vector2Int gridPos = obj.TargetGridPos;
        UnregisterObject(obj);

        if (!gridDic.ContainsKey(gridPos))
            gridDic[gridPos] = new List<GridObject>();

        if (!gridDic[gridPos].Contains(obj))
            gridDic[gridPos].Add(obj);

        if (!allObjectsList.Contains(obj))
            allObjectsList.Add(obj);

        if (obj.type == GridObject.ObjectType.Text)
            RuleManager.Instance.RegisterTextObject(obj);
    }

    public void UnregisterObject(GridObject obj)
    {
        if (obj == null) return;

        foreach (var kvp in gridDic)
        {
            if (kvp.Value.Contains(obj))
            {
                kvp.Value.Remove(obj);
                if (kvp.Value.Count == 0)
                    gridDic.Remove(kvp.Key);
                break;
            }
        }
        allObjectsList.Remove(obj);
    }

    public void UpdateObjectPosition(GridObject obj, Vector2Int newGridPos)
    {
        if (obj == null) return;
        obj.SetTargetGrid(newGridPos);
        UnregisterObject(obj);
        RegisterObject(obj, skipForceAlign: true);
    }

    public bool MoveObject(GridObject obj, Vector2Int dir, HashSet<GridObject> movedThisTurn)
    {
        if (obj == null || movedThisTurn.Contains(obj))
            return false;

        Vector2Int targetPos = obj.TargetGridPos + dir;
        if (Mathf.Abs(targetPos.x) > maxGridBound || Mathf.Abs(targetPos.y) > maxGridBound)
            return false;

        var objectsAtTarget = GetObjectsAtGrid(targetPos);
        foreach (var targetObj in objectsAtTarget)
        {
            if (targetObj == obj) continue;
            if (movedThisTurn.Contains(targetObj)) continue;

            if (targetObj.isStop && targetObj.isPush)
            {
                if (!MoveObject(targetObj, dir, movedThisTurn))
                    return false;
            }
            else if (targetObj.isStop)
            {
                return false;
            }
            else if (targetObj.isPush)
            {
                if (!MoveObject(targetObj, dir, movedThisTurn))
                    return false;
            }
        }

        movedThisTurn.Add(obj);
        UpdateObjectPosition(obj, targetPos);
        return true;
    }

    public List<GridObject> GetAllObjects() => new List<GridObject>(allObjectsList);

    public List<GridObject> GetObjectsAtGrid(Vector2Int gridPos)
    {
        return gridDic.TryGetValue(gridPos, out var list) ? new List<GridObject>(list) : new List<GridObject>();
    }

    public GridObject GetSingleObjectAtGrid(Vector2Int gridPos)
    {
        if (gridDic.TryGetValue(gridPos, out var list) && list.Count > 0)
            return list[0];
        return null;
    }

    public List<GridObject> GetObjectsByNoun(GridObject.TextContent noun)
    {
        GridObject.ObjectType targetType = NounToObjectType(noun);
        List<GridObject> result = new List<GridObject>();
        foreach (var obj in allObjectsList)
            if (obj.type == targetType)
                result.Add(obj);
        return result;
    }

    public List<GridObject> GetAllYouObjects()
    {
        List<GridObject> result = new List<GridObject>();
        foreach (var obj in allObjectsList)
            if (obj.isYou)
                result.Add(obj);
        return result;
    }

    public bool CheckWinCondition()
    {
        var allYou = GetAllYouObjects();
        if (allYou.Count == 0) return false;

        foreach (var you in allYou)
        {
            if (you.isWin)
                return true;

            if (gridDic.TryGetValue(you.TargetGridPos, out var cellObjects))
            {
                foreach (var obj in cellObjects)
                {
                    if (obj.isWin)
                        return true;
                }
            }
        }
        return false;
    }

    public List<GridObject> SortObjectsByDirection(List<GridObject> objects, Vector2Int dir)
    {
        List<GridObject> sorted = new List<GridObject>(objects);

        if (dir == Vector2Int.right)
            sorted.Sort((a, b) => b.TargetGridPos.x.CompareTo(a.TargetGridPos.x));
        else if (dir == Vector2Int.left)
            sorted.Sort((a, b) => a.TargetGridPos.x.CompareTo(b.TargetGridPos.x));
        else if (dir == Vector2Int.up)
            sorted.Sort((a, b) => b.TargetGridPos.y.CompareTo(a.TargetGridPos.y));
        else if (dir == Vector2Int.down)
            sorted.Sort((a, b) => a.TargetGridPos.y.CompareTo(b.TargetGridPos.y));

        return sorted;
    }

    private GridObject.ObjectType NounToObjectType(GridObject.TextContent noun)
    {
        return noun switch
        {
            GridObject.TextContent.Man => GridObject.ObjectType.Man,
            GridObject.TextContent.Wall => GridObject.ObjectType.Wall,
            GridObject.TextContent.Rock => GridObject.ObjectType.Rock,
            GridObject.TextContent.Cherry => GridObject.ObjectType.Cherry,
            _ => GridObject.ObjectType.Empty,
        };
    }
}