namespace Experimental
{
    public class Modifier
    {
        public Experimental.Feature.FeatureType m_type;  // type of feature to modify
        public float m_fFactor = 1.0f;      // defaults to no change
        public float m_fAddendum = 0.0f;    // defaults to no change

        // Constructor for runtime creation
        public Modifier(Experimental.Feature.FeatureType type, float factor = 1.0f, float addendum = 0.0f)
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
    }
}