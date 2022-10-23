using UnityEngine;

namespace Character.Asset
{
    public class CharacterStatsModifier : ScriptableObject
    {
        [SerializeField] private string m_name;
        [SerializeField] private float m_percentHealthUpgrade;
        [SerializeField] private float m_flatHealthUpgrade;

        public string Name => m_name;
        public float PercentHealthUpgrade => m_percentHealthUpgrade;
        public float FlatHealthUpgrade => m_flatHealthUpgrade;
    }
}