using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dialog.Runtime.Assets;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialog.Editor.Graph
{
    public class DialogGraph : EditorWindow
    {
        private string m_fileName = "New Narrative";

        private DialogGraphView m_graphView;
        private DialogContainer m_dialogueContainer;

        [MenuItem("Graph/Narrative Graph")]
        public static void CreateGraphViewWindow()
        {
            var window = GetWindow<DialogGraph>();
            window.titleContent = new GUIContent("Narrative Graph");
        }

        private void ConstructGraphView()
        {
            m_graphView = new DialogGraphView(this)
            {
                name = "Narrative Graph",
            };
            m_graphView.StretchToParentSize();
            rootVisualElement.Add(m_graphView);
        }

        private void GenerateToolbar()
        {
            var l_toolbar = new Toolbar();

            var l_fileNameTextField = new TextField("File Name:");
            l_fileNameTextField.SetValueWithoutNotify(m_fileName);
            l_fileNameTextField.MarkDirtyRepaint();
            l_fileNameTextField.RegisterValueChangedCallback(p_evt => m_fileName = p_evt.newValue);
            l_toolbar.Add(l_fileNameTextField);
            l_toolbar.Add(new Button(() => RequestDataOperation(true)) {text = "Save Data"});
            l_toolbar.Add(new Button(() => RequestDataOperation(false)) {text = "Load Data"});
            l_toolbar.Add(new Button(() => m_graphView.CreateNewDialogueNode("Dialogue Node", Vector2.zero)) {text = "New Node",});
            rootVisualElement.Add(l_toolbar);
        }

        private void RequestDataOperation(bool p_save)
        {
            if (!string.IsNullOrEmpty(m_fileName))
            {
                var l_saveUtility = DialogSaveUtility.GetInstance(m_graphView);
                if (p_save)
                    l_saveUtility.SaveGraph(m_fileName);
                else
                    l_saveUtility.LoadGraph(m_fileName);
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid File name", "Please Enter a valid filename", "OK");
            }
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            GenerateMiniMap();
            // GenerateBlackBoard();
        }

        private void GenerateMiniMap()
        {
            MiniMap l_miniMap = new MiniMap {anchored = true};
            Vector2 l_cords = m_graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
            l_miniMap.SetPosition(new Rect(l_cords.x, l_cords.y, 200, 140));
            m_graphView.Add(l_miniMap);
        }

        private void GenerateBlackBoard()
        {
            var l_blackboard = new Blackboard(m_graphView);
            /*l_blackboard.Add(new BlackboardSection {title = "Exposed Variables"});
            l_blackboard.addItemRequested = _blackboard =>
            {
                m_graphView.AddPropertyToBlackBoard(ExposedProperty.CreateInstance(), false);
            };
            l_blackboard.editTextRequested = (_blackboard, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField) element).text;
                if (m_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one.",
                        "OK");
                    return;
                }

                var targetIndex = m_graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
                m_graphView.ExposedProperties[targetIndex].PropertyName = newValue;
                ((BlackboardField) element).text = newValue;
            };*/
            l_blackboard.SetPosition(new Rect(10,30,200,300));
            m_graphView.Add(l_blackboard);
            m_graphView.Blackboard = l_blackboard;
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(m_graphView);
        }
    }
}