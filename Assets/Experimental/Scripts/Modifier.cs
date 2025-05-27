namespace Experimental
{
    public class Modifier
    {
        string m_tag = "Default";    // tag of the entity
        Feature.FeatureType m_type;  // type of feature to modify
        float m_fFactor = 1.0f;      // defaults to no change
        float m_fAddendum = 0.0f;    // defaults to no change




        // Constructor for runtime creation
        public Modifier(Feature.FeatureType type, float factor = 1.0f, float addendum = 0.0f)
        {
            m_type = type;
            m_fFactor = factor;
            m_fAddendum = addendum;
        }

        // Apply the modifier to a value
        public float Apply(float baseValue)
        {
            return baseValue * m_fFactor + m_fAddendum;
        }

        // Load from file
        public static Modifier LoadFromFile(string path)
        {
            // Implementation for loading from file
            throw new System.NotImplementedException();
        }

        public string GetTag()
        {
            return m_tag;
        }

        public Feature.FeatureType GetFeatureType()
        {
            return m_type;
        }
    }
}