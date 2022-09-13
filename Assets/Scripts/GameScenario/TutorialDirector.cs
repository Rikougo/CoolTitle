using Dialog.Runtime;
using Dialog.Runtime.Assets;
using UnityEngine;


namespace GameScenario
{
    [RequireComponent(typeof(GameDirector), typeof(DialogDirector))]
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
            
            m_gameDirector.OnPlayerDeath += () =>
            {
                m_gameDirector.AddDelayedAction(1.5f, (_) =>
                {
                    m_dialogDirector.SubmitDialog(deathDialogItem);
                });
            };
        }
    }
}