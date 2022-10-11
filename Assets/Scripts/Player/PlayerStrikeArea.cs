using System;
using Enemies;
using UnityEngine;

namespace Player
{
    public class PlayerStrikeArea : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D p_collider)
        {
            if (!p_collider.CompareTag("Enemy")) return;

            Enemy l_enemy = p_collider.GetComponent<Enemy>();
            if (l_enemy == null)
            {
                Debug.Log($"Error: {p_collider.gameObject.GetInstanceID()} game object with Enemy tag has no Enemy component.");
                return;
            }

            l_enemy.TakeDamage();
        }
    }
}
