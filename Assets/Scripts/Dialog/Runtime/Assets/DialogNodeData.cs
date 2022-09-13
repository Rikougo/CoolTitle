using System;
using UnityEngine;

namespace Dialog.Editor.Assets
{
    [Serializable]
    public class DialogNodeData
    {
        public bool EntryPoint;
        public string NodeGUID;
        public string DialogueText;
        public Vector2 Position;
    }
}