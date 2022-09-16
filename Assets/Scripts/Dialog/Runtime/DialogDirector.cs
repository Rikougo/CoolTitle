using System;
using System.Collections.Generic;
using System.Linq;
using Animations.Text;
using Dialog.Editor.Assets;
using Dialog.Runtime.Assets;
using GameScenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialog.Runtime
{
    [RequireComponent(typeof(GameDirector))]
    public class DialogDirector : MonoBehaviour
    {
        [SerializeField] private Image m_dialogBox;
        [SerializeField] private TextMeshProUGUI m_dialogText;
        [SerializeField] private Image m_authorFrame;
        [SerializeField] private TextMeshProUGUI m_authorText;
        [SerializeField] private GameObject m_choicesLayout;
        [SerializeField] private Button m_choicePrefab;

        [SerializeField] private float m_letterPerSecond = 5.0f;
        private float m_secondPerLetter;

        private GameDirector m_gameDirector;

        private DialogContainer m_currentContainer;
        private DialogNodeData m_currentNode;
        private List<NodeLinkData> m_currentChoices;
        private int m_choicesCursor;
        private int m_currentTimer;

        public int SelectedChoices
        {
            get => m_choicesCursor;
            set
            {
                Destroy(m_choicesLayout.transform.GetChild(m_choicesCursor).gameObject.GetComponent<WavingText>());
                m_choicesCursor = value % m_currentChoices.Count;
                m_choicesLayout.transform.GetChild(m_choicesCursor).gameObject.AddComponent<WavingText>().PlayAnimation();
            }
        }

        public delegate void OnDialogOpenHandler();

        public delegate void OnDialogCloseHandler();

        public event OnDialogOpenHandler OnDialogOpen;
        public event OnDialogCloseHandler OnDialogClose;

        private void Awake()
        {
            m_gameDirector = GetComponent<GameDirector>();
            m_secondPerLetter = 1.0f / m_letterPerSecond;
            m_currentTimer = -1;
        }

        private void Start()
        {
            if (m_currentContainer is null)
            {
                m_dialogText.text = String.Empty;
                m_dialogBox.gameObject.SetActive(false);
            }
        }

        public void SubmitDialog(DialogContainer p_container)
        {
            if (m_gameDirector.State is GameDirector.GameState.DIALOG) return;

            OnDialogOpen?.Invoke();
            m_gameDirector.State = GameDirector.GameState.DIALOG;

            m_currentContainer = p_container;
            m_currentNode = m_currentContainer.EntryPoint;
            m_currentChoices = m_currentContainer.GetChoices(m_currentNode);
            m_choicesCursor = 0;

            StartWriting();
        }

        public void ForwardDialog()
        {
            // Dialog is currently writing, skip writing phase
            if (m_currentTimer != -1)
            {
                m_gameDirector.EndTimer(m_currentTimer);
                return;
            }

            // No choices then it was last dialog
            if (m_currentChoices.Count == 0)
            {
                EndDialog();
                return;
            }

            if (m_choicesCursor >= m_currentChoices.Count)
            {
                Debug.LogError("Choices selection were out of choices bounds (taking last).");
                m_choicesCursor = m_currentChoices.Count - 1;
            }

            try
            {
                m_currentNode = m_currentContainer.DialogueNodeData
                    .First(p_dialog => p_dialog.NodeGUID == m_currentChoices[m_choicesCursor].TargetNodeGUID);
            }
            catch (InvalidOperationException l_exception)
            {
                Debug.LogError($"Next choices weren't found (end dialog).\n ${l_exception.Message}");
                EndDialog();
                return;
            }

            m_currentChoices = m_currentContainer.GetChoices(m_currentNode);
            m_choicesCursor = 0;
            StartWriting();
        }

        private void EndDialog()
        {
            m_gameDirector.State = GameDirector.GameState.PLAYING;
            OnDialogClose?.Invoke();
            m_dialogText.text = String.Empty;
            m_authorText.text = String.Empty;
            m_dialogBox.gameObject.SetActive(false);
            m_authorFrame.gameObject.SetActive(false);
        }

        private void StartWriting()
        {
            m_dialogText.text = String.Empty;
            m_dialogBox.gameObject.SetActive(true);
            m_choicesLayout.gameObject.SetActive(false);

            m_currentTimer = m_gameDirector.AddTimer(m_currentNode.DialogueText.Length * m_secondPerLetter,
                (p_timer, p_deltaTime) =>
                {
                    string l_contentSlice = m_currentNode.DialogueText.Substring(0,
                        (int)(p_timer.Progress * m_currentNode.DialogueText.Length));
                    m_dialogText.text = l_contentSlice;
                },
                (_) =>
                {
                    m_dialogText.text = m_currentNode.DialogueText;
                    m_currentTimer = -1;
                    
                    if (m_currentChoices.Count > 1) ShowChoices();
                });
        }

        private void ShowChoices()
        {
            m_choicesLayout.gameObject.SetActive(true);
            int l_index = 0;
            for (; l_index < m_choicesLayout.transform.childCount; l_index++)
            {
                GameObject.Destroy(m_choicesLayout.transform.GetChild(l_index).gameObject);
            }

            l_index = 0;
            foreach (NodeLinkData l_choice in m_currentChoices)
            {
                Button l_choiceButton = Instantiate(m_choicePrefab, m_choicesLayout.transform);
                TextMeshProUGUI l_choiceText = l_choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                l_choiceText.text = l_choice.PortName;

                if (m_choicesCursor == l_index)
                {
                    l_choiceText.gameObject.AddComponent<WavingText>().PlayAnimation();
                }

                int l_captureIndex = l_index;
                l_choiceButton.onClick.AddListener(() =>
                {
                    m_choicesCursor = l_captureIndex;
                    ForwardDialog();
                });

                l_index++;
            }
        }
    }
}