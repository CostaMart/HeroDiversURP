using System.Collections.Generic;

public abstract class ObjectWithFeatures
{
    protected List<Experimental.Feature> features = new();
    
    public virtual void AddFeature(Experimental.Feature feature)
    {
        features.Add(feature);
    }
    
    public virtual Experimental.Feature[] GetFeatures()
    {
        return features.ToArray();
    }
}
