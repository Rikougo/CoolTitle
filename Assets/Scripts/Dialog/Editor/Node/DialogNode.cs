using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dialog.Editor.Node
{
    public class DialogNode : UnityEditor.Experimental.GraphView.Node
    {
        public string DialogueText;
        public string GUID;
        public bool EntryPoint = false;
    }
}