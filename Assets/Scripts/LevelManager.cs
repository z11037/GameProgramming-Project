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
                GameObject temp = new GameObject("LevelManager_AutoCreated");
                _instance = temp.AddComponent<LevelManager>();
            }
            return _instance;
        }
    }

    private Dictionary<Vector2Int, GridObject> gridDic = new();

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        RuleManager.Instance.RefreshAllRules();
    }

    public void RegisterObject(GridObject obj)
    {
        if (obj == null) return;
        gridDic[obj.GridPos] = obj;
        obj.SetTargetGrid(obj.GridPos);

        if (obj.type == GridObject.ObjectType.Text)
        {
            RuleManager.Instance.RegisterTextObject(obj);
        }
    }

    public List<GridObject> GetAllObjects()
    {
        return new List<GridObject>(gridDic.Values);
    }

    public List<GridObject> GetObjectsByNoun(GridObject.TextContent noun)
    {
        List<GridObject> result = new List<GridObject>();
        GridObject.ObjectType targetType = NounToObjectType(noun);

        foreach (var obj in gridDic.Values)
        {
            if (obj.type == targetType)
            {
                result.Add(obj);
            }
        }
        return result;
    }

    public bool CheckWinCondition()
    {
        List<GridObject> allYouObjects = GetAllYouObjects();
        if (allYouObjects.Count == 0) return false;

        foreach (var youObj in allYouObjects)
        {
            if (youObj.isWin) return true;

            if (gridDic.TryGetValue(youObj.TargetGridPos, out GridObject targetObj))
            {
                if (targetObj.isWin && targetObj != youObj) return true;
            }
        }

        return false;
    }

    public List<GridObject> GetAllYouObjects()
    {
        List<GridObject> result = new List<GridObject>();
        foreach (var obj in gridDic.Values)
        {
            if (obj.isYou) result.Add(obj);
        }
        return result;
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

        Vector2Int targetPos = obj.TargetGridPos + dir;

        if (gridDic.TryGetValue(targetPos, out GridObject targetObj))
        {
            if (targetObj.isStop) return false;
            if (targetObj.isPush)
            {
                if (!TryMove(targetObj, dir)) return false;
            }
        }

        gridDic.Remove(obj.TargetGridPos);
        obj.SetTargetGrid(targetPos);
        gridDic[targetPos] = obj;

        return true;
    }
}