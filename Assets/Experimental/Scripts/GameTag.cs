using System.Collections.Generic;
using UnityEngine;

public class GameTag : MonoBehaviour
{
    public enum TagType
    {
        Random,
        Sequential,
    };

    public string tagName;
    public TagType tagType;
    public List<GameObject> taggedObjects = new();

    public ITagIterator iteratorStrategy;

    void Awake()
    {
        InitializeIterator();
    }
    
    private void InitializeIterator()
    {
        switch (tagType)
        {
            case TagType.Random:
                iteratorStrategy = new RandomTagIterator(taggedObjects);
                break;
            case TagType.Sequential:
                iteratorStrategy = new SequentialTagIterator(taggedObjects);
                break;
        }
    }

    public void UpdateIterator()
    {
        InitializeIterator();
    }

    public GameObject NextElement()
    {
        return iteratorStrategy?.NextElement();
    }

    // Aggiunge un oggetto alla lista dei tagged objects e aggiorna l'iteratore
    public void AddTaggedObject(GameObject obj)
    {
        if (obj != null && !taggedObjects.Contains(obj))
        {
            taggedObjects.Add(obj);
            UpdateIterator();
        }
    }
    
    // Rimuove tutti gli oggetti dalla lista e aggiorna l'iteratore
    public void ClearTaggedObjects()
    {
        taggedObjects.Clear();
        UpdateIterator();
    }

}

public interface ITagIterator
{
    GameObject NextElement();
}