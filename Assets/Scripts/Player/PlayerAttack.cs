using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAttack : MonoBehaviour
    {
        private Animator m_animator;

        private bool m_attacking = false;
        private bool m_canAttack = true;
        private float m_attackTimer = 0.0f;
        private int m_currentCombo = 0;
        
        private bool m_desiredAttack = false;
        private float m_attackBufferTimer = 0.0f;

        public float attackBuffer = 0.2f;
        public float chainTime = 0.1f;
        public float attackTime = 0.2f;
        public int maxCombo = 3;

        public List<Collider2D> strikeColliders;
        
        private static readonly int ComboAnimID = Animator.StringToHash("combo_state");
        private static readonly int StrikeAnimID = Animator.StringToHash("strike");

        public bool Attacking => m_attacking;

        private void Awake()
        {
            m_animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            DisableColliders();
        }

        public void OnStrike(InputAction.CallbackContext p_ctx)
        {
            if (p_ctx.started)
            {
                if (m_canAttack)
                {
                    DoAttack();
                }
                else
                {
                    m_desiredAttack = true;
                    m_attackBufferTimer = attackBuffer;
                }
            }
        }

        public void Update()
        {
            if (m_attackTimer > 0) m_attackTimer -= Time.deltaTime;
            else
            {
                DisableColliders();
                m_attacking = false;
            }

            if (attackTime - m_attackTimer > chainTime) m_canAttack = true;

            if (m_attackBufferTimer > 0) m_attackBufferTimer -= Time.deltaTime;

            if (m_desiredAttack && m_attackBufferTimer < 0.0f)
            {
                m_desiredAttack = false;
                DoAttack();
            }
        }

        private void DoAttack()
        {
            m_attacking = true;
            m_canAttack = false;
            m_attackTimer = attackTime;
            
            m_animator.SetInteger(ComboAnimID, m_currentCombo);
            m_animator.SetTrigger(StrikeAnimID);

            DisableColliders();
            strikeColliders[m_currentCombo].gameObject.SetActive(true);

            m_currentCombo = (m_currentCombo + 1) % maxCombo;
        }

        private void DisableColliders()
        {
            foreach (Collider2D l_collider in strikeColliders)
            {
                l_collider.gameObject.SetActive(false);
            }
        }
    }
}
