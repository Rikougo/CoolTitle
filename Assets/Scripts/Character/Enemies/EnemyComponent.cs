using UnityEngine;
using UnityEngine.Events;

namespace Character.Enemies
{
    public class EnemyComponent : MonoBehaviour
    {
        private SpriteRenderer m_spriteRenderer;
        
        public float damageBlink = 0.2f;
        private float m_damageBlinkTimer = 0.0f;

        public float maxHealth = 100.0f;
        private float m_currentHealth = 0.0f;

        public UnityEvent<float> OnHealthChange;

        private void Awake()
        {
            m_spriteRenderer = GetComponent<SpriteRenderer>();

            m_currentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChange?.Invoke(m_currentHealth / maxHealth);
        }
        
        public void TakeDamage()
        {
            m_damageBlinkTimer = damageBlink;

            m_currentHealth = Mathf.Max(0.0f, m_currentHealth - 10.0f);
            OnHealthChange?.Invoke(m_currentHealth / maxHealth);
        }

        private void Update()
        {
            if (m_damageBlinkTimer > 0.0f)
            {
                m_damageBlinkTimer -= Time.deltaTime;

                float l_coef = (1.0f - (m_damageBlinkTimer / damageBlink)) * 2 * Mathf.PI;
                float l_cosValue = (Mathf.Cos(l_coef) + 1.0f) * 0.5f;
                m_spriteRenderer.color = m_damageBlinkTimer < 0.0f ? 
                    Color.white : 
                    new Color(1.0f, l_cosValue, l_cosValue);
            }
        }
    }
}