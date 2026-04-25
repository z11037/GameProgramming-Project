using UnityEngine;
using System.Collections.Generic;

// 单个关卡的完整数据
[System.Serializable]
public class LevelData
{
    public string levelName;
    public int levelWidth;
    public int levelHeight;
    public List<LevelObjectData> objects; // 仅存普通物体：Wall/Rock/Man/Cherry
    public List<LevelTextData> textObjects; // 单独List存所有文字方块
}

// 普通物体数据（墙、玩家、樱桃等）
[System.Serializable]
public class LevelObjectData
{
    public string objectType; // Wall/Rock/Man/Cherry
    public int gridX;
    public int gridY;
}

// 文字方块专属数据（单独拆分，字段更精简）
[System.Serializable]
public class LevelTextData
{
    public int gridX;
    public int gridY;
    public string textType; // Noun/Verb/Property
    public string textContent; // Man/Wall/Rock/Cherry/Is/You/Stop/Push/Win
}

// 所有关卡的列表容器
[System.Serializable]
public class LevelDataList
{
    public List<LevelData> levels;
}