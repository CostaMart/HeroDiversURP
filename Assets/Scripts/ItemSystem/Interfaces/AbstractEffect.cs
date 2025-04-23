using NCalc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Implement this to participate to the upgrade system and define your own effects
/// </summary>
public abstract class AbstractEffect
{
    public List<IDictionary> containedIn = new List<IDictionary>();
    public int ID;

    protected EffectsDispatcher dispatcher;

    /// <summary>
    /// expression to compute new vals from this effect.
    /// It is already in its compiled form, so it is not necessary to compile it again.
    /// </summary>
    public Expression ex;

    /// <summary>
    /// This is the list of parameters translated into index from the 'expr' string of the json effect (only local,
    /// the ones with @ before)
    /// </summary>
    public int[][] localParametersRef;

    /// <summary>
    /// This is the list of parameters translated into index from the 'expr' string of the json effect (only external,
    /// the ones with ! before)
    /// </summary>
    public int[][] externParametersRef;

    /// <summary>
    /// This will contain references to the objects resolved by dispatcher, this effect can use this references
    /// to resolve values he needs on activation.
    /// </summary>
    public AbstractStatus[] localParametersRefClasses = new AbstractStatus[0];

    /// <summary>
    /// This will contain references to the objects resolved by dispatcher, this effect can use this references
    /// to resolve values he needs on activation.
    /// </summary>
    public AbstractStatus[] externParametersRefClasses = new AbstractStatus[0];

    /// <summary>
    /// parameters key used in the ex for local references
    /// </summary>
    public char[] localParametersKey;

    /// <summary>
    /// parameters key used in the ex for external references
    /// </summary>
    public char[] externParametersKey;

    /// <summary>
    /// This array the value of the parameters in parametersRef, it is updated at each DoEffect call
    /// </summary>
    public object[] resolvedValsLocal;
    public object[] resolvedValsExternal;

    /// <summary>
    /// ID of the class to be affected
    /// </summary>
    public int localTargetClassID = -1;

    public int externalTargetClassID = -1;

    /// <summary>
    /// ID of the attribute to be affected within the class specified by <see cref="localTargetClassID"/>
    /// </summary>
    public int targetAttributeID { get; private set; }


    /// <summary>
    /// This constructor inits the effect expression with data from json file. it transaltes the expression parameters into IDs
    /// This constructer is only called at game start when ItemManager reads the JSON file. The expression is compiled only once and stored in the ex variable.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="itemID"></param>
    public AbstractEffect(Dictionary<string, string> data, int itemID, bool inABullet)
    {
        try
        {
            string targetClassString = data["target"];
            string s = data["expr"];


            // search for parameters to resolve locally
            MatchCollection internalRefs = Regex.Matches(data["expr"], @"@\w+\.\d+");
            MatchCollection external = Regex.Matches(data["expr"], @"!\w+\.\d+");

            if (targetClassString.Contains("@"))
            {
                localTargetClassID = ItemManager.statClassToIdRegistry[targetClassString.Split(".")[0].Replace("@", "")];
            }
            else if (targetClassString.Contains("!"))
            {
                externalTargetClassID = ItemManager.statClassToIdRegistry[targetClassString.Split(".")[0].Replace("!", "")];
            }

            targetAttributeID = (int)Enum.Parse(typeof(FeatureType), targetClassString.Split(".")[1]);


            char c = 'A';
            int len = internalRefs.Count;
            localParametersRef = new int[len][];
            localParametersKey = new char[len];
            Debug.Log("this is the numebr of local parameters " + len + "for item " + itemID);

            len = external.Count;

            externParametersRef = new int[len][];
            externParametersKey = new char[len];

            Debug.Log("this is the numebr of external parameters " + len + "for item " + itemID);
            int i = 0;

            // cerchiamo nella stringa tutti i riferimenti a variabili di altre classi, nel caso se ne trovino vengono sostituite con un ID alfabetico nella stringa dell'espression
            // contemporaneamente il nome viene tradotto in ID numerico e inserito nell'array dei riferimenti da risolvere. I valori vengono risolti 
            // ad ogni invocazione di DoEffect in modo che siano sempre aggiornati
            foreach (var match in internalRefs)
            {
                s = s.Replace(match.ToString(), c.ToString());
                string laClass = match.ToString().Split('.')[0].Substring(1);
                string laAttribute = match.ToString().Split('.')[1];

                localParametersRef[i] = new int[2];
                localParametersRef[i][0] = ItemManager.statClassToIdRegistry[laClass];
                localParametersRef[i][1] = int.Parse(laAttribute);
                localParametersKey[i] = c;

                Debug.Log("it is just been assigned key " + c.ToString() + " for item " + itemID);
                c += (char)1;
                i++;
            }

            i = 0;

            foreach (var match in external)
            {
                s = s.Replace(match.ToString(), c.ToString());
                string laClass = match.ToString().Split('.')[0].Substring(1);
                string laAttribute = match.ToString().Split('.')[1];

                externParametersRef[i] = new int[2];
                externParametersRef[i][0] = ItemManager.statClassToIdRegistry[laClass];
                externParametersRef[i][1] = int.Parse(laAttribute);
                externParametersKey[i] = c;

                c += (char)1;
                i++;
            }

            if (!inABullet)
            {
                if (external.Count > 0)
                {
                    if (!data.ContainsKey("targetType") || data["targetType"] == "local")
                    {
                        Debug.LogError("External parameters found but no target type specified. This effect will not be applied.");
                        return;
                    }
                }
            }

            ex = new Expression(s);
        }
        catch (Exception e)
        {
            if (e.Message.Contains("The given key"))
                Debug.LogError("Error in effect creation: " + e.Message +
                " this effect is trying to access type class which is not registered in the item system manager");
            throw;
        }
    }

