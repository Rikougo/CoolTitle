using System;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;

namespace Character.Enemies.Attacks
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AreaDamage : MonoBehaviour
    {
        private SpriteRenderer m_sprite;
        private Collider2D m_collider2D;

        private float m_attackDelay;
        private Func<bool> m_currentEndFunc;

        private void Awake()
        {
            m_sprite = GetComponent<SpriteRenderer>();
            m_collider2D = GetComponent<Collider2D>();

            m_sprite.enabled = false;
            m_attackDelay = 0.0f;
        }

        public void PrepareAttack(float p_delay, Func<bool> p_endFunction)
        {
            m_sprite.enabled = true;
            m_attackDelay = p_delay;
            m_currentEndFunc = p_endFunction;
        }

        private void FixedUpdate()
        {
            if (m_attackDelay > 0.0f)
            {
                m_attackDelay -= Time.deltaTime;

                if (m_attackDelay <= 0.0f)
                {
                    FireAttack();
                }
            }
        }

        private void FireAttack()
        {
            m_currentEndFunc?.Invoke();
            List<Collider2D> l_results = new List<Collider2D>();
            m_collider2D.OverlapCollider(new ContactFilter2D().NoFilter(), l_results);
            
            foreach (Collider2D l_collider2D in l_results)
            {
                if (l_collider2D.CompareTag("Player"))
                {
                    l_collider2D.GetComponent<PlayerBrain>().TakeDamage(50.0f, this.gameObject);
                }
            }

            m_sprite.enabled = false;
        }
    }
}