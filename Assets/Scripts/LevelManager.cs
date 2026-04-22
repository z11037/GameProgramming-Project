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

    public void RegisterObject(GridObject obj)
    {
        if (obj == null) return;
        gridDic[obj.GridPos] = obj;
        obj.SetTargetGrid(obj.GridPos);
    }

    public List<GridObject> GetAllObjects()
    {
        return new List<GridObject>(gridDic.Values);
    }

    public bool TryMove(GridObject obj, Vector2Int dir)
    {
        if (obj == null || dir == Vector2Int.zero) return false;

        Vector2Int targetPos = obj.TargetGridPos + dir;

        if (gridDic.TryGetValue(targetPos, out GridObject targetObj))
        {

            if (targetObj.isStop)
                return false;

            if (targetObj.isPush)
            {
                if (!TryMove(targetObj, dir))
                    return false;
            }
        }

        gridDic.Remove(obj.TargetGridPos);
        obj.SetTargetGrid(targetPos);
        gridDic[targetPos] = obj;

        return true;
    }
}