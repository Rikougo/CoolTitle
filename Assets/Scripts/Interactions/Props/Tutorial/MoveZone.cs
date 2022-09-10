using System;
using UnityEngine;

namespace Interactions.Props.Tutorial
{
    public class MoveZone : MonoBehaviour
    {
        [SerializeField] private DeathButton m_deathButton;

        public void Awake()
        {
            m_deathButton.gameObject.SetActive(false);
        }

        public void OnTriggerExit2D(Collider2D p_collider2D)
        {
            if (p_collider2D.CompareTag("Player"))
            {
                m_deathButton.gameObject.SetActive(true);
            }
        }
    }
}