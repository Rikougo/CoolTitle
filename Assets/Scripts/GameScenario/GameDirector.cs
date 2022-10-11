using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Dialog;
using GameScenario.Utils;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GameScenario
{
    [RequireComponent(typeof(PlayerInput))]
    public class GameDirector : MonoBehaviour
    {
        public enum GameState
        {
            PLAYING,
            DIALOG,
            PAUSE
        }

        private PlayerInput m_input;
        private DialogDirector m_dialogDirector;

        private PlayerMovement m_playerMovement;
        private PlayerJump m_playerJump;
        private PlayerAttack m_playerAttack;
        private Camera m_camera;
        private Volume m_globalVolume;

        private Dictionary<int, TimerHolder> m_timers;
        private bool m_started;

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

        public PlayerInput Input => m_input;

        private void Awake()
        {
            m_timers = new Dictionary<int, TimerHolder>();
            m_input = GetComponent<PlayerInput>();
            m_dialogDirector = GetComponent<DialogDirector>();

            m_started = false;
        }

        private void Start()
        {
            GameObject l_playerGO = GameObject.FindWithTag("Player");
            m_playerMovement = l_playerGO.GetComponent<PlayerMovement>();
            m_playerJump = l_playerGO.GetComponent<PlayerJump>();
            m_playerAttack = l_playerGO.GetComponent<PlayerAttack>();
            m_camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            m_globalVolume = GameObject.FindWithTag("GlobalVolume").GetComponent<Volume>();

            if (m_playerMovement is null)
            {
                Debug.LogError("Couldn't find Player component in Scene.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            m_started = true;

            OnStartOrEnable();
        }

        private void OnEnable()
        {
            OnStartOrEnable();
        }

        private void OnStartOrEnable()
        {
            if (m_started)
            {
                m_input.actions["Move"].performed += m_playerMovement.OnMovement;
                m_input.actions["Move"].canceled += m_playerMovement.OnMovement;

                m_input.actions["Jump"].started += m_playerJump.OnJump;
                m_input.actions["Jump"].canceled += m_playerJump.OnJump;
                
                // m_input.actions["Restart"].performed += (ctx) => ResurrectPlayer();
                m_input.actions["Interact"].started += (ctx) =>
                {
                    switch (m_state)
                    {
                        case GameState.PLAYING:
                            m_playerAttack.OnStrike(ctx);
                            break;
                        case GameState.DIALOG:
                            m_dialogDirector.ForwardDialog();
                            break;
                    }
                };

                /*m_input.actions["VerticalMove"].performed += (ctx) =>
                {
                    switch (m_state)
                    {
                        case GameState.DIALOG:
                            float l_direction = ctx.ReadValue<float>();
                            m_dialogDirector.SelectedChoices = m_dialogDirector.SelectedChoices + Mathf.RoundToInt(l_direction);
                            break;
                    }
                };*/
            }
        }

        private void OnDisable()
        {
            m_input.actions["Move"].performed -= m_playerMovement.OnMovement;
            m_input.actions["Move"].canceled -= m_playerMovement.OnMovement;
        }

        private void Update()
        {
            TickTimers();
        }

        public void KillPlayer()
        {
            // m_player.Die();
            m_globalVolume.profile = deathVolume;

            const float targetAmplitudeGain = 5.0f;
            defaultCameraBrain.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain =
                targetAmplitudeGain;

            TimerHolder l_shakeTimer = new TimerHolder() { Duration = 0.7f };
            l_shakeTimer.OnUpdate += (TimerHolder p_timer, float p_deltaTime) =>
            {
                float l_invertProgress = 1.0f - p_timer.Progress;
                float l_coef = Math.Abs(l_invertProgress - 1.0f) < 0.001f
                    ? 1.0f
                    : 1 - Mathf.Pow(2.0f, -10.0f * l_invertProgress);
                defaultCameraBrain.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain =
                    targetAmplitudeGain * l_coef;
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
            // m_player.Resurrect();
        }

        #region TIMERS

        private void TickTimers()
        {
            List<int> l_toDelete = new List<int>();
            List<int> l_keys = m_timers.Keys.ToList();
            for (int l_index = 0; l_index < l_keys.Count; l_index++)
            {
                int l_key = l_keys[l_index];
                TimerHolder l_timer = m_timers[l_key];
                if (!l_timer.UpdateTimer(Time.deltaTime))
                {
                    l_toDelete.Add(l_key);
                }
            }

            for (int l_index = 0; l_index < l_toDelete.Count; l_index++)
            {
                m_timers.Remove(l_toDelete[l_index]);
            }
        }

        public bool HasTimer(int p_id)
        {
            return m_timers.ContainsKey(p_id);
        }

        public void EndTimer(int p_id)
        {
            if (HasTimer(p_id))
            {
                TimerHolder l_timer = m_timers[p_id];
                l_timer.End();
            }
        }

        public int AddTimer(TimerHolder p_timer)
        {
            if (p_timer.Started || p_timer.Ended)
            {
                Debug.LogWarning("Registering an already started Timer.");
            }

            p_timer.Start();
            int l_id = Mathf.RoundToInt(Time.time * 100);
            m_timers.Add(l_id, p_timer);

            return l_id;
        }

        public int AddTimer(
            float p_duration,
            TimerHolder.OnUpdateHandler p_updateFunc,
            TimerHolder.OnEndHandler p_endFunc)
        {
            TimerHolder l_timer = new TimerHolder() { Duration = p_duration };
            l_timer.OnUpdate += p_updateFunc;
            l_timer.OnEnd += p_endFunc;

            return this.AddTimer(l_timer);
        }

        public int AddDelayedAction(float p_delay, TimerHolder.OnEndHandler p_function)
        {
            if (p_delay <= 0.0f) return -1;

            TimerHolder l_timer = new TimerHolder() { Duration = p_delay };
            l_timer.OnEnd += p_function;

            return AddTimer(l_timer);
        }

        #endregion

        #region EVENTS

        public delegate void OnPlayerDeathHandler();

        public delegate void OnGameStateChangeHandler(GameState p_newState, GameState p_oldState);

        public event OnPlayerDeathHandler OnPlayerDeath;
        public event OnGameStateChangeHandler OnGameStateChange;

        #endregion
    }
}