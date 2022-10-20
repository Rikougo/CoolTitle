using System;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions
{
    public class AreaActionable : MonoBehaviour
    {
        public UnityEvent<CharacterMovement> ExecuteAction;

        public UnityAction OnPlayerEnter;
        public UnityAction OnPlayerExit;

        public void Execute(CharacterMovement p_characterMovement)
        {
            ExecuteAction?.Invoke(p_characterMovement);
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
