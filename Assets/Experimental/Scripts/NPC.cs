using System;
using System.Collections.Generic;
using Experimental;
using UnityEngine;

public class NPC : MonoBehaviour
{
    List<Component> m_components;
    List<Experimental.Feature> m_features;
    List<Modifier> m_modifiers;

    public string m_id = "NPC_0";     // Unique identifier for the NPC

    void Awake()
    {
        gameObject.name = m_id;
    }

    void Start()
    {
        // Initialize components, features, and modifiers
        m_components = new List<Component>();
        m_features = new List<Experimental.Feature>();  
        m_modifiers = new List<Modifier>();

        if (!TryGetComponent<AgentController>(out var agentController))
        {
            agentController = gameObject.AddComponent<AgentController>();
        }
        
        AddComponent(new Chase(agentController));
        AddComponent(new Patrol(agentController));
        
        // Examples of adding features to the NPC
        Experimental.Feature speedFeature = new(10.0f, Experimental.Feature.FeatureType.SPEED);
        AddFeature(speedFeature);

        Experimental.Feature healthFeature = new(100.0f, Experimental.Feature.FeatureType.HEALTH);
        AddFeature(healthFeature);

        // Example of adding a modifier to the NPC
        Modifier speedModifier = new Modifier(Experimental.Feature.FeatureType.SPEED, 1.5f, 2.0f);
        AddModifier(speedModifier);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var component in m_components)
        {
            // Update each component
            component.Update();
        }
    }

    public void AddFeature(Experimental.Feature feature)
    {
        if (feature == null)
        {
            throw new ArgumentNullException(nameof(feature), "Feature cannot be null.");
        }
        
        // Add feature to the NPC
        m_features.Add(feature);
    } 

    public void AddModifier(Modifier modifier)
    {
        if (modifier == null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier cannot be null.");
        }
        
        // Add modifier to the NPC
        m_modifiers.Add(modifier);
    }

    public void AddComponent(Component component)
    {
        if (component == null)
        {
            throw new ArgumentNullException(nameof(component), "Component cannot be null.");
        }
        
        // Add component to the NPC
        m_components.Add(component);
    }
}
