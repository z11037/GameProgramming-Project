using UnityEngine;
using System.Collections.Generic;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance { get; private set; }

    // ========== 完整的ValidRule类定义 ==========
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

    // ========== 新增：清空文字列表的方法 ==========
    public void ClearAllTextObjects()
    {
        allTextObjects.Clear();
        Debug.Log("【RuleManager】已清空所有文字方块注册");
    }

    public void RegisterTextObject(GridObject textObj)
    {
        if (textObj == null || textObj.type != GridObject.ObjectType.Text) return;
        if (!allTextObjects.Contains(textObj))
        {
            allTextObjects.Add(textObj);
        }
    }

    public void RefreshAllRules()
    {
        if (allTextObjects.Count == 0)
        {
            Debug.LogWarning("【RuleManager】无注册的文字物体，跳过规则刷新");
            return;
        }

        Debug.Log("【RuleManager】开始刷新规则");
        List<GridObject> allObjects = LevelManager.Instance.GetAllObjects();

        // 先重置所有物体的规则属性
        foreach (var obj in allObjects)
        {
            obj.ResetRuleProperties();
        }

        // 扫描有效规则
        List<ValidRule> validRules = ScanAllValidRules();
        Debug.Log($"【RuleManager】扫描到有效规则数量：{validRules.Count}");

        // 应用规则
        ApplyRules(validRules, allObjects);
    }

    // ========== 完整的ScanAllValidRules方法 ==========
    private List<ValidRule> ScanAllValidRules()
    {
        List<ValidRule> rules = new List<ValidRule>();
        Dictionary<Vector2Int, GridObject> textPosDic = new();

        foreach (var textObj in allTextObjects)
        {
            Vector2Int pos = textObj.TargetGridPos;
            if (!textPosDic.ContainsKey(pos))
            {
                textPosDic[pos] = textObj;
            }
        }

        // 横向规则扫描（左→右）
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
                    Debug.Log($"【RuleManager】发现规则：{textObj.textContent} IS {targetText.textContent}");
                }
            }
        }

        // 纵向规则扫描（上→下）
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
                    Debug.Log($"【RuleManager】发现规则：{textObj.textContent} IS {targetText.textContent}");
                }
            }
        }

        return rules;
    }

    // ========== 完整的ApplyRules方法 ==========
    private void ApplyRules(List<ValidRule> validRules, List<GridObject> allObjects)
    {
        foreach (var rule in validRules)
        {
            List<GridObject> targetObjects = LevelManager.Instance.GetObjectsByNoun(rule.noun);

            foreach (var obj in targetObjects)
            {
                if (rule.targetType == GridObject.TextType.Property)
                {
                    switch (rule.target)
                    {
                        case GridObject.TextContent.You:
                            obj.isYou = true;
                            Debug.Log($"【RuleManager】设置isYou：{obj.name}");
                            break;
                        case GridObject.TextContent.Stop:
                            obj.isStop = true;
                            Debug.Log($"【RuleManager】设置isStop：{obj.name}");
                            break;
                        case GridObject.TextContent.Push:
                            obj.isPush = true;
                            Debug.Log($"【RuleManager】设置isPush：{obj.name}");
                            break;
                        case GridObject.TextContent.Win:
                            obj.isWin = true;
                            Debug.Log($"【RuleManager】设置isWin：{obj.name}");
                            break;
                    }
                }
                else if (rule.targetType == GridObject.TextType.Noun)
                {
                    // 物体类型转化逻辑（保留你之前的原有逻辑）
                    GridObject.ObjectType newType = NounToObjectType(rule.target);
                    if (newType != GridObject.ObjectType.Empty && obj.type != newType)
                    {
                        obj.type = newType;
                        // 这里可以加Sprite同步逻辑，保留你之前的原有代码
                        Debug.Log($"【RuleManager】物体类型转化：{obj.name} → {newType}");
                    }
                }
            }
        }
    }

    // ========== 辅助方法：名词转物体类型 ==========
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
}