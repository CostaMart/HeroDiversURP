using System;
using System.Collections.Generic;

[Serializable]
public class PoolConfig
{
    public List<PoolItem> poolItems;
}

[Serializable]
public class PoolItem
{
    public int id;
    public string prefabName;
    public int initialSize;
}