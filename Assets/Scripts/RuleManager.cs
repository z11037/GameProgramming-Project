using UnityEngine;
using System.Collections.Generic;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance { get; private set; }

    private class ValidRule
    {
        public GridObject.TextContent noun;
        public GridObject.TextContent target;
        public GridObject.TextType targetType;
    }

    private List<GridObject> allTextObjects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ClearAllTextObjects()
    {
        allTextObjects.Clear();
    }

    public void RegisterTextObject(GridObject textObj)
    {
        if (textObj == null || textObj.type != GridObject.ObjectType.Text) return;
        if (!allTextObjects.Contains(textObj))
            allTextObjects.Add(textObj);
    }

    public void RefreshAllRules()
    {
        if (allTextObjects.Count == 0) return;

        List<GridObject> allObjects = LevelManager.Instance.GetAllObjects();
        foreach (var obj in allObjects)
            obj.ResetRuleProperties();

        List<ValidRule> validRules = ScanAllValidRules();
        ApplyRules(validRules, allObjects);
    }

    private List<ValidRule> ScanAllValidRules()
    {
        List<ValidRule> rules = new List<ValidRule>();
        Dictionary<Vector2Int, List<GridObject>> textPosDic = new();

        foreach (var textObj in allTextObjects)
        {
            Vector2Int pos = textObj.TargetGridPos;
            if (!textPosDic.ContainsKey(pos))
                textPosDic[pos] = new List<GridObject>();
            textPosDic[pos].Add(textObj);
        }

        // 横向规则
        foreach (var textObj in allTextObjects)
        {
            if (textObj.textType != GridObject.TextType.Noun) continue;
            Vector2Int currentPos = textObj.TargetGridPos;
            Vector2Int isPos = currentPos + Vector2Int.right;
            Vector2Int targetPos = isPos + Vector2Int.right;

            if (textPosDic.TryGetValue(isPos, out var isList) && textPosDic.TryGetValue(targetPos, out var targetList))
            {
                foreach (var isText in isList)
                {
                    if (isText.textContent != GridObject.TextContent.Is) continue;
                    foreach (var targetText in targetList)
                    {
                        rules.Add(new ValidRule
                        {
                            noun = textObj.textContent,
                            target = targetText.textContent,
                            targetType = targetText.textType
                        });
                    }
                }
            }
        }

        // 竖向规则
        foreach (var textObj in allTextObjects)
        {
            if (textObj.textType != GridObject.TextType.Noun) continue;
            Vector2Int currentPos = textObj.TargetGridPos;
            Vector2Int isPos = currentPos + Vector2Int.down;
            Vector2Int targetPos = isPos + Vector2Int.down;

            if (textPosDic.TryGetValue(isPos, out var isList) && textPosDic.TryGetValue(targetPos, out var targetList))
            {
                foreach (var isText in isList)
                {
                    if (isText.textContent != GridObject.TextContent.Is) continue;
                    foreach (var targetText in targetList)
                    {
                        rules.Add(new ValidRule
                        {
                            noun = textObj.textContent,
                            target = targetText.textContent,
                            targetType = targetText.textType
                        });
                    }
                }
            }
        }

        return rules;
    }

    private void ApplyRules(List<ValidRule> validRules, List<GridObject> allObjects)
    {
        // 第一遍：名词转换
        foreach (var rule in validRules)
        {
            if (rule.targetType != GridObject.TextType.Noun) continue;

            List<GridObject> targetObjects = LevelManager.Instance.GetObjectsByNoun(rule.noun);
            GridObject.ObjectType newType = NounToObjectType(rule.target);
            foreach (var obj in targetObjects)
            {
                if (newType != GridObject.ObjectType.Empty && obj.type != newType)
                {
                    obj.type = newType;
                    obj.RefreshSpriteOnly();
                }
            }
        }

        // 第二遍：属性赋予
        foreach (var rule in validRules)
        {
            if (rule.targetType != GridObject.TextType.Property) continue;

            List<GridObject> targetObjects = LevelManager.Instance.GetObjectsByNoun(rule.noun);
            foreach (var obj in targetObjects)
            {
                switch (rule.target)
                {
                    case GridObject.TextContent.You: obj.isYou = true; break;
                    case GridObject.TextContent.Stop: obj.isStop = true; break;
                    case GridObject.TextContent.Push: obj.isPush = true; break;
                    case GridObject.TextContent.Win: obj.isWin = true; break;
                }
            }
        }
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