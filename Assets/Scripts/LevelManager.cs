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

    private Dictionary<Vector2Int, List<GridObject>> gridDic = new();
    private List<GridObject> allObjectsList = new();

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
        {
            obj.ForceAlignToGrid();
        }
        Vector2Int gridPos = obj.TargetGridPos;

        UnregisterObject(obj);

        if (!gridDic.ContainsKey(gridPos))
        {
            gridDic[gridPos] = new List<GridObject>();
        }
        if (!gridDic[gridPos].Contains(obj))
        {
            gridDic[gridPos].Add(obj);
        }
        if (!allObjectsList.Contains(obj))
        {
            allObjectsList.Add(obj);
        }

        if (obj.type == GridObject.ObjectType.Text)
        {
            RuleManager.Instance.RegisterTextObject(obj);
        }

    }

    public void UnregisterObject(GridObject obj)
    {
        if (obj == null) return;
        foreach (var item in gridDic)
        {
            if (item.Value.Contains(obj))
            {
                item.Value.Remove(obj);
                if (item.Value.Count == 0)
                {
                    gridDic.Remove(item.Key);
                }
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

    public List<GridObject> GetAllObjects()
    {
        return new List<GridObject>(allObjectsList);
    }

    public List<GridObject> GetObjectsByNoun(GridObject.TextContent noun)
    {
        List<GridObject> result = new List<GridObject>();
        GridObject.ObjectType targetType = NounToObjectType(noun);

        foreach (var obj in allObjectsList)
        {
            if (obj.type == targetType)
            {
                result.Add(obj);
            }
        }
        return result;
    }

    public List<GridObject> GetAllYouObjects()
    {
        List<GridObject> result = new List<GridObject>();
        foreach (var obj in allObjectsList)
        {
            if (obj.isYou)
            {
                result.Add(obj);
            }
        }
        return result;
    }

    public bool CheckWinCondition()
    {
        List<GridObject> allYouObjects = GetAllYouObjects();

        if (allYouObjects.Count == 0)
        {
            return false;
        }

        foreach (var youObj in allYouObjects)
        {
            if (youObj.isWin)
            {
                return true;
            }

            if (gridDic.TryGetValue(youObj.TargetGridPos, out List<GridObject> sameGridObjects))
            {
                foreach (var obj in sameGridObjects)
                {
                    if (obj.isWin && obj != youObj)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private GridObject.ObjectType NounToObjectType(GridObject.TextContent noun)
    {
        switch (noun)
        {
            case GridObject.TextContent.Man: return GridObject.ObjectType.Man;
            case GridObject.TextContent.Wall: return GridObject.ObjectType.Wall;
            case GridObject.TextContent.Rock: return GridObject.ObjectType.Rock;
            case GridObject.TextContent.Cherry: return GridObject.ObjectType.Cherry;
            default: return GridObject.ObjectType.Empty;
        }
    }

    public bool TryMove(GridObject obj, Vector2Int dir)
    {
        if (obj == null || dir == Vector2Int.zero) return false;

        Vector2Int currentPos = obj.TargetGridPos;
        Vector2Int targetPos = currentPos + dir;

        if (gridDic.TryGetValue(targetPos, out List<GridObject> targetObjects))
        {
            bool hasStop = false;
            List<GridObject> pushObjects = new List<GridObject>();

            foreach (var targetObj in targetObjects)
            {
                if (targetObj.isStop)
                {
                    hasStop = true;
                    break;
                }
                if (targetObj.isPush)
                {
                    pushObjects.Add(targetObj);
                }
            }

            if (hasStop) return false;

            foreach (var pushObj in pushObjects)
            {
                if (!TryMove(pushObj, dir))
                {
                    return false;
                }
            }
        }

        UpdateObjectPosition(obj, targetPos);
        return true;
    }

    public List<GridObject> GetObjectsAtGrid(Vector2Int gridPos)
    {
        if (gridDic.TryGetValue(gridPos, out List<GridObject> objects))
        {
            return new List<GridObject>(objects);
        }
        return new List<GridObject>();
    }
}