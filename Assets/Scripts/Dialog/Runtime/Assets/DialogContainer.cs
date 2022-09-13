using System;
using System.Collections.Generic;
using System.Linq;
using Dialog.Editor.Assets;
using JetBrains.Annotations;
using UnityEngine;

namespace Dialog.Runtime.Assets
{
    [Serializable]
    public class DialogContainer : ScriptableObject
    {
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public List<DialogNodeData> DialogueNodeData = new List<DialogNodeData>();
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
        public List<CommentBlockData> CommentBlockData = new List<CommentBlockData>();

        public DialogNodeData EntryPoint => DialogueNodeData.First(p_dialogData => p_dialogData.EntryPoint);

        public List<NodeLinkData> GetChoices(DialogNodeData p_dialog)
        {
            return NodeLinks.Where(p_edge => p_edge.BaseNodeGUID == p_dialog.NodeGUID).ToList();
        }
    }
}