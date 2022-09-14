using System;
using GameScenario.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScenario.Dialog
{
    [RequireComponent(typeof(GameDirector))]
    public class DialogDirector : MonoBehaviour
    {
        [SerializeField] private Image m_dialogBox;
        [SerializeField] private TextMeshProUGUI m_dialogText;

        [SerializeField] private float m_letterPerSecond = 5.0f;
        private float m_secondPerLetter;

        private GameDirector m_gameDirector;

        private void Awake()
        {
            m_gameDirector = GetComponent<GameDirector>();
            m_secondPerLetter = 1.0f / m_letterPerSecond;
        }

        private void Start()
        {
            m_dialogBox.gameObject.SetActive(false);
        }

        public void SubmitDialog(string p_content)
        {
            m_dialogText.text = String.Empty;
            m_dialogBox.gameObject.SetActive(true);

            Utils.TimerHolder l_timer = new TimerHolder() { Duration = p_content.Length * m_secondPerLetter };

            l_timer.OnUpdate += (Utils.TimerHolder p_timer, float p_deltaTime) =>
            {
                string l_contentSlice = p_content.Substring(0, (int)(p_timer.Progress * p_content.Length));
                m_dialogText.text = l_contentSlice;
            };

            l_timer.OnEnd += (_) =>
            {
                m_dialogText.text = p_content;
            };

            m_gameDirector.AddTimer(l_timer);
        }
    }
}