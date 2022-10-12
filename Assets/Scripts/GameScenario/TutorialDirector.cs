using CheapDialogSystem.Runtime.Assets;
using Dialog;
using UnityEngine;

namespace GameScenario
{
    [RequireComponent(typeof(GameDirector))]
    public class TutorialDirector : MonoBehaviour
    {
        private GameDirector m_gameDirector;
        private DialogDirector m_dialogDirector;

        public DialogContainer startDialogItem;
        public DialogContainer deathDialogItem;

        public void Start()
        {
            m_gameDirector = GetComponent<GameDirector>();
            m_dialogDirector = GetComponent<DialogDirector>();
            m_dialogDirector.SubmitDialog(startDialogItem);
        }
    }
}