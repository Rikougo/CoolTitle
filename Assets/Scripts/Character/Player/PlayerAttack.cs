using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        private Animator m_animator;
        private CharacterMoveLimit m_moveLimit;

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
        private static readonly int StrikeAnimID = Animator.StringToHash("strikeTrigger");

        public bool Attacking => m_attacking;

        private void Awake()
        {
            m_animator = GetComponentInChildren<Animator>();
            m_moveLimit = GetComponent<CharacterMoveLimit>();
        }

        private void Start()
        {
            DisableColliders();
        }

        public void OnStrike(InputAction.CallbackContext p_ctx)
        {
            if (p_ctx.started)
            {
                if (m_canAttack && m_moveLimit.CanDo(CharacterMoveLimit.Actions.Attack))
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
                if (m_attacking)
                {
                    DisableColliders();
                    m_attacking = false;
                    m_moveLimit.UnlockActions(CharacterMoveLimit.Actions.All);
                }
            }

            if (attackTime - m_attackTimer > chainTime) m_canAttack = true;

            if (m_attackBufferTimer > 0) m_attackBufferTimer -= Time.deltaTime;
            if (m_desiredAttack && m_attackBufferTimer <= 0.0f)
            {
                m_desiredAttack = false;
                m_attackBufferTimer = 0.0f;
            }

            if (m_moveLimit.CanDo(CharacterMoveLimit.Actions.Attack) && m_desiredAttack)
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

            m_moveLimit.LockActions(CharacterMoveLimit.Actions.All ^ CharacterMoveLimit.Actions.Attack);
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