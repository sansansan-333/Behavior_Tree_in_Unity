using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    [System.Serializable]
    public class TreeContainer : ScriptableObject
    {
        public List<NodeData> NodeDatas = new List<NodeData>();
        public List<LinkData> LinkDatas = new List<LinkData>();
    }
}