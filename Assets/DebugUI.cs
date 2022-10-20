using System.Collections;
using System.Collections.Generic;
using GameScenario;
using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    private GameDirector m_director;

    [Header("UI Components")] 
    [SerializeField] private TextMeshProUGUI m_lastStateText;

    [SerializeField] private TextMeshProUGUI m_stateText;
    [SerializeField] private TextMeshProUGUI m_fpsText;
    
    private void Start()
    {
        m_director = GameObject.FindWithTag("GameDirector").GetComponent<GameDirector>();

        m_director.OnGameStateChange += (p_newState, p_oldState) =>
        {
            m_lastStateText.text = p_oldState.ToString();
            m_stateText.text = p_newState.ToString();
        };
    }

    private void Update()
    {
        m_fpsText.text = $"{1.0f / Time.deltaTime:F2}";
    }
}
