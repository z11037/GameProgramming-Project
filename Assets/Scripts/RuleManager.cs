using UnityEngine;
using System.Collections.Generic;

public class RuleManager : MonoBehaviour
{
    private static RuleManager _instance;
    public static RuleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject temp = new GameObject("RuleManager_AutoCreated");
                _instance = temp.AddComponent<RuleManager>();
            }
            return _instance;
        }
    }

    private List<GridObject> allTextObjects = new();

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    public void RegisterTextObject(GridObject textObj)
    {
        if (textObj == null || textObj.type != GridObject.ObjectType.Text) return;
        if (!allTextObjects.Contains(textObj))
        {
            allTextObjects.Add(textObj);
        }
    }

    public void UnregisterTextObject(GridObject textObj)
    {
        if (allTextObjects.Contains(textObj))
        {
            allTextObjects.Remove(textObj);
        }
    }

    public void RefreshAllRules()
    {
        List<GridObject> allObjects = LevelManager.Instance.GetAllObjects();
        foreach (var obj in allObjects)
        {
            obj.ResetRuleProperties();
        }

        List<ValidRule> validRules = ScanAllValidRules();
        ApplyRules(validRules, allObjects);
        Debug.Log($"扫描到有效规则数量：{validRules.Count}");
    }

    private List<ValidRule> ScanAllValidRules()
    {
        List<ValidRule> rules = new List<ValidRule>();
        Dictionary<Vector2Int, GridObject> textPosDic = new();

        foreach (var textObj in allTextObjects)
        {
            textPosDic[textObj.TargetGridPos] = textObj;
        }

        // 横向规则扫描
        foreach (var textObj in allTextObjects)
        {
            if (textObj.textType != GridObject.TextType.Noun) continue;

            Vector2Int currentPos = textObj.TargetGridPos;
            Vector2Int isPos = currentPos + Vector2Int.right;
            Vector2Int targetPos = isPos + Vector2Int.right;

            if (textPosDic.TryGetValue(isPos, out GridObject isText) && textPosDic.TryGetValue(targetPos, out GridObject targetText))
            {
                if (isText.textContent == GridObject.TextContent.Is)
                {
                    rules.Add(new ValidRule()
                    {
                        noun = textObj.textContent,
                        target = targetText.textContent,
                        targetType = targetText.textType
                    });
                }
            }
        }

        // 纵向规则扫描
        foreach (var textObj in allTextObjects)
        {
            if (textObj.textType != GridObject.TextType.Noun) continue;

            Vector2Int currentPos = textObj.TargetGridPos;
            Vector2Int isPos = currentPos + Vector2Int.down;
            Vector2Int targetPos = isPos + Vector2Int.down;

            if (textPosDic.TryGetValue(isPos, out GridObject isText) && textPosDic.TryGetValue(targetPos, out GridObject targetText))
            {
                if (isText.textContent == GridObject.TextContent.Is)
                {
                    rules.Add(new ValidRule()
                    {
                        noun = textObj.textContent,
                        target = targetText.textContent,
                        targetType = targetText.textType
                    });
                }
            }
        }

        return rules;
    }

    private void ApplyRules(List<ValidRule> rules, List<GridObject> allObjects)
    {
        foreach (var rule in rules)
        {
            List<GridObject> targetObjects = LevelManager.Instance.GetObjectsByNoun(rule.noun);

            foreach (var obj in targetObjects)
            {
                // 属性规则
                if (rule.targetType == GridObject.TextType.Property)
                {
                    switch (rule.target)
                    {
                        case GridObject.TextContent.You:
                            obj.isYou = true;
                            break;
                        case GridObject.TextContent.Stop:
                            obj.isStop = true;
                            break;
                        case GridObject.TextContent.Push:
                            obj.isPush = true;
                            break;
                        case GridObject.TextContent.Win:
                            obj.isWin = true;
                            break;
                    }
                }

                // 类型转换规则
                if (rule.targetType == GridObject.TextType.Noun)
                {
                    obj.type = NounToObjectType(rule.target);
                }
            }
        }
    }

    // 仅保留你现有资源的类型转换
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

    private class ValidRule
    {
        public GridObject.TextContent noun;
        public GridObject.TextContent target;
        public GridObject.TextType targetType;
    }
}