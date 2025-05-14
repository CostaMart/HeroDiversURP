using System.Collections.Generic;
using UnityEngine;

public class SequentialTagIterator : ITagIterator
{
    private List<GameObject> objects;
    private int currentIndex = -1;
    
    public SequentialTagIterator(List<GameObject> taggedObjects)
    {
        objects = taggedObjects;
    }
    
    public GameObject NextElement()
    {
        if (objects == null || objects.Count == 0)
            return null;
            
        currentIndex = (currentIndex + 1) % objects.Count;
        return objects[currentIndex];
    }
}