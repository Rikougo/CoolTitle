using System;
using System.Collections;
using System.Collections.Generic;
using Dialog.Editor.Node;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialog.Editor.Graph
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow m_window;
        private DialogGraphView m_graphView;

        private Texture2D m_indentationIcon;

        public void Configure(EditorWindow p_window, DialogGraphView p_graphView)
        {
            m_window = p_window;
            m_graphView = p_graphView;

            //Transparent 1px indentation icon as a hack
            m_indentationIcon = new Texture2D(1, 1);
            m_indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            m_indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
                new SearchTreeEntry(new GUIContent("Dialogue Node", m_indentationIcon))
                {
                    level = 2, userData = new DialogNode()
                },
                new SearchTreeEntry(new GUIContent("Comment Block", m_indentationIcon))
                {
                    level = 1,
                    userData = new Group()
                }
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            //Editor window-based mouse position
            var mousePosition = m_window.rootVisualElement.ChangeCoordinatesTo(m_window.rootVisualElement.parent,
                context.screenMousePosition - m_window.position.position);
            var graphMousePosition = m_graphView.contentViewContainer.WorldToLocal(mousePosition);
            switch (SearchTreeEntry.userData)
            {
                case DialogNode dialogueNode:
                    m_graphView.CreateNewDialogueNode("Dialogue Node", graphMousePosition);
                    return true;
                case Group group:
                    var rect = new Rect(graphMousePosition, m_graphView.DefaultCommentBlockSize);
                    m_graphView.CreateCommentBlock(rect);
                    return true;
            }

            return false;
        }
    }
}