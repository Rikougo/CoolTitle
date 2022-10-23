using UnityEngine;

namespace Animations
{
    [ExecuteInEditMode]
    public class PulseAnimation : MonoBehaviour
    {
        [SerializeField] private Vector3 m_targetScale = new Vector3(1.2f, 1.2f, 1.2f);
        [SerializeField] private float m_duration = 1.0f;
        [SerializeField] private bool m_loop = false;

        [SerializeField] private bool m_reset = false;
        private Vector3 m_initialScale;
        private float m_currentTimer;

        public float Progress => Mathf.Clamp(m_currentTimer / m_duration, 0.0f, 1.0f);

        private void Awake()
        {
            m_initialScale = transform.localScale;
            m_currentTimer = m_duration;
        }

        private void Update()
        {
            if (m_reset)
            {
                m_reset = false;
                m_currentTimer = m_duration;
            }
            
            if (m_currentTimer > 0.0f)
            {
                m_currentTimer -= Time.deltaTime;

                float l_coef = 1.0f - (Mathf.Cos(Progress * 6.28f) * 0.5f + 0.5f);

                transform.localScale = Vector3.Lerp(m_initialScale, m_targetScale, l_coef);

                if (m_currentTimer < 0.0f && m_loop)
                {
                    m_currentTimer = m_duration;
                }
            }
        }
    }
}