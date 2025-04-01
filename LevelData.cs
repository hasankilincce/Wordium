using System;

[Serializable]
public class LevelData
{
    public string level;
    public string category;
    public string cellData;
    public int moveCount;
}

// JSON’da liste formatını doğru işlemek için bir sarmalayıcı (wrapper)
[Serializable]
public class LevelDataListWrapper
{
    public System.Collections.Generic.List<LevelData> items;
}
