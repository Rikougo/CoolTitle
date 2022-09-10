using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using GameScenario.Dialog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GameScenario
{
    [RequireComponent(typeof(GameDirector), typeof(DialogDirector))]
    public class TutorialDirector : MonoBehaviour
    {
        private GameDirector m_gameDirector;
        private DialogDirector m_dialogDirector;

        public void Start()
        {
            m_gameDirector = GetComponent<GameDirector>();
            m_dialogDirector = GetComponent<DialogDirector>();
            m_gameDirector.OnPlayerDeath += () =>
            {
                m_gameDirector.AddDelayedAction(1.5f, (_) =>
                {
                    m_dialogDirector.SubmitDialog("Hum... c'est ça de toucher a tout et n'importe quoi. Tu apprendra" +
                                                  " qu'ici il faut faire attention.");
                });
            };
        }
    }
}