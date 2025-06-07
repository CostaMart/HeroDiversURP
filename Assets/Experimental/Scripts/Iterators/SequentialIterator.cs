using System.Collections.Generic;

public class SequentialIterator : IIterator
{
    private int currentIndex = -1;
    
    public T NextElement<T>(List<T> objects)
    {
        if (objects == null || objects.Count == 0)
            return default;
            
        currentIndex = (currentIndex + 1) % objects.Count;
        return objects[currentIndex];
    }
}