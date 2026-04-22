using UnityEngine;
using System.Collections.Generic;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance;
    private List<GridObject> allTextObjects = new();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterText(GridObject textObj)
    {
        if (textObj.type == GridObject.ObjectType.Text)
            allTextObjects.Add(textObj);
    }

    public void RefreshRules()
    {
        foreach (var obj in LevelManager.Instance.GetAllObjects())
        {
            obj.isYou = false;
            obj.isStop = false;
            obj.isPush = false;
            obj.isWin = false;
        }

        foreach (var textObj in allTextObjects)
        {
        }
    }
}