using Animations.Text;
using UnityEngine;

namespace Interactions.Props.Tutorial
{
    [RequireComponent(typeof(AreaActionable))]
    public class DeathButton : MonoBehaviour
    {
        private AreaActionable m_actionable;
        [SerializeField] private WavingText m_textObject;

        private void Start()
        {
            m_actionable = GetComponent<AreaActionable>();
            m_textObject.gameObject.SetActive(false);

            m_actionable.OnPlayerEnter += PlayTextAnimation;
            m_actionable.OnPlayerExit += EndTextAnimation;
        }
        
        private void OnDisable()
        {
            m_actionable.OnPlayerEnter -= PlayTextAnimation;
            m_actionable.OnPlayerExit -= EndTextAnimation;
        }

        private void PlayTextAnimation()
        {
            m_textObject.gameObject.SetActive(true);
            m_textObject.PlayAnimation();
        }

        private void EndTextAnimation()
        {
            m_textObject.gameObject.SetActive(false);
            m_textObject.StopAnimation();
        }
    }
}