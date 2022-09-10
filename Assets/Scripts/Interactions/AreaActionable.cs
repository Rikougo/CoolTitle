using System;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions
{
    public class AreaActionable : MonoBehaviour
    {
        public UnityEvent<Player> ExecuteAction;

        public UnityAction OnPlayerEnter;
        public UnityAction OnPlayerExit;

        public void Execute(Player p_player)
        {
            ExecuteAction?.Invoke(p_player);
        }

        private void OnTriggerEnter2D(Collider2D p_collider2D)
        {
            if (p_collider2D.gameObject.CompareTag("Player"))
            {
                OnPlayerEnter?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D p_collider2D)
        {
            if (p_collider2D.gameObject.CompareTag("Player"))
            {
                OnPlayerExit?.Invoke();
            }
        }
    }
}
