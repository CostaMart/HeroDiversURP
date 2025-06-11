using System.Collections.Generic;
using System.IO;
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
    private string configContent;
    
    void ConfigFile()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "tags.json");
        if (File.Exists(filePath))
        {
            configContent = File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogError("Config file not found in StreamingAssets folder.");
            configContent = string.Empty;
        }
    }
    
    public void LoadTagsFromConfig()
    {
        ConfigFile();
        if (string.IsNullOrEmpty(configContent))
        {
            Debug.LogError("No config content available.");
            return;
        }

        TagsConfiguration config = JsonUtility.FromJson<TagsConfiguration>(configContent);

        foreach (TagConfiguration tagConfig in config.tags)
        {
            // Genera l'ID per il tag
            int tagId = TagIdGenerator.GetOrCreateTagId(tagConfig.name);
            Debug.Log($"Processing tag: {tagConfig.name} with ID: {tagId}");

            // Trova o crea il tag
            GameTag tag = FindOrCreateTag(tagId, tagConfig);

            // Configura gli oggetti del tag
            ConfigureTaggedObjects(tag, tagConfig);
        }
    }

    private GameTag FindOrCreateTag(int tagId, TagConfiguration tagConfig)
    {
        // Cerca un tag esistente con lo stesso nome
        if (TagManager.Instance.GetTag(tagId) is GameTag existingTag)
        {
            // Se esiste, ritorna l'istanza esistente
            Debug.Log($"Found existing tag: {existingTag.tagName}");
            return existingTag;
        }

        GameObject tagObject = new(tagConfig.name);

        // Aggiungi il componente appropriato in base al tipo
        GameTag newTag = tagObject.AddComponent<GameTag>();
        newTag.SetTagName(tagConfig.name);
        if (string.IsNullOrEmpty(tagConfig.type))
        {
            newTag.tagType = GameTag.TagType.Random;
        }
        else
        {
            newTag.tagType = (GameTag.TagType)System.Enum.Parse(typeof(GameTag.TagType), tagConfig.type);
        }
        newTag.transform.SetParent(TagManager.Instance.transform);
        TagManager.Instance.RegisterTag(newTag);

        Debug.Log($"Created new tag: {newTag.tagName} with ID: {newTag.id}");
        return newTag;
    }
    
    private void ConfigureTaggedObjects(GameTag tag, TagConfiguration tagConfig)
    {
        // Aggiungi gli oggetti specificati per nome
        foreach (string objectName in tagConfig.members)
        {
            PoolObjectType obj = ObjectPool.Instance.GetTypeFromName(objectName);
            if (obj != PoolObjectType.None)
            {
                tag.pooledObjects.Add(obj);
                Debug.Log($"Added object '{objectName}' of type '{obj}' to tag '{tag.tagName}'");
            }
            tag.RegisterToObjectEnableEvent(objectName);
            tag.RegisterToObjectDisableEvents(objectName);
        }
    }
}