using System;
using System.Collections.Generic;

public class Component : ObjectWithFeatures
{
    List<Experimental.Modifier> m_mods;
    
    // Constructor
    public Component()
    {
        m_mods = new List<Experimental.Modifier>();
    }

    public Component(string tag) : this()
    {
        m_tag = tag;
    }

    public virtual void Update()
    {
        // Update logic for the component
        foreach (var feature in features)
        {
            foreach (var modifier in m_mods)
            {
                if (feature.GetFeatureType() == modifier.GetFeatureType())
                {
                    feature.SetCurrentValue(modifier.Apply(feature.GetBaseValue()));
                }
            }
        }
    }
    
    // Methods
    public virtual void AddModifier(Experimental.Modifier modifier)
    {
        if (modifier == null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier cannot be null.");
        }
        m_mods.Add(modifier);
    }
    
    public virtual Experimental.Modifier[] GetModifiers()
    {
        return m_mods.ToArray();
    }
}