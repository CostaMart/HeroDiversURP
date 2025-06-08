using System.Collections.Generic;

public interface IIterator
{
    T NextElement<T>(List<T> list);
}