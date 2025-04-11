using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Weapon.State;
using static ItemManager;

/// <summary>
/// This component is responsible for dispatching the effects to the correct classes, and serves as a
/// bridge between the upgrades and all the gameobject components which could be useful to implement effects
/// 
/// this class shall manage overTime effects activation too
/// </summary>
public abstract class EffectsDispatcher : MonoBehaviour
{

    [SerializeField] protected Dictionary<int, AbstractStatus> affectables = new Dictionary<int, AbstractStatus>();
    [SerializeField] private ControlEventManager controlEventManager;
    [SerializeField] private BulletPoolStats bulletPoolPrimary;
    [SerializeField] private BulletPoolStats bulletPoolPoolSecondary;

    /// <summary>
    /// mantiene uno storico degli id degli item attivati durante la partita
    /// </summary>
    private List<int> activatedItems = new List<int>();

    public List<PermanentAreaEffect> toExternalDispatchArea = new List<PermanentAreaEffect>();




    void Start()
    {
        new ItemManager();

        // register the dispatcher for dipsatching effects
        if (bulletPoolPrimary != null)
            this.affectables.Add(bulletPoolPrimary.ID, bulletPoolPrimary);

        if (bulletPoolPoolSecondary != null)
            this.affectables.Add(bulletPoolPoolSecondary.ID, bulletPoolPoolSecondary);

        FindComponentsInChildren<AbstractStatus>(transform);
    }



    /// <summary>
    /// This method is called when an item is picked up by the player
    /// <paramref name="it"/> the item picked up
    /// </summary>
    public void ItemDispatch(Item it)
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

    public virtual void DispatchFromOtherDispatcher(Item it)
    {
        foreach (AbstractEffect up in it.effects)
        {
            Debug.Log("Dispatching external effect " + up.ToString());
            up.externParametersRefClasses = resolveReferences(up.externParametersRef);
            up.Attach(affectables, this);
        }
    }

    public virtual void DispatchFromOtherDispatcher(AbstractEffect up)
    {
        up.externParametersRefClasses = resolveReferences(up.externParametersRef);
        up.Attach(affectables, this);
    }

    /// <summary>
    /// If a member of effect class has a reference to an attribute in a status class, this method is called to resolve the current value of such reference
    /// <paramref name="calssID"/> the ID of the class to reference
    /// <paramref name="attributeID"/> the ID of the attribute to reference
    /// </summary>
    public AbstractStatus[] resolveReferences(int[][] references)
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
    /// search for all the components of type T in the hierarchy of the parent transform
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    private void FindComponentsInChildren<T>(Transform parent) where T : AbstractStatus
    {
        var components = parent.GetComponents<Component>();

        foreach (var component in components)
        {
            if (component is T upgradable)
            {
                try
                {
                    Debug.Log("Found component added in affectables of type " + upgradable.GetType().Name + " in object: " + transform.gameObject.name + "in object" + parent.name + "with ID: " + upgradable.ID);
                    affectables.Add(upgradable.ID, upgradable);
                }
                catch (ArgumentException e)
                {
                    if (upgradable.GetType().Name == "WeaponState")
                    {
                        Debug.LogError("This character has two Weapon of the same type  (primary or secondary), please check gameobject: " + transform.gameObject.name + "in object" + parent.name);
                    }
                    else
                    {
                        Debug.LogError(e.Message + " in gameobject: " + transform.gameObject.name + "in object"
                        + parent.name);
                    }
                }
            }
        }

        foreach (Transform child in parent)
        {
            FindComponentsInChildren<T>(child);  // Chiamata ricorsiva per ogni figlio
        }
    }

    /// <summary>
    /// returns a collection of each feature contribution by any stat type class in the game object hierarchy starting
    /// from the position of the dispatcher
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="f"></param>
    /// <returns></returns>
    public T[] GetAllFeatureByType<T>(FeatureType f)
    {
        return affectables.Values
            .SelectMany(status => status.GetFeatureValuesByType<T>(f))
            .ToArray();
    }
}

