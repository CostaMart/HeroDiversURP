namespace Experimental
{
    public class Feature
    {
        public enum FeatureType
        {
            SPEED,
            AGILITY,
            INTELLIGENCE,
            ENDURANCE,
            PERCEPTION,
            HEALTH,
            ENERGY,
            ARMOR,
            DAMAGE

        }

        float m_baseValue;
        float m_currValue;
        FeatureType m_type;
        public Feature(float baseValue, FeatureType type)
        {
            m_currValue = m_baseValue = baseValue;
            m_type = type;
        }

        public float GetBaseValue()
        {
            return m_baseValue;
        }
        public float GetCurrentValue()
        {
            return m_currValue;
        } 
        public FeatureType GetFeatureType()
        {
            return m_type;
        }
        public void SetCurrentValue(float value)
        {
            m_currValue = value;
        }
    }
}