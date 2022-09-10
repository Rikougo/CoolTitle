using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using GameScenario.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GameScenario
{
    public class GameDirector : MonoBehaviour
    {
        public enum GameState
        {
            PLAYING,
            DIALOG,
            PAUSE
        }
        
        private Player m_player;
        private Camera m_camera;
        private Volume m_globalVolume;
        
        private List<TimerHolder> m_timers;
        
        public VolumeProfile defaultVolume;
        public VolumeProfile deathVolume;
        public CinemachineVirtualCamera defaultCameraBrain;

        private GameState m_state;

        public GameState State
        {
            get => m_state;
            set
            {
                GameState l_oldState = m_state;
                m_state = value;
                OnGameStateChange?.Invoke(m_state, l_oldState);
            }
        }

        public void Awake()
        {
            m_timers = new List<TimerHolder>();
        }
        
        public void Start()
        {
            m_player = GameObject.FindWithTag("Player").GetComponent<Player>();
            m_camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            m_globalVolume = GameObject.FindWithTag("GlobalVolume").GetComponent<Volume>();

            if (m_player is null)
            {
                Debug.LogError("Couldn't find Player component in Scene.");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
            
            m_player.GetComponent<PlayerInput>().actions["Restart"].performed += (ctx) => ResurrectPlayer();
        }
        
        private void Update()
        {
            TickTimers();
        }

        private void TickTimers()
        {
            List<TimerHolder> l_toDelete = new List<TimerHolder>();
            for (int l_index = 0; l_index < m_timers.Count; l_index++)
            {
                TimerHolder l_timer = m_timers[l_index];
                if (!l_timer.UpdateTimer(Time.deltaTime))
                {
                    l_toDelete.Add(l_timer);
                }
            }

            for (int l_index = 0; l_index < l_toDelete.Count; l_index++)
            {
                m_timers.Remove(l_toDelete[l_index]);
            }
        }

        public void KillPlayer()
        {
            m_player.Die();
            m_globalVolume.profile = deathVolume;
            
            const float targetAmplitudeGain = 5.0f;
            defaultCameraBrain.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = targetAmplitudeGain;

            TimerHolder l_shakeTimer = new TimerHolder(){ Duration = 0.7f };
            l_shakeTimer.OnUpdate += (TimerHolder p_timer) =>
            {
                float l_invertProgress = 1.0f - p_timer.Progress;
                float l_coef = Math.Abs(l_invertProgress - 1.0f) < 0.001f ? 1.0f : 1 - Mathf.Pow(2.0f, -10.0f * l_invertProgress);
                defaultCameraBrain.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = targetAmplitudeGain * l_coef;
            };
            l_shakeTimer.OnEnd += (TimerHolder p_timer) =>
            {
                defaultCameraBrain.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.0f;
                OnPlayerDeath?.Invoke();
            };

            this.AddTimer(l_shakeTimer);
        }

        public void ResurrectPlayer()
        {
            m_globalVolume.profile = defaultVolume;
            m_player.Resurrect();
        }
        
        public void AddTimer(TimerHolder p_timer)
        {
            if (p_timer.Started || p_timer.Ended)
            {
                Debug.LogWarning("Registering an already started Timer.");
            }
            
            p_timer.Start();
            m_timers.Add(p_timer);
        }

        public void AddDelayedAction(float p_delay, TimerHolder.OnEndHandler p_function)
        {
            if (p_delay <= 0.0f) return;
            
            TimerHolder l_timer = new TimerHolder() { Duration = p_delay };
            l_timer.OnEnd += p_function;

            l_timer.Start();
            m_timers.Add(l_timer);
        }
        
        #region EVENTS
        public delegate void OnPlayerDeathHandler();
        public delegate void OnGameStateChangeHandler(GameState p_newState, GameState p_oldState);

        public event OnPlayerDeathHandler OnPlayerDeath;
        public event OnGameStateChangeHandler OnGameStateChange;
        #endregion
    }
}
