using Character.Enemies.Attacks;
using Character.Player;
using UnityEngine;

namespace Character.Enemies.Bosses
{
    [RequireComponent(typeof(CharacterMovement))]
    public class MonkBrain : MonoBehaviour
    {
        [Header("AI settings")] 
        [SerializeField] private float m_targetDistance = 1.5f;

        [Header("Fighting components")] 
        [SerializeField] private AreaDamage m_simpleAttack;
    
        private Transform m_playerTransform;
        private CharacterMovement m_movement;
        private Animator m_animator;

        private float m_idleTimer = 0.0f;
        public bool Idling => m_idleTimer > 0.0f;
        private void Start()
        {
            m_playerTransform = GameObject.FindWithTag("Player").transform;
            m_movement = GetComponent<CharacterMovement>();
            m_animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            if (Idling)
            {
                m_idleTimer -= Time.deltaTime;
                return;
            } 
            
            float l_rawDist = m_playerTransform.position.x - transform.position.x;
            if (Mathf.Abs(l_rawDist) > m_targetDistance)
            {
                Debug.Log(Mathf.Sign(l_rawDist));
                m_movement.SetXDirection(Mathf.Sign(l_rawDist));
            }
            else
            {
                m_simpleAttack.PrepareAttack(0.75f, () =>
                {
                    m_animator.SetTrigger("attackTrigger");
                    return true;
                });
                m_movement.SetXDirection(0);

                this.StartIdling(1.5f);
            }
        }

        private void StartIdling(float p_duration)
        {
            m_idleTimer = p_duration;
        }
    }
}
