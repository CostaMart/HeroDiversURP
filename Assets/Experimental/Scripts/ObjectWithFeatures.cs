using System.Collections.Generic;

public abstract class ObjectWithFeatures
{
    protected string m_tag = "Default";
    protected List<Experimental.Feature> features = new();
    
    public virtual void AddFeature(Experimental.Feature feature)
    {
        features.Add(feature);
    }
    
    public virtual Experimental.Feature[] GetFeatures()
    {
        return features.ToArray();
    }

    public virtual Experimental.Feature GetFeature(Experimental.Feature.FeatureType type)
    {
        foreach (var feature in features)
        {
            if (feature.GetFeatureType() == type)
            {
                return feature;
            }
        }
        return null;
    }
}
