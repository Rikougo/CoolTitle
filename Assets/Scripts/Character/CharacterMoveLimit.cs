using System;
using UnityEngine;

namespace Player
{
    public class CharacterMoveLimit : MonoBehaviour
    {
        [Flags]
        public enum Actions
        {
            None     = 0,
            Move     = 1 << 1,
            Jump     = 1 << 2,
            Roll     = 1 << 3, 
            Attack   = 1 << 4,
            Interact = 1 << 5,
            All      = (1 << 6) - 1
        }

        public Actions m_availableActions;

        private void Awake()
        {
            m_availableActions = Actions.All;
        }

        public void LockActions(Actions p_actions)
        {
            m_availableActions ^= (p_actions & m_availableActions);
        }

        public void UnlockActions(Actions p_actions)
        {
            m_availableActions |= p_actions;
        }

        public bool CanDo(Actions p_action)
        {
            return (p_action & m_availableActions) == p_action;
        }
    }
}