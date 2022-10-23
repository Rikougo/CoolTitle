using Inspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    [RequireComponent(typeof(CharacterMoveLimit))]
    public class CharacterRoll : MonoBehaviour
    {
        private CharacterGround m_characterGround;
        private CharacterMoveLimit m_moveLimit;
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
            m_characterGround = GetComponent<CharacterGround>();
            m_moveLimit = GetComponent<CharacterMoveLimit>();
            m_animator = GetComponent<Animator>();

            m_initialLayer = gameObject.layer;
            m_rolling = false;
        }
        
        public void OnRoll(InputAction.CallbackContext p_ctx)
        {
            if (p_ctx.started && m_characterGround.OnGround && m_moveLimit.CanDo(CharacterMoveLimit.Actions.Roll))
            {
                m_moveLimit.LockActions(CharacterMoveLimit.Actions.All);

                gameObject.layer = m_noColliderLayer;
                
                m_animator.SetTrigger(RollAnimID);
                m_rolling = true;
            }
        }

        /// <summary>
        /// Called from Animation event
        /// </summary>
        public void AnimationRollEnd()
        {
            m_rolling = false;
            m_moveLimit.UnlockActions(CharacterMoveLimit.Actions.All);
            gameObject.layer = m_initialLayer;
        }
    }
}