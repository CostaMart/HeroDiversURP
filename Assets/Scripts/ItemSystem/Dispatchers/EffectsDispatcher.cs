using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using Weapon.State;
using static ItemManager;

[DefaultExecutionOrder(-100)]
/// <summary>
/// This component is responsible for dispatching the effects to the correct classes, and serves as a
/// bridge between the upgrades and all the gameobject components which could be useful to implement effects
/// this class shall manage overTime effects activation too
/// </summary>
public abstract class EffectsDispatcher : MonoBehaviour
{

    /// <summary>
    /// list of all components (push to data components, e.g. statusclasses, not Unity's) in this gameobject (and its hierarchy)
    /// </summary>
    [SerializeField] protected Dictionary<int, AbstractStatus> affectables = new Dictionary<int, AbstractStatus>();

    /// <summary>
    /// disabled stat calsses are not used to compute stat values, they still recive modifiers
    /// </summary>
    Dictionary<int, bool> enabledStatClass = new Dictionary<int, bool>();
    [SerializeField] private BulletPoolStats bulletPoolPrimary;
    [SerializeField] private BulletPoolStats bulletPoolPoolSecondary;

    /// <summary>
    /// mantiene uno storico degli id degli item attivati durante la partita
    /// </summary>
    private List<int> activatedItems = new List<int>();

    void Start()
    {
        new ItemManager();

        // register the dispatcher for dipsatching effects
        if (bulletPoolPrimary != null)
            this.affectables.Add(bulletPoolPrimary.ID, bulletPoolPrimary);

        if (bulletPoolPoolSecondary != null)
            this.affectables.Add(bulletPoolPoolSecondary.ID, bulletPoolPoolSecondary);

        //   FindComponentsInChildren<AbstractStatus>(transform);
    }



    /// <summary>
    /// This method is called when an item is picked up by the player
    /// <paramref name="it"/> the item picked up
    /// </summary>
    public void modifierDispatch(Modifier it)
    {
        if (it.bullet)
        {
            foreach (var effect in it.effects)
            {
                effect.localParametersRefClasses = resolveReferences(effect.localParametersRef);
            }

            return;
        }

        activatedItems.Add(it.id);

        foreach (AbstractEffect up in it.effects)
        {
            up.localParametersRefClasses = resolveReferences(up.localParametersRef);
            up.Attach(affectables, this);
        }
    }

    /// <summary>
    /// send a modifier to this dispatcher resolving local parameters with yourDispatcher values 
    // and external parameters (on which this method is called) with this dispatcher values
    /// </summary>
    /// <param name="it"></param>
    public virtual void AttachModifierFromOtherDispatcher(EffectsDispatcher yourDispatcher, Modifier it)
    {
        foreach (AbstractEffect up in it.effects)
        {
            up.localParametersRefClasses = yourDispatcher.resolveReferences(up.localParametersRef);
        }

        foreach (AbstractEffect up in it.effects)
        {
            Debug.Log("Dispatching external effect " + up.ToString());
            up.externParametersRefClasses = resolveReferences(up.externParametersRef);
            up.Attach(affectables, this);
        }
    }


    /// <summary>
    /// If a member of effect class has a reference to an attribute in a status class, this method is called to 
    // resolve the current value of such reference
    /// <paramref name="calssID"/> the ID of the class to reference
    /// <paramref name="attributeID"/> the ID of the attribute to reference
    /// </summary>
    private AbstractStatus[] resolveReferences(int[][] references)
    {
        AbstractStatus[] toret = new AbstractStatus[references.Length];

        int x = 0;

        foreach (var refere in references)
        {
            try
            {
                toret[x] = affectables[refere[0]];
                x++;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogError("status with ID " + refere[0] + " not found in the dispatcher of object " +
                 transform.name + " with ID: " + refere[1] + " this might be normal");

                x++;
            }
            catch (Exception e)
            {
                Debug.LogError("Error in resolving value: " + e.Message);
            }

        }

        return toret;
    }

    /// <summary>
    /// used by status classes (alias for push to data components) to register themselves to the dispatcher
    /// </summary>
    /// <param name="statusClass"></param>
    /// <param name="isEnabled"></param>
    public void AttachStatusClass(AbstractStatus statusClass, bool isEnabled)
    {
        if (affectables.ContainsKey(statusClass.ID))
        {
            if (statusClass != affectables[statusClass.ID])
            {
                Debug.LogError("status with ID " + statusClass.ID + " already present in the dispatcher of object " +
                 transform.name);
                return;
            }
            else
            {
                return;
            }
        }

        affectables.Add(statusClass.ID, statusClass);
        enabledStatClass.Add(statusClass.ID, isEnabled);
    }

    /// <summary>
    /// used by status classes (alias for push to data components) to unregister themselves from the dispatcher
    /// </summary>
    /// <param name="status"></param>
    public void DetachStatusClass(AbstractStatus status)
    {
        if (affectables.ContainsKey(status.ID))
        {
            affectables.Remove(status.ID);
            enabledStatClass.Remove(status.ID);
        }
        else
        {
            Debug.LogError("status with ID " + status.ID + " not present in the dispatcher of object " +
             transform.name);
        }
    }

    /// <summary>
    /// used by status classes (alias for push to data components) to set their status as enabled or disabled
    /// to see what disabled means, see the description of the enabledStatClass dictionary
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="toSet"></param>
    public void SetActiveStatusClass(int ID, bool toSet)
    {
        enabledStatClass[ID] = toSet;
    }

    /// <summary>
    /// returns the list of all features values from all the components of this gameobject, 
    // can be used to compute final value of a feature
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="f"></param>
    /// <returns></returns>
    public T[] GetAllFeatureByType<T>(FeatureType f)
    {
        return affectables.Values
            .Where(status => enabledStatClass.ContainsKey(status.ID) && enabledStatClass[status.ID])
            .SelectMany(status => status.GetFeatureValuesByType<T>(f))
            .ToArray();
    }

    /// <summary>
    /// of all the stats classes into this object that have the feature f, returns the most recent value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="f"></param>
    /// <returns></returns>
    public T GetMostRecentFeatureValue<T>(FeatureType f)
    {
        try
        {
            var toRet = affectables.Values
                .Where(status => enabledStatClass.ContainsKey(status.ID) && enabledStatClass[status.ID])
                .SelectMany(stats => stats.features.Values.Where(feat => feat.id == f))
                .ToList();
            toRet.Sort((x, y) => x.lastModifiedTime >= y.lastModifiedTime ? -1 : 1);
            return (T)toRet.FirstOrDefault().currentValue;
        }
        catch (NullReferenceException e)
        {
            Debug.LogError("a null reference exception was thrown in the EffectsDispatcher" +
             " this might be caused by a miss configuration of the feature file, the following is the Exception thrown: "
             + e.Message);

            return default;
        }
    }

    public void DetachStatClass(int ID)
    {
        affectables.Remove(ID);
        enabledStatClass.Remove(ID);
    }
}

