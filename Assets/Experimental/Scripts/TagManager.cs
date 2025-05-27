// Esempio concettuale in C#
using System.Collections.Generic;
using UnityEngine;

public class TagManager : MonoBehaviour
{
    // Singleton instance
    public static TagManager Instance { get; private set; }

    // Dizionario principale: la chiave è il nome del Tag (stringa),
    // il valore è l'istanza di Tag.
    readonly Dictionary<string, GameTag> _tagRegistry = new();

    // Dizionario per tracciare a quali Tag un GameObject appartiene.
    // Chiave: ID dell'oggetto di gioco.
    // Valore: Lista dei nomi dei Tag a cui l'oggetto è associato.
    // readonly Dictionary<string, HashSet<string>> _objectToTagNames = new();

    // Loader per la configurazione dei Tag
    TagConfigLoader _tagConfigLoader;

    void Awake()
    {
        // Inizializza il TagManager se non esiste già
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!TryGetComponent<TagConfigLoader>(out _tagConfigLoader))
        {
            // Crea un nuovo TagConfigLoader se non esiste
            _tagConfigLoader = gameObject.AddComponent<TagConfigLoader>();
        }
    }

    private void Start()
    {
        // Carica i tag da un file di configurazione, se necessario
        _tagConfigLoader.LoadTagsFromConfig();
    }

    // --- Gestione dei Tag ---
    public void RegisterTag(GameTag tag)
    {
        if (!_tagRegistry.ContainsKey(tag.name))
        {
            _tagRegistry.Add(tag.name, tag);
        }
        else
        {
            // Gestire il caso di nomi di tag duplicati (es. lanciare eccezione o loggare un avviso)
            Debug.LogWarning($"Attenzione: Tag con nome '{tag.name}' già registrato.");
        }
    }

    public GameTag GetTag(string tagName)
    {
        if (_tagRegistry.TryGetValue(tagName, out GameTag tagObj))
        {
            return tagObj;
        }
        return null; // O lanciare un'eccezione se il tag non esiste
    }

    // --- Associazione Oggetti a Tag ---
    public void AddObjectToTag(GameObject obj, string tagName)
    {
        GameTag tag = GetTag(tagName);
        if (tag != null)
        {
            tag.AddObject(obj); // Aggiunge l'oggetto alla lista interna del Tag

            // Aggiorna il tracciamento inverso
            // if (!_objectToTagNames.ContainsKey(obj.name))
            // {
            //     _objectToTagNames[obj.name] = new HashSet<string>();
            // }
            // _objectToTagNames[obj.name].Add(tagName);
        }
        else
        {
            // Gestire il caso in cui il Tag non esista o il tipo non corrisponda
            Debug.LogError($"Errore: Impossibile aggiungere '{obj.name}' a Tag '{tagName}'.");
        }
    }

    public void RemoveObjectFromTag(GameObject obj, string tagName)
    {
        GameTag tag = GetTag(tagName);
        if (tag != null)
        {
            tag.RemoveObject(obj);

            // Aggiorna il tracciamento inverso
            // if (_objectToTagNames.TryGetValue(obj.name, out HashSet<string> tagNames))
            // {
            //     tagNames.Remove(tagName);
            //     if (tagNames.Count == 0)
            //     {
            //         _objectToTagNames.Remove(obj.name);
            //     }
            // }
        }
    }

    // --- Query ---
    public List<GameObject> GetObjectsInTag(string tagName)
    {
        GameTag tag = GetTag(tagName);
        return tag.taggedObjects ?? new List<GameObject>();
    }

    // public HashSet<string> GetTagsForObject(GameObject obj)
    // {
    //     if (_objectToTagNames.TryGetValue(obj.name, out HashSet<string> tagNames))
    //     {
    //         return new HashSet<string>(tagNames); // Restituisce una copia per evitare modifiche esterne
    //     }
    //     return new HashSet<string>();
    // }

    public bool IsObjectInTag(GameObject obj, string tagName)
    {
        GameTag tag = GetTag(tagName);
        return tag != null && tag.Contains(obj);
    }
}