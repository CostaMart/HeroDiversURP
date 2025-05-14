using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TagConfiguration
{
    public string name;
    public List<string> members = new();
    public string type;
}

[System.Serializable]
public class TagsConfiguration
{
    public List<TagConfiguration> tags = new();
}

public class TagConfigLoader : MonoBehaviour
{
    public TextAsset configFile;
    
    void Start()
    {
        configFile = Resources.Load<TextAsset>("tags");
        if (configFile == null)
        {
            Debug.LogError("Config file not found in Resources folder.");
            return;
        }
        if (configFile != null)
        {
            LoadTagsFromConfig();
        }
    }
    
    public void LoadTagsFromConfig()
    {
        string json = configFile.text;
        TagsConfiguration config = JsonUtility.FromJson<TagsConfiguration>(json);
        
        foreach (TagConfiguration tagConfig in config.tags)
        {
            // Trova o crea il tag
            GameTag tag = FindOrCreateTag(tagConfig);
            
            // Configura gli oggetti del tag
            ConfigureTaggedObjects(tag, tagConfig);
        }
    }
    
    private GameTag FindOrCreateTag(TagConfiguration tagConfig)
    {
        // Cerca un tag esistente con lo stesso nome
        GameObject tagObject = EntityManager.Instance.GetEntity(tagConfig.name);
        if (tagObject && tagObject.TryGetComponent(out GameTag existingTag))
        {
            Debug.Log($"Found existing tag: {existingTag.tagName}");
            return existingTag;
        }

        tagObject = new GameObject(tagConfig.name);

        // Aggiungi il componente appropriato in base al tipo
        GameTag newTag = tagObject.AddComponent<GameTag>();
        newTag.tagName = tagConfig.name;
        newTag.tagType = (GameTag.TagType)System.Enum.Parse(typeof(GameTag.TagType), tagConfig.type);

        EntityManager.Instance.RegisterEntity(newTag.tagName, tagObject);
        return newTag;
    }
    
    private void ConfigureTaggedObjects(GameTag tag, TagConfiguration tagConfig)
    {
        // Pulisci la lista corrente
        // tag.ClearTaggedObjects();
        
        // Aggiungi gli oggetti specificati per nome
        foreach (string objectName in tagConfig.members)
        {
            GameObject obj = EntityManager.Instance.GetEntity(objectName);
            if (obj != null)
            {
                tag.AddTaggedObject(obj);
            }
            else
            {
                Debug.LogWarning($"Object '{objectName}' not found for tag '{tagConfig.name}'");
            }
        }
    }
}