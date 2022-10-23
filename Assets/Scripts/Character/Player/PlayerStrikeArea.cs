using Character.Enemies;
using UnityEngine;

namespace Character.Player
{
    public class PlayerStrikeArea : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D p_collider)
        {
            if (!p_collider.CompareTag("Enemy")) return;

            EnemyComponent l_enemy = p_collider.GetComponent<EnemyComponent>();
            if (l_enemy == null)
            {
                Debug.Log($"Error: {p_collider.gameObject.GetInstanceID()} game object with Enemy tag has no Enemy component.");
                return;
            }

            l_enemy.TakeDamage();
        }
    }
}
