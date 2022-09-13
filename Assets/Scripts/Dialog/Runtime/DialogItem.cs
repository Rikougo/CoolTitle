using System;
using UnityEngine;

namespace Dialog.Runtime
{
    [CreateAssetMenu(fileName = "DialogItem", menuName = "Game/DialogItem", order = 1)]
    public class DialogItem : ScriptableObject
    {
        public string author;
        public string[] content;

        public string[] choices;
        public DialogItem[] result;

        public bool IsConsumed => m_currentIndex >= content.Length;
        private int m_currentIndex;

        public void Init()
        {
            m_currentIndex = 0;
        }

        public bool PeekContent(out string l_result)
        {
            if (IsConsumed)
            {
                l_result = String.Empty;
                return false;
            }
            l_result = content[m_currentIndex];
            m_currentIndex++;
            return true;
        }
    }
}