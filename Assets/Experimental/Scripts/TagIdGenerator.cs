using System.Collections.Generic;
using UnityEngine;

public static class TagIdGenerator
{
    private static readonly Dictionary<string, int> tagNameToId = new();
    private static readonly Dictionary<int, string> idToTagName = new();
    private static int nextId = 1;

    /// <summary>
    /// Genera o restituisce l'ID esistente per un nome di tag
    /// </summary>
    public static int GetOrCreateTagId(string tagName)
    {
        if (string.IsNullOrEmpty(tagName))
        {
            Debug.LogWarning("Tag name is null or empty, cannot generate ID.");
            return 0;
        }

        if (tagNameToId.TryGetValue(tagName, out int existingId))
        {
            return existingId;
        }

        int newId = nextId++;
        tagNameToId[tagName] = newId;
        idToTagName[newId] = tagName;
        
        Debug.Log($"Generated new tag ID: {newId} for tag: {tagName}");
        return newId;
    }

    /// <summary>
    /// Restituisce il nome del tag dall'ID
    /// </summary>
    public static string GetTagName(int tagId)
    {
        return idToTagName.TryGetValue(tagId, out string tagName) ? tagName : null;
    }

    /// <summary>
    /// Restituisce l'ID del tag dal nome
    /// </summary>
    public static int GetTagByName(string tagName)
    {
        return tagNameToId.TryGetValue(tagName, out int tagId) ? tagId : 0;
    }

    /// <summary>
    /// Verifica se esiste un tag con l'ID specificato
    /// </summary>
    public static bool HasTagId(int tagId)
    {
        return idToTagName.ContainsKey(tagId);
    }

    /// <summary>
    /// Verifica se esiste un tag con il nome specificato
    /// </summary>
    public static bool HasTagName(string tagName)
    {
        return tagNameToId.ContainsKey(tagName);
    }

    /// <summary>
    /// Restituisce tutti i tag registrati
    /// </summary>
    public static Dictionary<string, int> GetAllTags()
    {
        return new Dictionary<string, int>(tagNameToId);
    }

    /// <summary>
    /// Resetta tutti gli ID generati (utile per testing)
    /// </summary>
    public static void Reset()
    {
        tagNameToId.Clear();
        idToTagName.Clear();
        nextId = 1;
    }
}