    public AbstractEffect() { }

    /// <summary>
    /// This method applies the effect to the target. It resolves the parameters and applies the expression to compute the new value for the target attribute.
    /// </summary>
    /// <param name="target">The target of the item effect.</param>
    /// <param name="dispatcher">
    /// Provides access to other game system elements, such as event dispatchers. 
    /// Use this to implement custom behaviors.
    /// </param>
    protected object DoEffect()
    {
        resolvedValsLocal = resolveValues(localParametersRefClasses, localParametersRef);
        resolvedValsExternal = resolveValues(externParametersRefClasses, externParametersRef);

        var x = 0;
        foreach (var reference in localParametersRefClasses)
        {
            ex.Parameters[localParametersKey[x].ToString()] = resolvedValsLocal[x];
            x++;
        }

        x = 0;
        foreach (var reference in externParametersRefClasses)
        {
            ex.Parameters[externParametersKey[x].ToString()] = resolvedValsExternal[x];
            x++;
        }

        var returnable = Convert.ToSingle(ex.Evaluate());
        return returnable;
    }

    private object[] resolveValues(AbstractStatus[] statusClass, int[][] paramIndexes)
    {
        var x = 0;
        object[] resolved = new object[statusClass.Length];

        foreach (AbstractStatus refClass in statusClass)
        {
            resolved[x] = refClass.GetStatByID<object>(paramIndexes[x][1]);
            x++;
        }

        return resolved;
    }

    /// <summary>
    /// attach this effect to the target status class. 
    /// and to the dispatcher.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="dispatcher"></param>
    public abstract void Attach(Dictionary<int, AbstractStatus> target, EffectsDispatcher dispatcher);


    /// <summary>
    /// define here the activation logic of the effect.
    /// to apply the effect call <see cref="DoEffect"/> method.
    /// <paramref name="dispatcher"/> is used to access other game systems.
    /// <paramref name="target"/> is the target of the effect.
    /// </summary>
    /// TODO: potrei voler levare target come parametro per impedire a chi scrive gli effetti di fare cose strane
    public abstract object Activate(AbstractStatus target);

    protected void DetachEffect()
    {
        foreach (var l in containedIn)
        {
            l.Remove(this.ID);
        }

    }

}
