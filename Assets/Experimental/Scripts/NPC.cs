using System;
using System.Collections.Generic;
using System.Numerics;
using Experimental;
using UnityEngine;
using Utility.Positioning;

public class NPC : MonoBehaviour
{
    List<Component> m_components;
    List<Experimental.Feature> m_features;
    List<Modifier> m_mods;

    public string m_id = "NPC_0";     // Unique identifier for the NPC

    AgentController m_agentController; // Reference to the AgentController

    void Awake()
    {
        gameObject.name = m_id;
        m_agentController = GetComponent<AgentController>();
    }

    void Start()
    {
        // Initialize components, features, and modifiers
        m_components = new List<Component>();
        m_features = new List<Experimental.Feature>();  
        m_mods = new List<Modifier>();
        
        AddComponent(new Patrol(m_agentController));
        AddComponent(new Chase(m_agentController));
        
        // Examples of adding features to the NPC
        // Experimental.Feature speedFeature = new(10.0f, Experimental.Feature.FeatureType.SPEED);
        // AddFeature(speedFeature);

        // Experimental.Feature healthFeature = new(100.0f, Experimental.Feature.FeatureType.HEALTH);
        // AddFeature(healthFeature);

        // // Example of adding a modifier to the NPC
        // Modifier speedModifier = new Modifier(Experimental.Feature.FeatureType.SPEED, 1.5f, 2.0f);
        // AddModifier(speedModifier);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var feature in m_features)
        {
            foreach (var modifier in m_mods)
            {
                if (feature.GetFeatureType() == modifier.GetFeatureType())
                {
                    feature.SetCurrentValue(modifier.Apply(feature.GetBaseValue()));
                }
            }
        }

        foreach (var component in m_components)
        {
            // Update each component
            component.Update();
        }

        // Compute the current speed
        float currSpeed = m_features.Find(f => f.GetFeatureType() == Experimental.Feature.FeatureType.SPEED)?.GetCurrentValue() ?? 0.0f;
        
        foreach (var component in m_components)
        {
            if (component.GetFeature(Experimental.Feature.FeatureType.SPEED) != null)
            {
                currSpeed += component.GetFeature(Experimental.Feature.FeatureType.SPEED).GetCurrentValue();
            }
        }

        // Compute the current position
        float currX = m_features.Find(f => f.GetFeatureType() == Experimental.Feature.FeatureType.X_COORD)?.GetCurrentValue() ?? 0.0f;
        float currY = m_features.Find(f => f.GetFeatureType() == Experimental.Feature.FeatureType.Y_COORD)?.GetCurrentValue() ?? 0.0f;
        float currZ = m_features.Find(f => f.GetFeatureType() == Experimental.Feature.FeatureType.Z_COORD)?.GetCurrentValue() ?? 0.0f;

        foreach (var component in m_components)
        {
            // Update each component's position only is active (speed != 0)
            if (component.GetFeature(Experimental.Feature.FeatureType.SPEED)?.GetCurrentValue() != 0)
            {
                if (component.GetFeature(Experimental.Feature.FeatureType.X_COORD) != null)
                {
                    currX = component.GetFeature(Experimental.Feature.FeatureType.X_COORD).GetCurrentValue();
                }
                if (component.GetFeature(Experimental.Feature.FeatureType.Y_COORD) != null)
                {
                    currY = component.GetFeature(Experimental.Feature.FeatureType.Y_COORD).GetCurrentValue();
                }
                if (component.GetFeature(Experimental.Feature.FeatureType.Z_COORD) != null)
                {
                    currZ = component.GetFeature(Experimental.Feature.FeatureType.Z_COORD).GetCurrentValue();
                }
            }
        }
        
        UnityEngine.Vector3 currDest = new(currX, currY, currZ);

        // Compute the current health
        float currHealth = m_features.Find(f => f.GetFeatureType() == Experimental.Feature.FeatureType.HEALTH)?.GetCurrentValue() ?? 0.0f;

        foreach (var component in m_components)
        {
            if (component.GetFeature(Experimental.Feature.FeatureType.HEALTH) != null)
            {
                currHealth += component.GetFeature(Experimental.Feature.FeatureType.HEALTH).GetCurrentValue();
            }
        }

        // Update the AgentController with the current speed and destination
        m_agentController.SetSpeed(currSpeed);
        m_agentController.MoveTo(currDest);
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
        m_mods.Add(modifier);
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

    void OnDrawGizmos()
    {
        // Draw a sphere at the NPC's position with a radius of 0.5f
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
        
        // Draw the path of the NPC
        foreach (var component in m_components)
        {
            if (component is Patrol patrolComponent)
            {
                RandomPointGeneratorExtensions.DrawGizmos(patrolComponent.patrolPoints);
            }
        }
    }
}
