using UnityEngine;

namespace GameScenario.Dialog
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

        public string PeekContent()
        {
            string l_result = content[m_currentIndex];
            m_currentIndex++;
            return l_result;
        }
    }
}