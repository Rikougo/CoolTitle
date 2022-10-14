using System;
using Inspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerRoll : MonoBehaviour
    {
        private PlayerGround m_playerGround;
        private PlayerMoveLimit m_moveLimit;
        private Animator m_animator;

        [Header("Roll configuration")] 
        [SerializeField] private float m_rollSpeed = 1.5f;

        [SerializeField, Layer] private int m_noColliderLayer;
        private int m_initialLayer;

        private bool m_rolling;
        private static readonly int RollAnimID = Animator.StringToHash("rollTrigger");

        public bool Rolling => m_rolling;
        public float Speed => m_rollSpeed;

        private void Awake()
        {
            m_playerGround = GetComponent<PlayerGround>();
            m_moveLimit = GetComponent<PlayerMoveLimit>();
            m_animator = GetComponent<Animator>();

            m_initialLayer = gameObject.layer;
            m_rolling = false;
        }
        
        public void OnRoll(InputAction.CallbackContext p_ctx)
        {
            if (p_ctx.started && m_playerGround.OnGround && m_moveLimit.CanDo(PlayerMoveLimit.Actions.Roll))
            {
                m_rolling = true;
                m_moveLimit.LockActions(PlayerMoveLimit.Actions.All);

                gameObject.layer = m_noColliderLayer;
                
                m_animator.SetTrigger(RollAnimID);
            }
        }

        public void AnimationRollEnd()
        {
            m_rolling = false;
            m_moveLimit.UnlockActions(PlayerMoveLimit.Actions.All);
            gameObject.layer = m_initialLayer;
        }
    }
}