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
    public List<GameObject> activeObjects = new();
    public List<string> trackedObjectNames = new();

    public IIterator iteratorStrategy;

    protected override void Awake()
    {
        InitializeIterator();
    }

    void Start()
    {
        // Registra eventi per gestire l'aggiunta e la rimozione di oggetti
        RegisterAction(ActionRegistry.ADD_ENABLED_OBJECT, AddEnabledObjectAction);
        RegisterAction(ActionRegistry.REMOVE_DISABLED_OBJECT, RemoveDisabledObjectAction);
    }

    private void InitializeIterator()
    {
        iteratorStrategy = tagType switch
        {
            TagType.Random => new RandomIterator(),
            TagType.Sequential => new SequentialIterator(),
            _ => null
        };
    }

    public GameObject GetActiveObject()
    {
        return iteratorStrategy?.NextElement(activeObjects);
    }

    public GameObject CreateNextObject(Vector3 position, Quaternion rotation)
    {
        if (trackedObjectNames.Count == 0)
        {
            Debug.LogWarning("No tracked object names available to create an object.");
            return null;
        }

        string objName = iteratorStrategy?.NextElement(trackedObjectNames);

        return ObjectPool.Instance.Get(objName, position, rotation);
    }

    // Aggiunge un oggetto alla lista dei tagged objects e aggiorna l'iteratore
    public void AddObject(GameObject obj)
    {
        if (obj != null && !activeObjects.Contains(obj))
        {
            activeObjects.Add(obj);
        }
    }

    // Ascolta l'evento di creazione di un oggetto non ancora creato
    public void RegisterToObjectEnableEvent(string objName)
    {
        if (string.IsNullOrEmpty(objName))
        {
            Debug.LogWarning("Object name is null or empty, cannot register for create event.");
            return;
        }

        EventConfiguration configuration = new()
        {
            name = EventRegistry.OBJECT_ENABLED.name,
            emitterFilters = new List<string> { objName },
            actions = new List<ActionConfig>
            {
                new()
                {
                    action = ActionRegistry.ADD_ENABLED_OBJECT.name,
                    tag = tagName,
                    isTagAction = true,
                }
            }
        };

        EventActionManager.Instance.SetEventConfiguration(configuration);
    }

    // Registra questo tag per ascoltare l'evento di distruzione dell'oggetto
    public void RegisterToObjectDisableEvents(string objName)
    {
        if (string.IsNullOrEmpty(objName))
        {
            Debug.LogWarning("Object name is empty, cannot register for destroy event.");
            return;
        }

        EventConfiguration configuration = new()
        {
            name = EventRegistry.OBJECT_DISABLED.name,
            emitterFilters = new List<string> { objName },
            actions = new List<ActionConfig>
            {
                new() {
                    action = ActionRegistry.REMOVE_DISABLED_OBJECT.name,
                    tag = tagName,
                    isTagAction = true,
                }
            }
        };
        EventActionManager.Instance.SetEventConfiguration(configuration);
    }

    // Azione che viene chiamata quando un oggetto viene creato
    private void AddEnabledObjectAction(object[] parameters)
    {
        if (parameters != null && parameters.Length > 0 && parameters[0] is GameObject createdObj)
        {
            AddObject(createdObj);
        }
    }

    // Azione che viene chiamata quando un oggetto viene distrutto o disabilitato
    private void RemoveDisabledObjectAction(object[] parameters)
    {
        if (parameters != null && parameters.Length > 0 && parameters[0] is GameObject destroyedObj)
        {
            RemoveObject(destroyedObj);
            Debug.Log($"GameTag '{tagName}' removed object: {destroyedObj.name}");
        }
    }

    public bool Contains(GameObject obj)
    {
        return activeObjects.Contains(obj);
    }

    public GameObject[] GetObjects()
    {
        return activeObjects.ToArray();
    }

    public void RemoveObject(GameObject obj)
    {
        if (activeObjects.Contains(obj))
        {
            activeObjects.Remove(obj);
        }
    }


    // I tag non emettono eventi di OnEnable e OnDisable
    protected override void OnEnable() { }
    protected override void OnDisable() { }
}