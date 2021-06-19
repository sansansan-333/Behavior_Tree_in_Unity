using System;
using UnityEngine;

namespace BehaviorTree
{

    /// <summary>
    /// Holding every node data.
    /// </summary>
    [Serializable]
    public class NodeData
    {
        public string GUID;
        public string nodeType;
        public Vector2 position;
        public ActionNodeData actionNodeData;
        public ConditionalNodeData conditionalNodeData;
        public RepeaterNodeData repeaterNodeData;

        [Serializable]
        public class ActionNodeData
        {
            public string act;
        }

        [Serializable]
        public class ConditionalNodeData
        {
            public string condition;
            public bool value;
        }

        [Serializable]
        public class RepeaterNodeData
        {
            public int repeatCount;
        }
    }
}