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

        [MenuItem("Graph/DialogGraph")]
        public static void CreateGraphViewWindow()
        {
            DialogGraph l_window = GetWindow<DialogGraph>();
            l_window.titleContent = new GUIContent("DialogGraph");
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
            // GenerateMiniMap();
        }

        private void GenerateMiniMap()
        {
            MiniMap l_miniMap = new MiniMap {anchored = true};
            Vector2 l_cords = m_graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
            l_miniMap.SetPosition(new Rect(l_cords.x, l_cords.y, 200, 140));
            m_graphView.Add(l_miniMap);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(m_graphView);
        }
    }
}