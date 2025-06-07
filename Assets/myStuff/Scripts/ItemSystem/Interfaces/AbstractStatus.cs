using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-99)]
public abstract class AbstractStatus : MonoBehaviour
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

        ID = ComputeID();
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
    public AbstractStatus() { }

    protected abstract int ComputeID();

    public Dictionary<int, Feature> LoadFeatures()
    {
        string[] lines = File.ReadAllLines(Application.streamingAssetsPath + "/gameConfig/Features.txt");
        bool found = false;
        bool hasBeenFound = false;
        Dictionary<int, Feature> features = new();

        foreach (var thisLine in lines)
        {
            var line = thisLine.Trim();
            line = line.Split("//")[0];

            if (found && line.Contains("##")) found = false;

            if (found)
            {
                try
                {
                    string[] parts = line.Split("=");
                    FeatureType featureType = (FeatureType)Enum.Parse(typeof(FeatureType), parts[0]);

                    if (featureType == FeatureType.money || featureType == FeatureType.keys)
                    {
                        Debug.LogWarning("AbstractStatus: money and keys are special features, not customizable");
                        continue;
                    }

                    parts = parts[1].Split("ID:");
                    string value = parts[0].Trim();
                    int ID = int.Parse(parts[1].Split("range:")[0].Trim());

                    string rangeString = (parts[1].Split("range:").Length > 1) ? parts[1].Split("range:")[1].Trim() : null;
                    string maxVal = null, minVal = null;

                    if (rangeString != null)
                    {
                        maxVal = rangeString.Split("-")[1].Trim();
                        minVal = rangeString.Split("-")[0].Trim();
                    }

                    if (int.TryParse(value, out _))
                    {
                        Feature f = new Feature(featureType, int.Parse(value), typeof(int));
                        f.SetValue(int.Parse(value));
                        if (maxVal != null)
                        {
                            f.maxVal = int.Parse(maxVal);
                            f.minVal = int.Parse(minVal);
                        }
                        features.Add(ID, f);
                    }
                    else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                    {
                        Feature f = new Feature(featureType, float.Parse(value, System.Globalization.CultureInfo.InvariantCulture), typeof(float));
                        f.SetValue(float.Parse(value.Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture));
                        if (maxVal != null)
                        {
                            f.maxVal = float.Parse(maxVal, System.Globalization.CultureInfo.InvariantCulture);
                            f.minVal = float.Parse(minVal, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        features.Add(ID, f);
                    }
                    else
                    {
                        Feature f = new Feature(featureType, bool.Parse(value), typeof(bool));
                        f.SetValue(bool.Parse(value));
                        features.Add(ID, f);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Abstract Status: Error in parsing line: " + line + " " + e.Message);
                }
            }

            var name = symbolicName != null ? symbolicName : this.GetType().Name;
            if (line.Contains("#" + this.gameObject.name + "-" + name))
            {
                found = true;
                hasBeenFound = true;
            }
        }

        return hasBeenFound ? features : new Dictionary<int, Feature>();
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