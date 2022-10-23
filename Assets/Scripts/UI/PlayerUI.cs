using Character.Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerUI : MonoBehaviour
    {
        private PlayerBrain m_playerBrain;

        [Header("UI Components")]
        [SerializeField] private Image m_healthFill;

        private void Start()
        {
            m_playerBrain = GameObject.FindWithTag("Player").GetComponent<PlayerBrain>();
            m_playerBrain.OnHealthUpdate.AddListener(PlayerHealthUpdate);
        }

        private void PlayerHealthUpdate(float p_currentHealth, float p_currentHealthPercent)
        {
            m_healthFill.fillAmount = p_currentHealthPercent;
        }
    }
}
