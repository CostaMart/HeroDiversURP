using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// A status class that wants to participate to the item upgrade system must implement this interface, 
/// the effect dispatcher will look for these classes in the gameobject hierarchy.
/// </summary>
public abstract class AbstractStatus : MonoBehaviour
{
    public Dictionary<int, Feature> features = new();

    private List<AbstractEffect> activeEffects = new List<AbstractEffect>();

    public bool dirty = false;

    private List<AbstractEffect> effectsToRemove = new List<AbstractEffect>();

    /// <summary>
    /// ID of this affectable type 
    /// </summary>
    public int ID { get; private set; }

    public AbstractStatus()
    {

    }

    public Dictionary<int, Feature> LoadFeatures()
    {

        string[] lines = File.ReadAllLines(Application.streamingAssetsPath + "/gameConfig/Features.txt");
        bool found = false;
        bool hasBeenFound = false;
        Dictionary<int, Feature> features = new();

        /// semplice parsing del file Features.txt per recuperare le features
        foreach (var thisLine in lines)
        {

            var line = thisLine.Trim();
            line = line.Split("//")[0];

            if (found && line.Contains("##"))
            {
                found = false;
            }


            if (found)
            {

                try
                {
                    string[] parts = line.Split("=");

                    Type t;
                    FeatureType featureType = (FeatureType)Enum.Parse(typeof(FeatureType), parts[0]);

                    if (featureType == FeatureType.money || featureType == FeatureType.keys)
                    {
                        Debug.LogWarning("AbstractStatus: money and keys are special features, not customizable");
                        continue;
                    }

                    Debug.Log("starting parsing ");
                    parts = parts[1].Split("ID:");

                    string value = parts[0].Trim();
                    int ID = int.Parse(parts[1].Split("range:")[0].Trim());


                    string rangeString = (parts[1].Split("range:").Length > 1) ? parts[1].Split("range:")[1].Trim() : null;
                    string maxVal = null;
                    string minVal = null;

                    if (rangeString != null)
                    {
                        maxVal = rangeString.Split("-")[1].Trim();
                        minVal = rangeString.Split("-")[0].Trim();
                    }

                    var splittedComment = parts[1].Split('\\');

                    if (int.TryParse(parts[0], out _))
                    {
                        Debug.Log("gameobject" + this.gameObject.name + " parsed value: " + parts[0]
                         + " ID: " + parts[1] + " as int");

                        Feature f = new Feature(featureType, int.Parse(parts[0]), typeof(int));
                        f.SetValue(int.Parse(parts[0]));
                        if (maxVal != null)
                        {
                            f.maxVal = int.Parse(maxVal);
                            f.minVal = int.Parse(minVal);
                        }
                        features.Add(ID, f);
                    }
                    else if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                    {
                        Debug.Log("gameobject" + this.gameObject.name + " parsed value: " + parts[1]
                        + " ID: " + parts[0] + " as float");

                        Feature f = new Feature(featureType, float.Parse(parts[0],
                        System.Globalization.CultureInfo.InvariantCulture), typeof(float));
                        f.SetValue(float.Parse(parts[0].Replace(" ", ""),
                        System.Globalization.CultureInfo.InvariantCulture));

                        if (maxVal != null)
                        {
                            f.maxVal = float.Parse(maxVal, System.Globalization.CultureInfo.InvariantCulture);

                            f.minVal = float.Parse(minVal, System.Globalization.CultureInfo.InvariantCulture);
                        }

                        features.Add(ID, f);
                    }
                    else
                    {
                        Debug.Log("gameobject" + this.gameObject.name + " parsed value: " + parts[1] +
                         " ID: " + parts[0] + " as bool");

                        Feature f = new Feature(featureType, bool.Parse(parts[0]), typeof(bool));
                        f.SetValue(bool.Parse(parts[0]));
                        features.Add(ID, f);
                    }

                }
                catch
                (Exception e)
                {
                    Debug.LogError("Abstract Status: Error in parsing line: " + line + " " + e.Message);
                }

            }

            if (line.Contains("#" + this.gameObject.name + "-" + this.GetType().Name))
            {
                found = true;
                hasBeenFound = true;
            }
        }


        if (hasBeenFound)
            return features;

        /// simply empty
        return new Dictionary<int, Feature>();


    }

