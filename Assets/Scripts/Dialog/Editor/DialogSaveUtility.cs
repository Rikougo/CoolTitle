using System.Collections.Generic;
using System.Linq;
using Dialog.Editor.Assets;
using Dialog.Editor.Graph;
using Dialog.Editor.Node;
using Dialog.Runtime.Assets;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialog.Editor
{
    public class DialogSaveUtility
    {
        private DialogGraphView m_targetGraphView;
        private DialogContainer m_dialogContainer;

        private List<Edge> Edges => m_targetGraphView.edges.ToList();
        private List<DialogNode> Nodes => m_targetGraphView.nodes.ToList().Cast<DialogNode>().ToList();
        private List<Group> CommentBlocks => 
            m_targetGraphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();

        public static DialogSaveUtility GetInstance(DialogGraphView p_target)
        {
            return new DialogSaveUtility()
            {
                m_targetGraphView = p_target
            };
        }

        public void SaveGraph(string p_filePath)
        {
            if (!Edges.Any()) return;

            DialogContainer l_container = ScriptableObject.CreateInstance<DialogContainer>();

            Edge[] l_connectedPorts = Edges.Where(p_edge => p_edge.input.node != null).ToArray();

            for (int i = 0; i < l_connectedPorts.Length; i++)
            {
                DialogNode l_outputNode = l_connectedPorts[i].output.node as DialogNode;
                DialogNode l_inputNode = l_connectedPorts[i].input.node as DialogNode;

                l_container.NodeLinks.Add(new NodeLinkData()
                {
                    BaseNodeGUID = l_outputNode.GUID,
                    PortName = l_connectedPorts[i].output.portName,
                    TargetNodeGUID = l_inputNode.GUID
                });
            }

            foreach (DialogNode l_node in Nodes.Where(p_node => !p_node.EntryPoint))
            {
                bool l_isEntryPoint = Edges
                    .Where(p_edge => (p_edge.input.node as DialogNode)?.GUID == l_node.GUID)
                    .Any(p_edge => (p_edge.output.node as DialogNode)?.EntryPoint ?? false);
                
                l_container.DialogueNodeData.Add(new DialogNodeData()
                {
                    NodeGUID = l_node.GUID,
                    DialogueText = l_node.DialogueText,
                    Position = l_node.GetPosition().position,
                    EntryPoint = l_isEntryPoint
                });
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogs"))
                AssetDatabase.CreateFolder("Assets/Resources", "Dialogs");
            
            AssetDatabase.CreateAsset(l_container, $"Assets/Resources/Dialogs/{p_filePath}.asset");
            AssetDatabase.SaveAssets();
        }

        public void LoadGraph(string p_filePath)
        {
            m_dialogContainer = Resources.Load<DialogContainer>($"Dialogs/{p_filePath}");
            if (m_dialogContainer == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target Narrative Data does not exist!", "OK");
                return;
            }
            
            ClearGraph();
            GenerateDialogueNodes();
            ConnectDialogueNodes();
            AddExposedProperties();
            GenerateCommentBlocks();
        }
        
        /// <summary>
        /// Set Entry point GUID then Get All Nodes, remove all and their edges. Leave only the entrypoint node. (Remove its edge too)
        /// </summary>
        private void ClearGraph()
        {
            Nodes.Find(x => x.EntryPoint).GUID = m_dialogContainer.NodeLinks[0].BaseNodeGUID;
            foreach (var l_perNode in Nodes)
            {
                if (l_perNode.EntryPoint) continue;
                Edges.Where(x => x.input.node == l_perNode).ToList()
                    .ForEach(edge => m_targetGraphView.RemoveElement(edge));
                m_targetGraphView.RemoveElement(l_perNode);
            }
        }
        
        /// <summary>
        /// Create All serialized nodes and assign their guid and dialogue text to them
        /// </summary>
        private void GenerateDialogueNodes()
        {
            foreach (var l_perNode in m_dialogContainer.DialogueNodeData)
            {
                var l_tempNode = m_targetGraphView.CreateNode(l_perNode.DialogueText, Vector2.zero);
                l_tempNode.GUID = l_perNode.NodeGUID;
                m_targetGraphView.AddElement(l_tempNode);

                var l_nodePorts = m_dialogContainer.NodeLinks.Where(x => x.BaseNodeGUID == l_perNode.NodeGUID).ToList();
                l_nodePorts.ForEach(x => m_targetGraphView.AddChoicePort(l_tempNode, x.PortName));
            }
        }
        
        private void ConnectDialogueNodes()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var k = i; //Prevent access to modified closure
                var l_connections = m_dialogContainer.NodeLinks.Where(x => x.BaseNodeGUID == Nodes[k].GUID).ToList();
                for (var j = 0; j < l_connections.Count(); j++)
                {
                    var l_targetNodeGuid = l_connections[j].TargetNodeGUID;
                    var l_targetNode = Nodes.First(x => x.GUID == l_targetNodeGuid);
                    LinkNodesTogether(Nodes[i].outputContainer[j].Q<Port>(), (Port) l_targetNode.inputContainer[0]);

                    l_targetNode.SetPosition(new Rect(
                        m_dialogContainer.DialogueNodeData.First(x => x.NodeGUID == l_targetNodeGuid).Position,
                        m_targetGraphView.DefaultNodeSize));
                }
            }
        }
        
        private void LinkNodesTogether(Port p_outputSocket, Port p_inputSocket)
        {
            var l_tempEdge = new Edge()
            {
                output = p_outputSocket,
                input = p_inputSocket
            };
            l_tempEdge?.input.Connect(l_tempEdge);
            l_tempEdge?.output.Connect(l_tempEdge);
            m_targetGraphView.Add(l_tempEdge);
        }
        
        private void AddExposedProperties()
        {
            m_targetGraphView.ClearBlackBoardAndExposedProperties();
            foreach (var l_exposedProperty in m_dialogContainer.ExposedProperties)
            {
                m_targetGraphView.AddPropertyToBlackBoard(l_exposedProperty);
            }
        }

        private void GenerateCommentBlocks()
        {
            foreach (var l_commentBlock in CommentBlocks)
            {
                m_targetGraphView.RemoveElement(l_commentBlock);
            }

            foreach (var l_commentBlockData in m_dialogContainer.CommentBlockData)
            {
               var l_block = m_targetGraphView.CreateCommentBlock(new Rect(l_commentBlockData.Position, m_targetGraphView.DefaultCommentBlockSize),
                    l_commentBlockData);
               l_block.AddElements(Nodes.Where(x=>l_commentBlockData.ChildNodes.Contains(x.GUID)));
            }
        }
    }
}