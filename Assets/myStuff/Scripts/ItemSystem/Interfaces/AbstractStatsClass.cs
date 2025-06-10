using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-99)]
public abstract class AbstractStatsClass : MonoBehaviour
{
    // ====== FIELDS ======
    protected string symbolicName = null;
    public Dictionary<int, Feature> features = new();

    private List<AbstractEffect> activeEffects = new List<AbstractEffect>();
    private List<AbstractEffect> effectsToRemove = new List<AbstractEffect>();

    public bool dirty = false;
    public int ID { get; private set; }

    public EffectsDispatcher mydispatcher = null;

    // ====== MONOBEHAVIOUR LIFECYCLE ======
    protected virtual void Awake()
    {
        if (gameObject.name.Contains("Clone"))
            gameObject.name = gameObject.name.Replace("(Clone)", "");

        var couple = ComputeID();
        this.ID = couple.Item1;
        this.symbolicName = couple.Item2;
        features = LoadFeatures();
        Debug.Log("Assigning ID to status class " + this.GetType().Name);
        this.dirty = true;
    }

    public virtual void OnEnable()
    {
        mydispatcher = FindEffectDispatcherInParents(transform);
        if (mydispatcher != null)
        {
            mydispatcher.AttachStatusClass(this, true);
            Debug.Log("dispatcher found and registered");
        }
        else
        {
            Debug.LogError("dispatcher not found in the hierarchy of " + this.gameObject.name);
        }
    }

    public virtual void OnDisable() { }

    protected virtual void Update()
    {
        ActivateEffects();
        foreach (var ef in effectsToRemove)
            activeEffects.Remove(ef);

        effectsToRemove.Clear();
    }

    // ====== INIZIALIZZAZIONE / PARSING ======
    public AbstractStatsClass() { }

    protected virtual (int, string) ComputeID()
    {
        var name = this.GetType().Name;
        var ID = ItemManager.statClassToIdRegistry[name];
        return (ID, name);
    }

    public Dictionary<int, Feature> LoadFeatures()
    {
        string name = this.symbolicName;
        int hash = (this.gameObject.name + "-" + name).Trim().ToLower().GetHashCode();
        return ItemManager.featuresSets[hash];
    }

    // ====== GESTIONE DELLE FEATURE ======
    public void SetStatByID(int id, object newValue)
    {
        features[id].SetValue(Convert.ChangeType(newValue, features[id].currentValue.GetType()));
        dirty = true;
    }

    public T GetStatByID<T>(int id)
    {
        try
        {
            return (T)features[id].currentValue;
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError($"GetStatByID: Feature ID {id} not found in {this.GetType().Name} on {this.gameObject.name}");
        }
        catch (InvalidCastException)
        {
            Debug.LogError($"GetStatByID: Invalid cast for ID {id} in {this.GetType().Name}");
        }

        return default(T);
    }

    public T[] GetFeatureValuesByType<T>(FeatureType type)
    {
        List<T> values = new List<T>();

        foreach (var feature in features.Values)
        {
            if (feature.id == type)
                values.Add((T)feature.currentValue);
        }

        return values.ToArray();
    }

    // ====== GESTIONE EFFETTI ======
    public virtual void AttachEffect(AbstractEffect effect)
    {
        this.activeEffects.Add(effect);
    }

    // dal momento che C# non consente di rimuover oggetti da una lista durante l'iterazione,
    // si usa una lista temporanea per rimuovere gli effetti quando sono gli effetti stessi a richiedere l'eliminazione
    public virtual void RemoveEffect(AbstractEffect effect)
    {
        this.effectsToRemove.Add(effect);
    }

    protected virtual void ActivateEffects()
    {
        try
        {
            foreach (var effect in activeEffects)
            {
                int targetID = effect.targetAttributeID;

                if (!features.TryGetValue(targetID, out var target))
                    continue;

                if (target.type == typeof(int))
                {
                    int value = Convert.ToInt32(effect.Activate(this));
                    target.SetValue(value);
                }
                else if (target.type == typeof(float))
                {
                    float result = Convert.ToSingle(effect.Activate(this));
                    result = result != -1 ? result : (float)target.currentValue;
                    target.SetValue(result);
                }
                else if (target.type == typeof(bool))
                {
                    bool result = (bool)effect.Activate(this);
                    target.SetValue(result);
                }
                else
                {
                    throw new ArgumentException("Invalid type: " + target.type);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("AbstractStatus: Error during ActivateEffects - " + e.Message);
        }
    }

    // ====== UTILITÀ ======
    private static EffectsDispatcher FindEffectDispatcherInParents(Transform start)
    {
        Transform current = start;
        while (current != null)
        {
            EffectsDispatcher dispatcher = current.GetComponent<EffectsDispatcher>();
            if (dispatcher != null)
                return dispatcher;

            current = current.parent;
        }

        return null;
    }
}