using System.Collections.Generic;
using UnityEngine;

public class RandomIterator : IIterator
{
    public T NextElement<T>(List<T> objects)
    {
        if (objects == null || objects.Count == 0)
            return default;

        int randomIndex = Random.Range(0, objects.Count);
        return objects[randomIndex];
    }
}
