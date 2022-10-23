using UnityEngine;

using Character.Asset;
using UnityEngine.Events;

namespace Character.Player
{
    public class PlayerBrain : MonoBehaviour
    {
        [SerializeField] private CharacterStats m_stats;
        private Animator m_animator;
        private CharacterMoveLimit m_moveLimit;

        public UnityEvent<float, float> OnHealthUpdate;

        private void Awake()
        {
            m_stats ??= CharacterStats.GetDefault();
            m_animator = GetComponent<Animator>();
            m_moveLimit = GetComponent<CharacterMoveLimit>();

            m_stats.Init();
        }

        public void TakeDamage(float p_amount, GameObject p_origin)
        {
            m_stats.CurrentHealth -= p_amount - (p_amount * m_stats.Armor);

            OnHealthUpdate.Invoke(m_stats.CurrentHealth, m_stats.CurrentHealth / m_stats.MaxHealth);
            if (m_stats.CurrentHealth == 0.0f)
            {
                m_animator.SetTrigger("deathTrigger");
                m_moveLimit.LockActions(CharacterMoveLimit.Actions.All);
                GetComponent<Rigidbody2D>().isKinematic = true;
                GetComponent<Collider2D>().enabled = false;
                return;
            }
        }
    }
}
