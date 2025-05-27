using System.Collections.Generic;
using UnityEngine;

public class RandomTagIterator : ITagIterator
{
    private List<GameObject> objects;
    
    public RandomTagIterator(List<GameObject> taggedObjects)
    {
        objects = taggedObjects;
    }
    
    public GameObject NextElement()
    {
        if (objects == null || objects.Count == 0)
            return null;
            
        int randomIndex = Random.Range(0, objects.Count);
        return objects[randomIndex];
    }
}
