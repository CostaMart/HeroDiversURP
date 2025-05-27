using System.Collections.Generic;
using UnityEngine;

public class GameTag : InteractiveObject
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

    protected override void Start()
    {
        // Non emette l'evento di creazione, poiché è un tag

        // Registra eventi per gestire l'aggiunta e la rimozione di oggetti
        RegisterAction("AddCreatedObject", AddCreatedObjectAction);
        RegisterAction("RemoveDestroyedObject", RemoveDestroyedObjectAction);

        // Registra agli eventi di distruzione per gli oggetti già presenti nella lista
        foreach (var obj in taggedObjects)
        {
            if (obj != null)
            {
                RegisterToObjectDestroyEvent(obj);
            }
        }

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
    public void AddObject(GameObject obj)
    {
        if (obj != null && !taggedObjects.Contains(obj))
        {
            taggedObjects.Add(obj);
            RegisterToObjectDestroyEvent(obj);
            UpdateIterator();
        }
    }

    // Ascolta l'evento di creazione di un oggetto non ancora creato
    public void RegisterToObjectCreateEvent(string objName)
    {
        if (string.IsNullOrEmpty(objName))
        {
            Debug.LogWarning("Object name is null or empty, cannot register for create event.");
            return;
        }

        EventConfiguration configuration = new()
        {
            name = "OnCreate" + objName,
            actions = new List<ActionConfig>
            {
                new()
                {
                    action = "AddCreatedObject",
                    tag = tagName,
                    isTagAction = true,
                }
            }
        };

        EventActionManager.Instance.SetEventConfiguration(configuration);
        // if (!_trackedObjectNames.Contains(objName))
        // {
        //     string createEventName = "OnCreate" + objName;
        //     _trackedObjectNames.Add(objName);

        //     // Registra l'evento nel sistema se non esiste già
        //     EventActionManager.Instance.RegisterEvent(createEventName);

        //     Debug.Log($"GameTag '{tagName}' registered to listen for create event: {createEventName}");
        // }
    }

    // Registra questo tag per ascoltare l'evento di distruzione dell'oggetto
    private void RegisterToObjectDestroyEvent(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Object is null, cannot register for destroy event.");
            return;
        }
        EventConfiguration configuration = new()
        {
            name = "OnDestroy" + obj.name,
            actions = new List<ActionConfig>
            {
                new() {
                    action = "RemoveDestroyedObject",
                    tag = tagName,
                    isTagAction = true,
                }
            }
        };
        EventActionManager.Instance.SetEventConfiguration(configuration);
        // if (obj != null && !_trackedObjectNames.Contains(obj.name))
        // {
        //     string destroyEventName = "OnDestroy" + obj.name;
        //     _trackedObjectNames.Add(obj.name);

        //     // Registra l'evento nel sistema se non esiste già
        //     EventActionManager.Instance.RegisterEvent(destroyEventName);

        //     Debug.Log($"GameTag '{tagName}' registered to listen for destroy event: {destroyEventName}");
        // }
    }

    // Azione che viene chiamata quando un oggetto viene creato
    private void AddCreatedObjectAction(object[] parameters)
    {
        if (parameters != null && parameters.Length > 0 && parameters[0] is GameObject createdObj)
        {
            AddObject(createdObj);
            Debug.Log($"GameTag '{tagName}' added created object: {createdObj.name}");
        }
    }

    // Azione che viene chiamata quando un oggetto viene distrutto
    private void RemoveDestroyedObjectAction(object[] parameters)
    {
        if (parameters != null && parameters.Length > 0 && parameters[0] is GameObject destroyedObj)
        {
            RemoveObject(destroyedObj);
            Debug.Log($"GameTag '{tagName}' removed destroyed object: {destroyedObj.name}");
        }
    }

    // Rimuove tutti gli oggetti dalla lista e aggiorna l'iteratore
    public void ClearObjects()
    {
        taggedObjects.Clear();
        UpdateIterator();
    }

    public bool Contains(GameObject obj)
    {
        return taggedObjects.Contains(obj);
    }

    public GameObject[] GetObjects()
    {
        return taggedObjects.ToArray();
    }

    public void RemoveObject(GameObject obj)
    {
        if (taggedObjects.Contains(obj))
        {
            taggedObjects.Remove(obj);
            // if (obj != null)
            // {
            //     _trackedObjectNames.Remove(obj.name);
            // }
            UpdateIterator();
        }
    }
}

public interface ITagIterator
{
    GameObject NextElement();
}