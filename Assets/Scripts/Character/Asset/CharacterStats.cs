using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Character.Asset
{
    [Serializable, CreateAssetMenu(fileName = "CharacterStats", menuName = "CoolTitle/CharacterStats")]
    public class CharacterStats : ScriptableObject
    {
        [SerializeField] private float m_maxHealth = 100.0f;
        [SerializeField] private float m_maxStamina = 100.0f;
        [SerializeField] private float m_maxMana = 100.0f;
        [SerializeField] private List<CharacterStatsModifier> m_modifiers;
        [SerializeField] private float m_armor = 0.0f;

        public float MaxHealth
        {
            get
            {
                return m_maxHealth +
                    (m_maxHealth * m_modifiers.Sum(p_value => p_value.PercentHealthUpgrade)) +
                    m_modifiers.Sum(p_value => p_value.FlatHealthUpgrade);
            }
        }
        public float MaxStamina => m_maxStamina;
        public float MaxMana => m_maxMana;
        public float Armor => Mathf.Clamp(m_armor, -1.0f, 0.99f);

        private float m_currentHealth, m_currentStamina, m_currentMana;
        
        public float CurrentHealth
        {
            get => m_currentHealth;
            set => m_currentHealth = Mathf.Clamp(value, 0.0f, m_maxHealth);
        }
        
        public float CurrentStamina
        {
            get => m_currentStamina;
            set => m_currentStamina = Mathf.Clamp(value, 0.0f, m_maxStamina);
        }
        
        public float CurrentMana
        {
            get => m_currentMana;
            set => m_currentMana = Mathf.Clamp(value, 0.0f, m_maxMana);
        }

        public void Init()
        {
            CurrentHealth = m_maxHealth;
            CurrentStamina = m_maxStamina;
            CurrentMana = m_maxMana;
        }

        private void InitMax(float p_maxHealth, float p_maxStamina, float p_maxMana)
        {
            m_maxHealth = p_maxHealth;
            m_maxStamina = p_maxStamina;
            m_maxMana = p_maxMana;
        }
        
        public static CharacterStats GetDefault()
        {
            CharacterStats l_result = ScriptableObject.CreateInstance<CharacterStats>();
            l_result.InitMax(100.0f, 100.0f, 100.0f);

            return l_result;
        }
    }
}