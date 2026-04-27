using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public int levelWidth;
    public int levelHeight;
    public List<LevelObjectData> objects;
    public List<LevelTextData> textObjects;
}

[System.Serializable]
public class LevelObjectData
{
    public string objectType;
    public int gridX;
    public int gridY;
}

[System.Serializable]
public class LevelTextData
{
    public int gridX;
    public int gridY;
    public string textType;
    public string textContent;
}

[System.Serializable]
public class LevelDataList
{
    public List<LevelData> levels;
}