    protected abstract int ComputeID();

    protected virtual void Update()
    {
        this.ActivateEffects();

        foreach (var ef in effectsToRemove)
        {
            activeEffects.Remove(ef);
        }

        effectsToRemove.Clear();
    }

    protected virtual void Awake()
    {
        features = LoadFeatures();
        Debug.Log("Assigning ID to status class " + this.GetType().Name);
        ID = ComputeID();
        new ItemManager();
        this.dirty = true;
    }

    /// <summary>
    /// This method shall apply new values to attributes of this class referenced by their ID
    /// <paramref name="id"/> ID of the attribute to change
    /// <paramref name="newValue"/> new value to apply
    /// </summary>
    public void SetStatByID(int id, object newValue)
    {
        features[id].SetValue(Convert.ChangeType(newValue, features[id].GetValue().GetType()));
        dirty = true;
    }

    /// <summary>
    /// This method shall resolve and return the value of a parameter by its ID
    /// <paramref name="id"/> ID of the parameter to resolve
    /// </summary>
    public T GetStatByID<T>(int id)
    {
        try
        {
            return (T)features[id].GetValue();
        }

        catch (KeyNotFoundException)
        {
            Debug.LogError("invoked GetStatByID of object: " + this.GetType().Name
            + " in gameobject " + this.gameObject.name + " for " + id +
            " but the feature as not been assigned");
        }
        catch (InvalidCastException)
        {
            Debug.LogError("invoked GestStatByID with id: " + id + " and type: " + typeof(T) +
            " but the value is of type: " + features[id].GetValue().GetType());
        }

        return default(T);
    }

    public T[] GetFeatureValuesByType<T>(FeatureType type)
    {
        List<T> values = new List<T>();

        foreach (var feature in features)
        {
            if (feature.Value.id == type)
            {
                values.Add((T)feature.Value.GetValue());
            }
        }

        return values.ToArray();
    }

    /// <summary>
    /// Attach an effect to this status class.
    /// </summary>
    /// <param name="effect"></param>
    public virtual void AttachEffect(AbstractEffect effect)
    {
        this.activeEffects.Add(effect);
    }

    /// <summary>
    /// Remove an effect from this status class.
    /// </summary>
    /// <param name="effect"></param>
    public virtual void RemoveEffect(AbstractEffect effect)
    {
        this.effectsToRemove.Add(effect);
    }

    /// <summary>
    /// Activate effect in the effect list
    /// </summary>
    protected virtual void ActivateEffects()
    {
        object toApply;
        try
        {

            foreach (var effect in activeEffects)
            {
                int targetID = effect.targetAttributeID;

                Feature target = features[targetID];

                if (target.type == typeof(int))
                {
                    int inthelper = (int)features[targetID].GetValue();
                    inthelper = Convert.ToInt32(effect.Activate(this));
                    toApply = inthelper;
                    target.SetValue(inthelper);
                }
                else if (target.type == typeof(float))
                {
                    float? floathelper = (float)features[targetID].GetValue();
                    floathelper = Convert.ToSingle(effect.Activate(this));

                    floathelper = floathelper != -1 ? floathelper : (float)target.GetValue();

                    target.SetValue(floathelper);
                }
                else if (target.type == typeof(bool))
                {
                    bool boolhelper = (bool)features[targetID].GetValue();
                    boolhelper = (bool)effect.Activate(this);
                    toApply = boolhelper;
                    target.SetValue(boolhelper);
                }
                else
                {
                    throw new ArgumentException("Invalid type: " + target.type);
                }
            }
        }
        catch (KeyNotFoundException e)
        {
            Debug.LogWarning("AbstractStatus: tried to apply effect to a non existing features, this might be a normal behaviour if intended");
        }
        catch (InvalidCastException e)
        {
            Debug.LogWarning("AbstractStatus: tried to apply effect to a non existing features, this might be a normal behaviour if intended");
        }
        catch (ArgumentException e)
        {
            Debug.LogWarning("AbstractStatus: tried to apply effect to a non existing features, this might be a normal behaviour if intended");
        }


    }
}