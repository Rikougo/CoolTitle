using System;
using UnityEngine;

namespace GameScenario.Utils
{
    public class TimerHolder
    {
        public class TimerAlreadyEndedException : Exception
        {
        }

        public delegate void OnEndHandler(TimerHolder p_timer);
        public delegate void OnUpdateHandler(TimerHolder p_timer, float p_deltaTime);

        public event OnUpdateHandler OnUpdate;
        public event OnEndHandler OnEnd;

        public float Duration
        {
            get => m_duration;
            set
            {
                if (m_started) return;

                m_duration = value;
            }
        }

        private float m_duration;
        private float m_currentStatus;
        private bool m_started = false;
        private bool m_ended = false;

        public float Progress => m_currentStatus / Duration;
        public bool Ended => m_ended;
        public bool Started => m_started;

        public void Start()
        {
            m_currentStatus = 0.0f;
            m_started = true;
        }

        /// <summary>
        /// Update timer Progress and call OnUpdate or OnEnd event based on the progression.
        /// </summary>
        /// <param name="p_deltaTime"></param>
        /// <returns>If timer has ended or not</returns>
        public bool UpdateTimer(float p_deltaTime)
        {
            if (m_ended)
                return false;

            m_currentStatus += p_deltaTime;

            if (m_currentStatus > m_duration) End();
            else OnUpdate?.Invoke(this, p_deltaTime);

            return m_currentStatus < m_duration;
        }

        public void End()
        {
            m_currentStatus = m_duration;
            m_ended = true;
            this.OnEnd?.Invoke(this);
        }
    }
}