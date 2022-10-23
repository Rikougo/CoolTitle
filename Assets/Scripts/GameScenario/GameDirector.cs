using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using Character.Player;
using Cinemachine;
using Dialog;
using GameScenario.Utils;
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

        public static GameDirector Instance { get; private set; }

        #region INTERNAL_VAR

        private PlayerInput m_input;
        private DialogDirector m_dialogDirector;
        private Dictionary<int, TimerHolder> m_timers;
        private bool m_started;

        #endregion

        #region EXTERNAL_REF

        #region PLAYER_REF
        private PlayerBrain m_playerBrain;
        private CharacterMovement m_characterMovement;
        private CharacterJump m_characterJump;
        private PlayerAttack m_playerAttack;
        private CharacterRoll m_characterRoll;
        #endregion

        private Volume m_globalVolume;
        private CinemachineBrain m_mainCamera;

        #endregion

        [Header("Assets")] public VolumeProfile defaultVolume;
        public VolumeProfile deathVolume;
        public VolumeProfile dialogVolume;

        private GameState m_state;

        public GameState State
        {
            get => m_state;
            set
            {
                GameState l_oldState = m_state;
                m_state = value;
                GameStateChanged(l_oldState);
            }
        }

        public PlayerInput Input => m_input;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            m_timers = new Dictionary<int, TimerHolder>();
            m_input = GetComponent<PlayerInput>();
            m_dialogDirector = GetComponent<DialogDirector>();

            m_started = false;
        }

        private void Start()
        {
            GameObject l_playerGo = GameObject.FindWithTag("Player");

            if (l_playerGo == null)
            {
                Debug.LogError("Couldn't find Player component in Scene.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            m_playerBrain = l_playerGo.GetComponent<PlayerBrain>();
            m_playerAttack = l_playerGo.GetComponent<PlayerAttack>();
            m_characterJump = l_playerGo.GetComponent<CharacterJump>();
            m_characterRoll = l_playerGo.GetComponent<CharacterRoll>();
            m_characterMovement = l_playerGo.GetComponent<CharacterMovement>();

            m_globalVolume = GameObject.FindWithTag("GlobalVolume").GetComponent<Volume>();
            m_mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<CinemachineBrain>();

            m_started = true;

            OnStartOrEnable();
        }

        private void OnEnable()
        {
            OnStartOrEnable();
        }

        /// <summary>
        /// Called on Start and on OnEnable but only proceed if Start event already got called previously
        /// </summary>
        private void OnStartOrEnable()
        {
            if (m_started)
            {
                m_input.actions["Move"].performed += OnMoveAction;
                m_input.actions["Move"].canceled += OnMoveAction;

                m_input.actions["Jump"].started += OnJumpAction;
                m_input.actions["Jump"].canceled += OnJumpAction;

                m_input.actions["Special"].started += OnRollAction;
                m_input.actions["Special"].canceled += OnRollAction;

                m_input.actions["Interact"].started += OnInteractAction;
            }
        }

        private void OnDisable()
        {
            m_input.actions["Move"].performed -= OnMoveAction;
            m_input.actions["Move"].canceled -= OnMoveAction;

            m_input.actions["Jump"].started -= OnJumpAction;
            m_input.actions["Jump"].canceled -= OnJumpAction;

            m_input.actions["Special"].started -= OnRollAction;
            m_input.actions["Special"].canceled -= OnRollAction;

            m_input.actions["Interact"].started -= OnInteractAction;
        }

        private void OnMoveAction(InputAction.CallbackContext p_ctx)
        {
            switch (m_state)
            {
                case GameState.PLAYING:
                    m_characterMovement.OnMovement(p_ctx);
                    break;
            }
        }

        private void OnJumpAction(InputAction.CallbackContext p_ctx)
        {
            switch (m_state)
            {
                case GameState.PLAYING:
                    m_characterJump.OnJump(p_ctx);
                    break;
                case GameState.DIALOG:
                    if (p_ctx.started) m_dialogDirector.ForwardDialog();
                    break;
            }
        }

        private void OnInteractAction(InputAction.CallbackContext p_ctx)
        {
            switch (m_state)
            {
                case GameState.PLAYING:
                    m_playerAttack.OnStrike(p_ctx);
                    break;
            }
        }

        private void OnRollAction(InputAction.CallbackContext p_ctx)
        {
            switch (m_state)
            {
                case GameState.PLAYING:
                    m_characterRoll.OnRoll(p_ctx);
                    break;
            }
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
            CinemachineVirtualCamera l_currentVirtualCam = m_mainCamera.ActiveVirtualCamera as CinemachineVirtualCamera;

            if (l_currentVirtualCam != null)
            {
                CinemachineBasicMultiChannelPerlin l_noiseComp =
                    l_currentVirtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                if (l_noiseComp != null)
                {
                    l_noiseComp.m_AmplitudeGain = targetAmplitudeGain;

                    this.AddTimer(
                        0.7f,
                        (p_timer, _) =>
                        {
                            float l_invertProgress = 1.0f - p_timer.Progress;
                            float l_progress = Math.Abs(l_invertProgress - 1.0f) < 0.001f
                                ? 1.0f
                                : 1 - Mathf.Pow(2.0f, -10.0f * l_invertProgress);
                            l_noiseComp.m_AmplitudeGain = targetAmplitudeGain * l_progress;
                        },
                        (_) =>
                        {
                            l_noiseComp.m_AmplitudeGain = 0.0f;
                            OnPlayerDeath?.Invoke();
                        });
                }
            }
        }

        private void GameStateChanged(GameState p_oldState)
        {
            OnGameStateChange?.Invoke(m_state, p_oldState);

            switch (m_state)
            {
                case GameState.DIALOG:
                    m_globalVolume.sharedProfile = dialogVolume;
                    break;
                default:
                    m_globalVolume.sharedProfile = defaultVolume;
                    break;
            }

            ;
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